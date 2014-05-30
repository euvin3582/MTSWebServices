using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using DataLayer.domains;
using iTraycerSection.Invoice;
using System.Drawing;
using iTraycerSection.Domain;
using System.Data;
using System.Text;
using iTraycerSection.Case;
using iTraycerSection.Conflict;
using iTraycerSection.Session;

namespace FacadeRestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FacadeRestServiceImpl" in code, svc and config file together.
    public class FacadeRestServiceImpl : IFacadeRestServiceImpl
    {
        public string CreateSession()
        {
            string bodyRequest = OperationContext.Current.RequestContext.RequestMessage.ToString();
           
            XmlDocument payload = new XmlDocument();
            payload.LoadXml(bodyRequest);
            
            // deserialize payload
            JsonEnvelope requestEnvelope = new JsonEnvelope();

            XmlSerializer xmlSer = new XmlSerializer(typeof(JsonEnvelope));
            requestEnvelope = (JsonEnvelope)xmlSer.Deserialize(new StringReader(payload.OuterXml));

            // build response envelope
            JsonEnvelope responseEnvelope = new JsonEnvelope();
            XmlNode[] serviceQueueNodes = null;
            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString("o");

            // check to see if the appLaunchCount is present, if it is convert it to an int and see if its greater than 1
            if (((!String.IsNullOrEmpty(requestEnvelope.AppLaunchCount)) ? Convert.ToInt32(requestEnvelope.AppLaunchCount) : 1) > 1)
            {
                // changes the current ServiceQueues array and adds the new one to it with the missing Sync Elements
                requestEnvelope.ServiceQueues[0] = iTraycerSection.InitData.InitData.AddSyncObjects(payload, requestEnvelope);
            }

            // create service name list to access in switch (needs to be done after the new objects are added to it)
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                serviceQueueNodes = (XmlNode[])requestEnvelope.ServiceQueues[0];
            }

            if(serviceQueueNodes.Length > 1)
            {
                // create response dictionary and create response envelope
                Dictionary<object, string> resp = null;
                responseEnvelope.Response = new List<object>();

                for (int i = 0; i < serviceQueueNodes.Length-1; i++)
                {
                    // creat the payload child
                    XmlDocument payloadChild = new XmlDocument();

                    // The try catch will catch bad json envelope request and create an error log for end user
                    try
                    {
                        payloadChild.LoadXml(serviceQueueNodes[i + 1].ChildNodes[0].OuterXml);
                    }
                    catch (Exception e)
                    {
                        resp = new Dictionary<object, string>();
                        resp.Add("SRVERROR", "Service request objects envelope does not match. StackTrace: " + e.StackTrace);
                        responseEnvelope.Response.Add(resp);
                        continue;
                    }

                    // gets the service name from the object
                    string data = null;
                    string serviceName = payloadChild.DocumentElement.Name;
                    string childInnerText = payloadChild.DocumentElement.InnerText;
                    XmlNodeList authObject = payload.SelectNodes("/root/ServiceQueue/item/MTSMobileAuth"); 

                    if (String.IsNullOrEmpty(serviceName))
                    {
                        resp = new Dictionary<object, string>();
                        resp.Add("SRVERROR", "No service name was specified");
                        responseEnvelope.Response.Add(resp);
                    }

                    bool validToken  = !String.IsNullOrEmpty(requestEnvelope.MtsToken) || !String.IsNullOrEmpty(responseEnvelope.MtsToken);

                    // First check if valid token exist
                    if (validToken)
                    {
                        bool sessionValid = Session.ValidateSession(String.IsNullOrEmpty(requestEnvelope.MtsToken) ? responseEnvelope.MtsToken : requestEnvelope.MtsToken);

                        if (!sessionValid)
                        {
                            resp = new Dictionary<object, string>();
                            resp.Add(serviceName, (Session.errorMessage == null) ? "Session Expired" : Session.errorMessage);
                            responseEnvelope.Response.Add(resp);
                            break;
                        }
                    }
                    else if (authObject.Count == 0)
                    {
                        resp = new Dictionary<object, string>();
                        resp.Add("SRVERROR", "MTSMobileAuth was not sent or a valid authorization token found");
                        responseEnvelope.Response.Add(resp);
                        break;
                    }

                    switch (serviceName)
                    {
                        case "MTSMobileAuth":
                            XmlNode email = payloadChild.SelectSingleNode("//Email");
                            XmlNode password = payloadChild.SelectSingleNode("//Password");
                            resp = new Dictionary<object, string>();
                            string[] authResponse = new string[3];

                            // validate the user
                            if (email != null && password != null)
                                authResponse = Session.CreateUserSession(email.InnerText, password.InnerText, requestEnvelope.DevID);

                            if (authResponse != null)
                            {
                                responseEnvelope.CoID = authResponse[0];
                                responseEnvelope.RepID = authResponse[1];
                                responseEnvelope.MtsToken = authResponse[2];
                                resp.Add(serviceName, "Successfully authenticated user");
                                responseEnvelope.Commit = "true";
                            }
                            else
                            {
                                resp.Add(serviceName, Session.errorMessage);
                                responseEnvelope.Commit = "false";
                                responseEnvelope.Response.Add(resp);
                                goto stopProcessing;
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "MobileDeviceRegister":
                            iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                            XmlNode DeviceId = payloadChild.SelectSingleNode("//DeviceID");
                            XmlNode DeviceOsVersion = payloadChild.SelectSingleNode("//DeviceOSVersion");
                            XmlNode DevicePlatform = payloadChild.SelectSingleNode("//DevicePlatform");
                            resp = new Dictionary<object, string>();
                            
                            // validate all data first
                            if (DeviceId != null || DeviceOsVersion != null || DevicePlatform != null){
                                device.DeviceId = DeviceId.InnerText;
                                device.DeviceOsVersion = DeviceOsVersion.InnerText;
                                device.Platform = DevicePlatform.InnerText;
                            } else{
                                resp.Add(serviceName, "SRVERROR:Fail to provide device info: DeviceId, DeviceOsVersion or DevicePlatform");
                                responseEnvelope.Response.Add(resp);
                                responseEnvelope.Commit = "false";
                                break;
                            }

                            if (!iTraycerSection.Device.Device.CheckIfExist(device.DeviceId))
                            {
                                if (iTraycerSection.Device.Device.AddDeviceInfo(device))
                                {
                                    // create the application object info
                                    iTraycerApplication ita = new iTraycerApplication();
                                    // create iTraycerApplication object
                                    ita.RepId = Session.userInfo.Id;
                                    ita.CoId = Session.userInfo.CustomerId;
                                    ita.CreatedDate = DateTime.UtcNow;
                                    ita.LastSync = DateTime.UtcNow;
                                    ita.DeviceId = device.DeviceId;
                                    ita.LaunchCount = Convert.ToInt32(requestEnvelope.AppLaunchCount);

                                    if(iTraycerSection.Device.Device.AddApplicationInfo(ita)){
                                        resp.Add(serviceName, "Successfully resgistered device");
                                        responseEnvelope.Response.Add(resp);
                                        responseEnvelope.Commit = "true";
                                        break;
                                    }
                                }
                                else
                                {
                                    resp.Add(serviceName, "SRVERROR:Failed to register device");
                                    responseEnvelope.Response.Add(resp);
                                    responseEnvelope.Commit = "false";
                                    break;
                                }   
                            }
                            else
                            {
                                if (!Session.UpdateApplicationInfo(Session.userInfo, DeviceId.InnerText))
                                    resp.Add(serviceName, "SRVERROR:Device already registered, information updated");
                                else
                                    resp.Add(serviceName, "SRVERROR:Fail to update Device Info");
                                responseEnvelope.Response.Add(resp);
                                responseEnvelope.Commit = "false";
                                break;
                            }

                            if (iTraycerSection.Validation.Validate.ValidateApplicationDeviceInfo(Session.userInfo.Id, Session.userInfo.CustomerId, device, Convert.ToInt32(requestEnvelope.AppLaunchCount)))
                            {
                                resp.Add(serviceName, "Successfully validate device and application info");
                            }
                            else
                            {
                                resp.Add(serviceName, Session.errorMessage);
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        //Always checking for childInnerText because the Mobile device sends an empty object, if they do not send an object
                        // the Facade creates the object to check for syncs and adds data to to the object body to let the system know that
                        // its a sync object
                        #region Init Data Downloads
                        case "InitCases":
                            resp = new Dictionary<object, string>();

                            data = iTraycerSection.InitData.InitData.GetInitialCaseData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitInventory":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetInitialInventoryData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitDoctors":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetInitialDoctorsData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitAddresses":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetInitialAddressData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitStatus":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetInitialStatusTableData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitKitAllocation":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetAllKitTrayUsageDates(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;
                        case "InitTrayTypesBySurgeryType":
                            resp = new Dictionary<object, string>();
                            data = iTraycerSection.InitData.InitData.GetInitialTrayTypesBySurgeryType(Session.userInfo.MfgId);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                            }
                            else
                            {
                                resp.Add(serviceName, "SRVERROR:No surgery kit types found for Customer Id: " + Session.userInfo.CustomerId);
                            }
                            responseEnvelope.Response.Add(resp);
                            break;
                        #endregion

                        case "GetAddressesByLatLong":
                            resp = new Dictionary<object, string>();
                            DataTable address = iTraycerSection.Address.AddressesInfo.GetAddressByLatLong(payloadChild);

                            if (address != null)
                            {
                                resp.Add(serviceName, JsonConvert.SerializeObject(address));
                            }
                            else
                            {
                                resp.Add(serviceName, Session.errorMessage);
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "CreateCase":
                            resp = new Dictionary<object, string>();
                            ScheduleInfo obj = CaseScheduler.CreateScheduleInfoObj(payloadChild);

                            if (obj != null)
                            {
                                obj.RepId = Session.userInfo.Id;
                                obj.CompanyId = Session.userInfo.CustomerId;

                                if (ConflictResolution.CheckForSchedulingConflict(obj))
                                {
                                    if (obj.Id < 0)
                                    {
                                        if (DataLayer.Controller.InsertSchedule(obj) == 0)
                                            resp.Add(serviceName, "SRVERROR:Failed to create case");
                                        else
                                            resp.Add(serviceName, JsonConvert.SerializeObject(obj));
                                    }
                                    else
                                    {
                                        int update = DataLayer.Controller.UpdateScheduleByScheduleId(obj);

                                        if (update > 0)
                                            resp.Add(serviceName, JsonConvert.SerializeObject(obj));
                                        else
                                            resp.Add(serviceName, "SRVERROR:Failed update case");
                                    }
                                }
                            } else
                                resp.Add(serviceName, "SRVERROR:Failed to create case schedule object");
                            
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "GenerateInvoice":
                            resp = new Dictionary<object, string>();
                            Invoice invoice = new Invoice();
                            XmlNode CaseId = payloadChild.SelectSingleNode("//CaseId");
                            XmlNode RepSig = payloadChild.SelectSingleNode("//RepSig");
                            XmlNode HosSig = payloadChild.SelectSingleNode("//HosSig");
                            Image repSig = null;
                            Image hosSig = null;
             
                            if (CaseId == null)
                            {
                                resp.Add(serviceName, "SRVERROR:Case number is missing");
                                responseEnvelope.Response.Add(resp);
                                break;
                            }

                            if (!String.IsNullOrEmpty(RepSig.InnerText))
                            {
                                repSig = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(RepSig.InnerText);
                            }

                            if (!String.IsNullOrEmpty(HosSig.InnerText))
                            {
                                hosSig = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(HosSig.InnerText);
                            }

                            List<RequisitionOrder> reqOrders = DataLayer.Controller.GetRequesitionOrderByCaseId(Convert.ToInt32(CaseId.InnerText));
                            
                            // create the pdf memory stream
                            if (reqOrders.Count > 0)
                            {
                                byte[] pdfBA = invoice.CreateInvoice(reqOrders, repSig, hosSig);
                                string pdfString = Convert.ToBase64String(pdfBA, 0, pdfBA.Length);
                                resp.Add(serviceName, pdfString);
                            }
                            else
                            {
                                resp.Add(serviceName, "SRVERROR:Unable to create requistion order. Case number did not return a valid requisition invoice.");
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "UpdateTrayItemsUsage":
                            resp = new Dictionary<object, string>();
                            XmlNode TrayId = payloadChild.SelectSingleNode("//TrayId");
                            XmlNode LotNumber = payloadChild.SelectSingleNode("//LotNumber");
                            XmlNode PartNumber = payloadChild.SelectSingleNode("//PartNumber");
                            XmlNode QntyUsed = payloadChild.SelectSingleNode("//QntyUsed");
                            XmlNode Type = payloadChild.SelectSingleNode("//Type");

                            if (String.IsNullOrEmpty(TrayId.InnerText) || String.IsNullOrEmpty(PartNumber.InnerText)
                                || String.IsNullOrEmpty(QntyUsed.InnerText) || String.IsNullOrEmpty(Type.InnerText))
                            {
                                resp.Add(serviceName, "SRVERROR:One or more items are empty, all fields are required to purge order usage.");
                                continue;
                            }
                            else
                            {
                                bool purge = iTraycerSection.Update.PurgeInventory.PurgeInventroy(TrayId.InnerText, LotNumber.InnerText, PartNumber.InnerText, QntyUsed.InnerText, Type.InnerText);

                                if (purge)
                                {
                                    resp.Add(serviceName, "Successfully updated tray usage.");
                                }
                                else
                                {
                                    resp.Add(serviceName, "Fail to update tray purge usage.");
                                }
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "UpdateSyncTime":
                            DateTime sync = Convert.ToDateTime(responseEnvelope.SyncResponseTime);
                            
                            if (!Session.UpdateLastSyncTime(Session.userInfo, requestEnvelope.DevID, sync))
                            {
                                resp = new Dictionary<object, string>();
                                resp.Add("SRVERROR", "Fail to update sync time for request");
                                responseEnvelope.Response.Add(resp);
                            }
                            break;
                    }
                }
            }
            stopProcessing:
            
            // Always send role and sync time
            responseEnvelope.Role = Session.userInfo.Role;
            responseEnvelope.SyncRequestTime = requestEnvelope.SyncRequestTime;
            return JsonConvert.SerializeObject(responseEnvelope);
        }

        public String ReceiverUpdate()
        {
            string bodyRequest = OperationContext.Current.RequestContext.RequestMessage.ToString();

            XmlDocument payload = new XmlDocument();
            payload.LoadXml(bodyRequest);

            // deserialize payload
            ReceiverEnvelope requestEnvelope = new ReceiverEnvelope();

            XmlSerializer xmlSer = new XmlSerializer(typeof(ReceiverEnvelope));
            requestEnvelope = (ReceiverEnvelope)xmlSer.Deserialize(new StringReader(payload.OuterXml));

            // build response envelope
            ReceiverEnvelope responseEnvelope = new ReceiverEnvelope();
            DataTable dt = DataLayer.Controller.ReceiverReturnKitTrays(1);

            responseEnvelope.ReturnKitTrays = JsonConvert.SerializeObject(dt);
            return JsonConvert.SerializeObject(responseEnvelope);
        }
    }
}

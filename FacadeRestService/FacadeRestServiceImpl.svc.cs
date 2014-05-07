using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using iTraycerSection.Session;
using DataLayer.domains;
using iTraycerSection.Invoice;
using System.Drawing;

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

            // check to see if the appLaunchCount is present, if it is convert it to an int and see if its greater than 1
            if (((!String.IsNullOrEmpty(requestEnvelope.AppLaunchCount)) ? Convert.ToInt32(requestEnvelope.AppLaunchCount) : 1) > 1)
            {
                // changes the current ServiceQueues array and adds the new one to it with the missing Sync Elements
                requestEnvelope.ServiceQueues[0] = FacadeRestService.InitData.AddSyncObjects(payload, requestEnvelope);
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

                    if (String.IsNullOrEmpty(serviceName))
                    {
                        resp = new Dictionary<object, string>();
                        resp.Add("SRVERROR", "No service name was specified");
                        responseEnvelope.Response.Add(resp);
                    }

                    if (!serviceName.Equals("MTSMobileAuth") && !Session.ValidateSession(String.IsNullOrEmpty(requestEnvelope.MtsToken) ? responseEnvelope.MtsToken : requestEnvelope.MtsToken))
                    {
                        resp = new Dictionary<object, string>();
                        resp.Add(serviceName, (Session.errorMessage == null) ? "Session Expired" : Session.errorMessage);
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
                            //payloadChild.InnerText;
                            resp = new Dictionary<object, string>();

                            data = InitData.GetInitialCaseData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitInventory":
                            resp = new Dictionary<object, string>();
                            data = FacadeRestService.InitData.GetInitialInventoryData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitDoctors":
                            resp = new Dictionary<object, string>();
                            data = FacadeRestService.InitData.GetInitialDoctorsData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitAddresses":
                            resp = new Dictionary<object, string>();
                            data = FacadeRestService.InitData.GetInitialAddressData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitStatus":
                            resp = new Dictionary<object, string>();
                            data = FacadeRestService.InitData.GetInitialStatusTableData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitKitAllocation":
                            resp = new Dictionary<object, string>();
                            data = FacadeRestService.InitData.GetAllKitTrayUsageDates(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;
                        #endregion

                        case "CreateCase":
                            resp = new Dictionary<object, string>();
                            List<XmlNode> nodeList = new List<XmlNode>(){
                                payloadChild.SelectSingleNode("//CaseId"),
                                payloadChild.SelectSingleNode("//SurgeonId"),
                                payloadChild.SelectSingleNode("//SurgeonId"),
                                payloadChild.SelectSingleNode("//SurgeryDate"),
                                payloadChild.SelectSingleNode("//DeliverByDate"),
                                payloadChild.SelectSingleNode("//SurgeryType"),
                                payloadChild.SelectSingleNode("//MedicalRecordNumber"),
                                payloadChild.SelectSingleNode("//PatientId"),
                                payloadChild.SelectSingleNode("//SurgeryStatus"),
                                payloadChild.SelectSingleNode("//LocationId"),
                                payloadChild.SelectSingleNode("//VerdibraeLevel"),
                                payloadChild.SelectSingleNode("//LoanerFlag"),
                                payloadChild.SelectSingleNode("//OrderSourceId"),
                                payloadChild.SelectSingleNode("//KitTypeNumber"),
                                payloadChild.SelectSingleNode("//PartNumber"),
                                payloadChild.SelectSingleNode("//LocationId")};

                            ScheduleInfo obj = new ScheduleInfo(nodeList);
                            obj.CreatedDate = DateTime.UtcNow;
                            obj.RepId = Session.userInfo.Id;
                            obj.CompanyId = Session.userInfo.CustomerId;
                            int caseId = DataLayer.Controller.InsertSchedule(obj);
                            if (caseId == 0)
                                resp.Add(serviceName, "SRVERROR:Failed to create case");
                            else
                                resp.Add(serviceName, caseId.ToString());
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
                            byte[] pdfBA = new byte[byte.MaxValue];

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

                            RequisitionOrder reqOrder = DataLayer.Controller.GetRequesitionOrderByCaseId(8);
                            
                            // create the pdf memory stream
                            if(reqOrder != null)
                                pdfBA = invoice.CreateInvoice(reqOrder, repSig, hosSig);

                            MemoryStream pdfDoc = invoice.CreatePdfMemStreamFromByteArray(pdfBA);

                            String pdfString = MTSUtilities.Conversions.Serialization.ObjSerializer < MemoryStream>(pdfDoc);

                            resp.Add(serviceName, pdfString);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "UpdateSyncTime":
                            DateTime sync = DateTime.UtcNow;
                            
                            if (!Session.UpdateLastSyncTime(Session.userInfo, requestEnvelope.DevID, sync))
                            {
                                resp = new Dictionary<object, string>();
                                resp.Add("SRVERROR", "Fail to update sync time for request");
                                responseEnvelope.Response.Add(resp);
                            }
                            responseEnvelope.SyncResponseTime = sync.ToString();
                            break;
                    }
                }
            }
            stopProcessing:
            
            responseEnvelope.Role = Session.userInfo.Role;
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

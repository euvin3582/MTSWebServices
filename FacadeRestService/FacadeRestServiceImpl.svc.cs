﻿using System;
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
using MTSUtilities.Enums;

namespace FacadeRestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FacadeRestServiceImpl" in code, svc and config file together.
    public class FacadeRestServiceImpl : IFacadeRestServiceImpl
    {
        public string CreateSession()
        {
            MTSLoggerLib.Entities.Logger.MobileLog item = new MTSLoggerLib.Entities.Logger.MobileLog();
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

            //fill SrvError Loging object
            item.DeviceId = requestEnvelope.DevID;
            item.JsonRequestEnvelope = bodyRequest;
            item.RequestTime = Convert.ToDateTime(requestEnvelope.SyncRequestTime);

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
                    catch (Exception ex)
                    {
                        // Add info to Db Log Error object
                        item.SrvErrorMsg = "Service request objects envelope does not match";
                        item.ErrorObjectName = SrvErrorEnum.SrvErrorLevel.SRVERROR.ToString();
                        item.SrvErrorException = ex.Message.ToString();

                        // store envelope and do forensics
                        resp = new Dictionary<object, string>();
                        resp.Add(item.ErrorObjectName, item.SrvErrorMsg + ". StackTrace: " + ex.StackTrace);
                        
                        responseEnvelope.Response.Add(resp);
                        MTSUtilities.Logger.Log.MOBILEToDB(item);
                        continue;
                    }

                    // gets the service name from the object
                    string data = null;
                    string serviceName = payloadChild.DocumentElement.Name;
                    string childInnerText = payloadChild.DocumentElement.InnerText;
                    XmlNodeList authObject = payload.SelectNodes("/root/ServiceQueue/item/MTSMobileAuth"); 

                    if (String.IsNullOrEmpty(serviceName))
                    {
                        // Add info to Db Log Error object
                        item.SrvErrorMsg = "No service name was specified";
                        item.ErrorObjectName = SrvErrorEnum.SrvErrorLevel.SRVERROR.ToString();

                        resp = new Dictionary<object, string>();
                        resp.Add(item.ErrorObjectName, item.SrvErrorMsg);
                        responseEnvelope.Response.Add(resp);
                        MTSUtilities.Logger.Log.MOBILEToDB(item);
                    }

                    bool validToken  = !String.IsNullOrEmpty(requestEnvelope.MtsToken) || !String.IsNullOrEmpty(responseEnvelope.MtsToken);

                    // First check if valid token exist
                    if (validToken)
                    {
                        bool sessionValid = Session.ValidateSession(String.IsNullOrEmpty(requestEnvelope.MtsToken) ? responseEnvelope.MtsToken : requestEnvelope.MtsToken);

                        if (!sessionValid)
                        {
                            item.SrvErrorMsg = (Session.errorMessage == null) ? "Session Expired" : Session.errorMessage;

                            resp = new Dictionary<object, string>();
                            item.ErrorObjectName = serviceName;

                            resp.Add(serviceName, item.SrvErrorMsg);
                            responseEnvelope.Response.Add(resp);
                            MTSUtilities.Logger.Log.MOBILEToDB(item);
                            break;
                        }
                    }
                    else if (authObject.Count == 0)
                    {
                        item.SrvErrorMsg = "MTSMobileAuth was not sent or a valid authorization token found";
                        item.ErrorObjectName = SrvErrorEnum.SrvErrorLevel.SRVERROR.ToString();

                        resp = new Dictionary<object, string>();
                        resp.Add(item.ErrorObjectName, item.SrvErrorMsg);
                        responseEnvelope.Response.Add(resp);
                        MTSUtilities.Logger.Log.MOBILEToDB(item);
                        break;
                    }

                    // assign server error name and create new response object on the top level
                    item.ErrorObjectName = serviceName;
                    resp = new Dictionary<object, string>();

                    // if session is valid get customer and rep id
                    if (Session.sessionValid)
                    {
                        item.RepId = Session.userInfo.Id;
                        item.CompanyId = Session.userInfo.CustomerId;
                    }

                    switch (serviceName)
                    {
                        case "MTSMobileAuth":
                            XmlNode email = payloadChild.SelectSingleNode("//Email");
                            XmlNode password = payloadChild.SelectSingleNode("//Password");
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
                            
                            // validate all data first
                            if (DeviceId != null || DeviceOsVersion != null || DevicePlatform != null){
                                device.DeviceId = DeviceId.InnerText;
                                device.DeviceOsVersion = DeviceOsVersion.InnerText;
                                device.Platform = DevicePlatform.InnerText;
                            } else{
                                item.SrvErrorMsg = "SRVERROR:Fail to provide device info: DeviceId, DeviceOsVersion or DevicePlatform";

                                resp.Add(serviceName, item.SrvErrorMsg);
                                responseEnvelope.Response.Add(resp);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
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
                                    item.SrvErrorMsg = "SRVERROR:Failed to register device";

                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    responseEnvelope.Response.Add(resp);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item);
                                    break;
                                }   
                            }
                            else
                            {
                                item.ErrorObjectName = serviceName;

                                if (Session.UpdateApplicationInfo(Session.userInfo, DeviceId.InnerText))
                                {
                                    item.SrvErrorMsg = "SRVWARNING:Device already registered, information updated";

                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item, MTSLoggerLib.Enums.Level.Warning);
                                }
                                else
                                {
                                    item.SrvErrorMsg = "SRVERROR:Fail to update Device Info";
                                    
                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item);
                                }
                                responseEnvelope.Response.Add(resp);
                                break;
                            }

                            if (iTraycerSection.Validation.Validate.ValidateApplicationDeviceInfo(Session.userInfo.Id, Session.userInfo.CustomerId, device, Convert.ToInt32(requestEnvelope.AppLaunchCount)))
                            {
                                resp.Add(serviceName, "Successfully validate device and application info");
                            }
                            else
                            {
                                item.SrvErrorMsg = Session.errorMessage;
                                resp.Add(serviceName, Session.errorMessage);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        //Always checking for childInnerText because the Mobile device sends an empty object, if they do not send an object
                        // the Facade creates the object to check for syncs and adds data to to the object body to let the system know that
                        // its a sync object
                        #region Init Data Downloads
                        case "InitCases":
                            data = iTraycerSection.InitData.InitData.GetInitialCaseData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitInventory":
                            data = iTraycerSection.InitData.InitData.GetInitialInventoryData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitDoctors":
                            data = iTraycerSection.InitData.InitData.GetInitialDoctorsData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);
                            
                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitAddresses":
                            data = iTraycerSection.InitData.InitData.GetInitialAddressData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitStatus":
                            data = iTraycerSection.InitData.InitData.GetInitialStatusTableData(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitKitAllocation":
                            data = iTraycerSection.InitData.InitData.GetAllKitTrayUsageDates(String.IsNullOrEmpty(childInnerText) ? null : Session.lastSync);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "InitTrayTypesBySurgeryType":
                            data = iTraycerSection.InitData.InitData.GetInitialTrayTypesBySurgeryType(Session.userInfo.MfgId);

                            if (!String.IsNullOrEmpty(data))
                            {
                                resp.Add(serviceName, data);
                            }
                            else
                            {
                                item.SrvErrorMsg = "SRVERROR:No surgery kit types found for Customer Id: " + Session.userInfo.CustomerId;

                                resp.Add(serviceName, item.SrvErrorMsg);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
                            }
                            responseEnvelope.Response.Add(resp);
                            break;
                        #endregion

                        case "GetAddressesByLatLong":
                            DataTable address = iTraycerSection.Address.AddressesInfo.GetAddressByLatLong(payloadChild);

                            if (address != null)
                                resp.Add(serviceName, JsonConvert.SerializeObject(address));
                            else
                            {
                                item.SrvErrorMsg = "SRVERROR:A Latitude or Longitude was not provided";
                                resp.Add(serviceName, item.SrvErrorMsg);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
                            }

                            responseEnvelope.Response.Add(resp);
                            break;

                        case "CreateCase":
                            ScheduleInfo obj = CaseScheduler.CreateScheduleInfoObj(payloadChild);
                            item.ErrorObjectName = serviceName;

                            if (obj != null)
                            {
                                if (obj.SurgeonId < 0)
                                {
                                    obj.RepId = Session.userInfo.Id;
                                    obj.CompanyId = Session.userInfo.CustomerId;
                                    obj.CreatedByRepId = Session.userInfo.Id;
                                    obj.SurgeonId = CaseScheduler.CreateDoctor(obj);
                                    obj.SurgeonInfo.Id = obj.SurgeonId;
                                }

                                if (!ConflictResolution.CheckForConflict("SCheduledConflict", obj))
                                {
                                    if (obj.Id < 0)
                                    {
                                        // Inserts the new Case Id back into the schedule object
                                        try
                                        {
                                            obj.Id = DataLayer.Controller.InsertSchedule(obj);
                                        }
                                        catch (Exception ex)
                                        { 
                                            item.SrvErrorMsg = "SRVERROR:Failed to create case";
                                            item.SrvErrorException = ex.Message;
                                            resp.Add(serviceName, item.SrvErrorMsg);
                                            MTSUtilities.Logger.Log.MOBILEToDB(item);
                                        }

                                        if (obj.Id > 0)
                                            resp.Add(serviceName, JsonConvert.SerializeObject(obj));
                                    }
                                    else
                                    {
                                        int update = DataLayer.Controller.UpdateScheduleByScheduleId(obj);

                                        if (update > 0)
                                            resp.Add(serviceName, JsonConvert.SerializeObject(obj));
                                        else
                                        {
                                            item.SrvErrorMsg = "SRVERROR:Failed update case";
                                            resp.Add(serviceName, item.SrvErrorMsg);
                                            MTSUtilities.Logger.Log.MOBILEToDB(item);
                                        }
                                    }
                                }
                                else
                                {
                                    item.SrvErrorMsg = "SRVERROR:A Schedule conflict was detected";
                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item);
                                }
                            }
                            else
                            {
                                item.ErrorObjectName = serviceName;
                                item.SrvErrorMsg = "SRVERROR:Failed to create case schedule object";

                                resp.Add(serviceName,  item.SrvErrorMsg);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
                            }
                            
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "GenerateInvoice":
                            Invoice invoice = new Invoice();
                            XmlNode CaseId = payloadChild.SelectSingleNode("//CaseId");
                            XmlNode RepSig = payloadChild.SelectSingleNode("//RepSig");
                            XmlNode HosSig = payloadChild.SelectSingleNode("//HosSig");
                            Image repSig = null;
                            Image hosSig = null;
             
                            if (CaseId == null)
                            {
                                item.ErrorObjectName = serviceName;
                                item.SrvErrorMsg = "SRVERROR:Case number is missing";

                                resp.Add(serviceName, item.SrvErrorMsg);
                                responseEnvelope.Response.Add(resp);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
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

                                if (Invoice.SaveRequistionToDB(Convert.ToInt32(CaseId.InnerText), pdfBA) > 0)
                                {
                                    if (iTraycerSection.Session.Session.errorMessage != null)
                                    {
                                        resp.Add(serviceName, iTraycerSection.Session.Session.errorMessage);
                                    }
                                    resp.Add(serviceName, "Successfully store Requistion Invoice into MTSSurgeryDetails Table");
                                }
                                else
                                {
                                    item.SrvErrorMsg = "Fail to store Requisition Invoice into MTSSurgeryDetails Table";
                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item);
                                }
                            }
                            else
                            {
                                item.SrvErrorMsg = "SRVERROR:Unable to create requistion order. Case number did not return a valid requisition invoice.";
                                resp.Add(serviceName, item.SrvErrorMsg);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
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
                                item.SrvErrorMsg = "SRVERROR:One or more items are empty, all fields are required to purge order usage.";
                                resp.Add(serviceName, item.SrvErrorMsg);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
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
                                    item.SrvErrorMsg = "SRVERROR:Fail to update tray purge usage.";
                                    resp.Add(serviceName, item.SrvErrorMsg);
                                    MTSUtilities.Logger.Log.MOBILEToDB(item);
                                }
                                responseEnvelope.Response.Add(resp);
                            }
                            break;

                        case "UpdateSyncTime":
                            DateTime sync = Convert.ToDateTime(responseEnvelope.SyncResponseTime);
                            
                            if (!Session.UpdateLastSyncTime(Session.userInfo, requestEnvelope.DevID, sync))
                            {
                                item.ErrorObjectName = SrvErrorEnum.SrvErrorLevel.SRVERROR.ToString();
                                item.SrvErrorMsg = "Fail to update sync time for request";

                                resp.Add(item.ErrorObjectName, item.SrvErrorMsg);
                                responseEnvelope.Response.Add(resp);
                                MTSUtilities.Logger.Log.MOBILEToDB(item);
                            }
                            break;
                    }
                }
            }
            stopProcessing:
            
            // Always send role and sync time
            responseEnvelope.Role = Session.userInfo.Role;
            responseEnvelope.SyncRequestTime = requestEnvelope.SyncRequestTime;
            responseEnvelope.Commit = "true";
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

    public class SrvErrorEnum
    {
        public enum SrvErrorLevel : byte
        {
            [StringValue("SRVERROR")]
            SRVERROR,
            [StringValue("SRVWARNING")]
            SRVWARNING
        }
    }
}

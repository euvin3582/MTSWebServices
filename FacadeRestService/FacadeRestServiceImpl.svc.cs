using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using DataLayer.domains;
using Newtonsoft.Json;
using iTraycerSection.Session;
using MTSUtilities.Conversions;

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

            // get service name to access
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                XmlNode[] serviceQueueNodes = (XmlNode[])requestEnvelope.ServiceQueues[0];

                // create response dictionary and create response envelope
                Dictionary<object, string> resp = null;
                responseEnvelope.Response = new List<object>();

                for (int i = 0; i < serviceQueueNodes.Length-1; i++)
                {
                    // creat the payload child
                    XmlDocument payloadChild = new XmlDocument();
                    payloadChild.LoadXml(serviceQueueNodes[i+1].ChildNodes[0].OuterXml);

                    // gets the service name from the object
                    string serviceName = payloadChild.DocumentElement.Name;

                    if (String.IsNullOrEmpty(serviceName))
                        resp.Add("Error", "No service name was specified"); 

                    switch (serviceName)
                    {
                        case "MTSMobileAuth":
                            XmlNode email = payloadChild.SelectSingleNode("//Email");
                            XmlNode password = payloadChild.SelectSingleNode("//Password");
                            resp = new Dictionary<object, string>();
                            string[] authResponse = new string[3];

                            // validate the user
                            if (email != null && password != null)
                                authResponse = Session.CreateUserSession(email.InnerText, password.InnerText);

                            if (authResponse.Length > 0)
                            {
                                responseEnvelope.CoID = authResponse[0];
                                responseEnvelope.RepID = authResponse[1];
                                responseEnvelope.MtsToken = authResponse[2];
                                resp.Add(serviceName, "Successfully authenticated user");
                                responseEnvelope.Commit = "true";
                            }
                            else
                            {
                                resp.Add(serviceName, "Failed to authenticate user");
                                responseEnvelope.Commit = "false";
                            }
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "MobileDeviceRegister":
                            iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                            XmlNode DeviceId = payloadChild.SelectSingleNode("//DeviceID");
                            XmlNode DeviceOsVersion = payloadChild.SelectSingleNode("//DeviceOSVersion");
                            XmlNode DevicePlatform = payloadChild.SelectSingleNode("//DevicePlatform");
                            resp = new Dictionary<object, string>();
                            String msg = "";
                            
                            if (DeviceId != null)
                                   device.DeviceId = DeviceId.InnerText;

                            if (DeviceOsVersion != null)
                                   device.DeviceOsVersion = DeviceOsVersion.InnerText;
                            if (DevicePlatform != null)
                                   device.Platform = DevicePlatform.InnerText;

                            if (iTraycerSection.Device.Device.CheckIfExist(device.DeviceId))
                            {
                               msg = "Device already registered";
                                responseEnvelope.Commit = "false";
                            }
                            else
                            {
                                if (iTraycerSection.Device.Device.AddDeviceInfo(device))
                                {
                                    msg = "Successfully resgistered device";
                                    responseEnvelope.Commit = "true";
                                }
                                else
                                {
                                    msg = "Failed to register device";
                                    responseEnvelope.Commit = "false";
                                }
                            }

                            if (iTraycerSection.Validation.Validate.ValidateApplicationDeviceInfo(Session.userInfo.Id, Session.userInfo.CustomerId, device))
                            {
                                msg = msg + ", Successfully validate device and application info";
                            }
                            resp.Add(serviceName, msg);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "InitDataLoad":
                            resp = new Dictionary<object, string>();
                            string data = JsonConvert.SerializeObject(
                                Session.userInfo.IsSuperUser ? 
                                    DataLayer.Controller.GetSchedulesByCustomerId(Session.userInfo.CustomerId) : 
                                    DataLayer.Controller.GetSchedulesByRep(Session.userInfo));

                            resp.Add(serviceName, data);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "InitInventory":
                            resp = new Dictionary<object, string>();
                            data = JsonConvert.SerializeObject(DataLayer.Controller.GetInventoryByCompanyId(Session.userInfo.CustomerId));
                            resp.Add(serviceName, data);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "CreateCase":
                            List<XmlNode> nodeList = new List<XmlNode>(){
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
                                payloadChild.SelectSingleNode("//PartNumber")};

                            ScheduleInfo obj = new ScheduleInfo(nodeList);
                            obj.CreatedDate = DateTime.UtcNow;
                            obj.RepId = Session.userInfo.Id;
                            int caseId = DataLayer.Controller.InsertSchedule(obj);
                            resp.Add(serviceName, caseId.ToString());
                            responseEnvelope.Response.Add(resp);
                            break;
                    }
                }
            }
            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            responseEnvelope.Role = Session.userInfo.Role;
            return JsonConvert.SerializeObject(responseEnvelope);
        }
    }
}

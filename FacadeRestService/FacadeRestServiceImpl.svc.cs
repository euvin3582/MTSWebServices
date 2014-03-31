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

                // create response object in return envelope
                responseEnvelope.Response = new List<object>();

                // create new response object and initialize list
                List<string> responseVariables = null;

                for (int i = 0; i < serviceQueueNodes.Length-1; i++)
                {
                    // creat the payload child
                    XmlDocument payloadChild = new XmlDocument();
                    payloadChild.LoadXml(serviceQueueNodes[i+1].ChildNodes[0].OuterXml);
                    
                    // create response dictionary
                    Dictionary<object, List<string>> resp = new Dictionary<object, List<string>>();

                    // gets the service name from the object
                    string serviceName = payloadChild.DocumentElement.Name;

                    if (String.IsNullOrEmpty(serviceName))
                        responseVariables.Add("No service name was specified"); 

                    switch (serviceName)
                    {
                        case "MTSMobileAuth":
                            XmlNode email = payloadChild.SelectSingleNode("//Email");
                            XmlNode password = payloadChild.SelectSingleNode("//Password");
                            string[] authResponse = new string[3];
                            responseVariables = new List<string>();

                            // validate the user
                            if (email != null && password != null)
                                authResponse = Session.CreateUserSession(email.InnerText, password.InnerText);

                            if (authResponse.Length > 0)
                            {
                                responseEnvelope.CoID = authResponse[0];
                                responseEnvelope.RepID = authResponse[1];
                                responseEnvelope.MtsToken = authResponse[2];
                                responseVariables.Add("Succesfully authenticated user");
                                responseEnvelope.Commit = "true";
                            }
                            else
                            {
                                responseVariables.Add("Failed to authenticate user");
                                responseEnvelope.Commit = "false";
                            }
                            
                            resp.Add(serviceName, responseVariables);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "MobileDeviceRegister":
                            iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                            responseVariables = new List<string>();
                            XmlNode DeviceId = payloadChild.SelectSingleNode("//DeviceID");
                            XmlNode DeviceOsVersion = payloadChild.SelectSingleNode("//DeviceOSVersion");
                            XmlNode DevicePlatform = payloadChild.SelectSingleNode("//DevicePlatform");
                            int repId = Session.userInfo.Id;
                            
                            if (DeviceId != null)
                                   device.DeviceId = DeviceId.InnerText;

                            if (DeviceOsVersion != null)
                                   device.DeviceOsVersion = DeviceOsVersion.InnerText;
                            if (DevicePlatform != null)
                                   device.Platform = DevicePlatform.InnerText;

                            if (iTraycerSection.Device.Device.CheckIfExist(device.DeviceId))
                            {
                                responseVariables.Add("Device already exist in database");
                                responseEnvelope.Commit = "false";
                            }
                            else
                            {
                                if (iTraycerSection.Device.Device.AddDeviceInfo(device))
                                {
                                    responseVariables.Add("Successfully resgistered device");
                                    responseEnvelope.Commit = "true";
                                }
                                else
                                {
                                    responseVariables.Add("Failed to register device");
                                    responseEnvelope.Commit = "false";
                                }
                            }

                            if (iTraycerSection.Validation.Validate.ValidateApplicationDeviceInfo(Session.userInfo.Id, Session.userInfo.CustomerId, device))
                            {
                                responseVariables.Add("Successfully validate device and application info");
                            }

                            resp.Add(serviceName, responseVariables);
                            responseEnvelope.Response.Add(resp);
                            break;

                        case "OtherServiceName":
                            responseVariables = new List<string>();
                            string data = JsonConvert.SerializeObject(
                                Session.userInfo.IsSuperUser ? 
                                    DataLayer.Controller.GetSchedulesByCustomerId(Session.userInfo.CustomerId) : 
                                    DataLayer.Controller.GetSchedulesByRep(Session.userInfo));
                            responseVariables.Add(data);
                            resp.Add(serviceName, responseVariables);
                            responseEnvelope.Response.Add(resp);
                            break;
                    }
                }
            }
            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            return JsonConvert.SerializeObject(responseEnvelope);
           
        }
    }
}

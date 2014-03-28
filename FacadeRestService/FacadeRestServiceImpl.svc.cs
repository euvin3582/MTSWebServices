using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using DataLayer.domains;
using Newtonsoft.Json;
using iTraycerSection.Session;

namespace FacadeRestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FacadeRestServiceImpl" in code, svc and config file together.
    public class FacadeRestServiceImpl : IFacadeRestServiceImpl
    {
        public string CreateSession()
        {
            string bodyRequest = OperationContext.Current.RequestContext.RequestMessage.ToString();
           //System.Runtime.Remoting.Contexts.Context.
            XmlDocument payload = new XmlDocument();
            payload.LoadXml(bodyRequest);
            
            // deserialize payload
            JsonEnvelope requestEnvelope = new JsonEnvelope();

            XmlSerializer xmlSer = new XmlSerializer(typeof(JsonEnvelope));
            requestEnvelope = (JsonEnvelope)xmlSer.Deserialize(new StringReader(payload.OuterXml));

            // build response envelope
            JsonEnvelope responseEnvelope = new JsonEnvelope();
            object[] ServiceQueues;

            // get service name to access
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                // keeps track of which service the response message needs to go into
                object response;
                List<string> responseVariables = null;
                XmlNodeList serviceQueueNodes = ((XmlElement)((XmlNode[])requestEnvelope.ServiceQueues.GetValue(0)).GetValue(1)).ChildNodes;

                // create response object in return envelope
                ServiceQueues = new object[serviceQueueNodes.Count];
                responseEnvelope.ServiceQueues = ServiceQueues;

                for (int i = 0; i < serviceQueueNodes.Count; i++)
                {
                    // create new response object and initialize list
                    response = new object();
                    responseVariables = new List<string>();

                    // creat the payload child
                    XmlDocument payloadChild = new XmlDocument();
                    payloadChild.LoadXml(serviceQueueNodes.Item(i).OuterXml);
                    

                    // gets the service name from the object
                    string serviceName = payloadChild.DocumentElement.Name;

                    if (!String.IsNullOrEmpty(serviceName))
                        responseVariables.Add(serviceName);
                    else
                        responseVariables.Add("No service name was specified"); 

                    switch (serviceName)
                    {
                        case "MTSMobileAuth":
                            XmlNode email = payloadChild.SelectSingleNode("//Email");
                            XmlNode password = payloadChild.SelectSingleNode("//Password");
                            string[] authResponse = new string[3];

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

                            response = responseVariables;
                            responseEnvelope.ServiceQueues[i] = response;
                            break;

                        case "MobileDeviceRegister":
                            iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                            XmlNode DeviceId = payloadChild.SelectSingleNode("//DeviceID");
                            XmlNode DeviceOsVersion = payloadChild.SelectSingleNode("//DeviceOSVersion");
                            XmlNode DevicePlatform = payloadChild.SelectSingleNode("//DevicePlatform");
                            int repId = Session.userInfo.Id;
                            
                            if (DeviceId != null)
                                   device.DeviceId = DeviceId.InnerText;
                            //else responseEnvelope.ResponseMessage = "No device info was found";
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

                            response = responseVariables;
                            responseEnvelope.ServiceQueues[i] = response;
                            break;
                    }
                }
            }

            if (payload == null)
            {
                //responseEnvelope.ServiceQueues = "Service Name was not found";
            }




            
           
            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            return JsonConvert.SerializeObject(responseEnvelope);
           
        }
    }
}

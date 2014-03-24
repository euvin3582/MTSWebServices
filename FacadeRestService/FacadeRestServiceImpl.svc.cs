using DataLayer.domains;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

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
            ServiceQueue servieQueue = new ServiceQueue();
            Parameter parameters = new Parameter();
            responseEnvelope.ServiceQueues = servieQueue;
            responseEnvelope.ServiceQueues.Parameters = parameters;

            // get service name to access
            string serviceName = requestEnvelope.ServiceQueues.ServiceName;

            if (String.IsNullOrEmpty(serviceName))
                responseEnvelope.ServiceQueues.ResponseMessage = "No service name was specified";
            else
                responseEnvelope.ServiceQueues.ServiceName = serviceName;

            switch (serviceName)
            {
                case "MTSMobileAuth":
                    iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                    String email = "";
                    String pass = "";

                    if (!String.IsNullOrEmpty(requestEnvelope.ServiceQueues.ServiceName))
                    {
                        device.DeviceId = (requestEnvelope.ServiceQueues).Parameters.DeviceID;
                        device.DeviceOsVersion = (requestEnvelope.ServiceQueues).Parameters.DeviceOSVersion;
                        device.Platform = (requestEnvelope.ServiceQueues).Parameters.DevicePlatform;
                        email = (requestEnvelope.ServiceQueues).Parameters.Email;
                        pass = (requestEnvelope.ServiceQueues).Parameters.Password;
                    }
                    else return "No divice info found";
            
                    // before you run this we need to pass a device to the json envelope to fill in the device info 
                    string[] resposne = iTarycerSection.Session.Session.CreateUserSession(email, pass, device);

                    if (resposne.Length > 0)
                    {
                        responseEnvelope.CoID = resposne[0];
                        responseEnvelope.RepID = resposne[1];
                        responseEnvelope.MtsToken = resposne[2];
                        responseEnvelope.ServiceQueues.ResponseMessage = "Succesfully authenticated user";
                        responseEnvelope.Commit = "true";
                    }
                    else
                    {
                        responseEnvelope.ServiceQueues.ResponseMessage = "Failed to authenticate user";
                        responseEnvelope.Commit = "false";
                    }
                    break;

                case "MTSOther":
                    break;
            }

            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            return JsonConvert.SerializeObject(responseEnvelope);
           
        }
    }
}

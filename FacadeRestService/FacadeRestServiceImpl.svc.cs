using DataLayer.domains;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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
            //servieQueue = (ServiceQueue)xmlSer.Deserialize(new StringReader(payload.OuterXml));

            // build response envelope
            JsonEnvelope responseEnvelope = new JsonEnvelope();

            // get service name to access
            string serviceName = ""; //requestEnvelope.ServiceQueues[0].MTSMobileAuths.ToString();

            //if (String.IsNullOrEmpty(serviceName))
               //responseEnvelope.ServiceQueues[0].ResponseMessage = "No service name was specified";

            switch (serviceName)
            {
                case "FacadeRestService.MTSMobileAuth":
                    iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                    String email = "";
                    String pass = "";

                    //if (requestEnvelope.ServiceQueues[0].MTSMobileAuth != null)
                    //{
                    //    device.DeviceId = (requestEnvelope.ServiceQueues[0]).MTSMobileAuth.DeviceID;
                    //    device.DeviceOsVersion = (requestEnvelope.ServiceQueues[0]).MTSMobileAuth.DeviceOSVersion;
                    //    device.Platform = (requestEnvelope.ServiceQueues[0]).MTSMobileAuth.DevicePlatform;
                    //    email = (requestEnvelope.ServiceQueues[0]).MTSMobileAuth.Email;
                    //    pass = (requestEnvelope.ServiceQueues[0]).MTSMobileAuth.Password;
                    //}
                    //else return "No divice info found";
            
                    // before you run this we need to pass a device to the json envelope to fill in the device info 
                    string[] resposne = iTarycerSection.Session.Session.CreateUserSession(email, pass, device);

                    if (resposne.Length > 0)
                    {
                        responseEnvelope.CoID = resposne[0];
                        responseEnvelope.RepID = resposne[1];
                        responseEnvelope.MtsToken = resposne[2];
                       // responseEnvelope.ServiceQueues[0].ResponseMessage = "Succesfully authenticated user";
                        responseEnvelope.Commit = "true";
                    }
                    else
                    {
                        //responseEnvelope.ServiceQueues[0].ResponseMessage = "Failed to authenticate user";
                        responseEnvelope.Commit = "false";
                    }
                    break;

                case "FacadeRestService.MTSOther":
                    break;
            }

            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            return JsonConvert.SerializeObject(responseEnvelope);
           
        }
    }
}

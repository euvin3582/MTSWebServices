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
           //System.Runtime.Remoting.Contexts.Context.
            XmlDocument payload = new XmlDocument();
            payload.LoadXml(bodyRequest);
            object response = new object();
            List<string> temp = new List<string>();
            temp.Add("MTSAuth");
            temp.Add("Response:");
            temp.Add("Fail");
            var something = temp;
            response = something;
            // deserialize payload
            JsonEnvelope requestEnvelope = new JsonEnvelope();

            XmlSerializer xmlSer = new XmlSerializer(typeof(JsonEnvelope));
            requestEnvelope = (JsonEnvelope)xmlSer.Deserialize(new StringReader(payload.OuterXml));

            // build response envelope
            JsonEnvelope responseEnvelope = new JsonEnvelope();

            // get service name to access
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                XmlNode[] nodes = (XmlNode[])requestEnvelope.ServiceQueues.GetValue(0);

                if (nodes.Length > 1)
                {
                    payload = new XmlDocument();
                    XmlElement element = (XmlElement)nodes.GetValue(1);
                    payload.LoadXml(element.InnerXml);
                }
            }

            if (payload == null)
            {
                //responseEnvelope.ServiceQueues = "Service Name was not found";
            }

            string serviceName = payload.DocumentElement.Name;

            if (String.IsNullOrEmpty(serviceName))
                //responseEnvelope.ResponseMessage = "No service name was specified";

            switch (serviceName)
            {
                case "MTSMobileAuth":
                    iTraycerDeviceInfo device = new iTraycerDeviceInfo();
                    String email = "";
                    String pass = "";

                    XmlNode DeviceId = payload.SelectSingleNode("//DeviceID");
                    XmlNode DeviceOsVersion = payload.SelectSingleNode("//DeviceOSVersion");
                    XmlNode DevicePlatform = payload.SelectSingleNode("//DevicePlatform");
                    XmlNode Email = payload.SelectSingleNode("//Email");
                    XmlNode Password = payload.SelectSingleNode("//Password");

                    if (DeviceId != null)
                           device.DeviceId = DeviceId.InnerText;
                    //else responseEnvelope.ResponseMessage = "No device info was found";
                    if (DeviceOsVersion != null)
                           device.DeviceOsVersion = DeviceOsVersion.InnerText;
                    if (DevicePlatform != null)
                           device.Platform = DevicePlatform.InnerText;
                    if (Email != null)
                           email = Email.InnerText;
                    if (Password != null)
                           pass = Password.InnerText;
            
                    string[] resposne = iTarycerSection.Session.Session.CreateUserSession(email, pass, device);

                    if (resposne.Length > 0)
                    {
                        responseEnvelope.CoID = resposne[0];
                        responseEnvelope.RepID = resposne[1];
                        responseEnvelope.MtsToken = resposne[2];
                        //responseEnvelope.ResponseMessage = "Succesfully authenticated user";
                        responseEnvelope.Commit = "true";
                    }
                    else
                    {
                        //responseEnvelope.ResponseMessage = "Failed to authenticate user";
                        responseEnvelope.Commit = "false";
                    }
                    break;

                case "FacadeRestService.MTSOther":
                    break;
            }
            object[] ServiceQueues = new object[3];
            responseEnvelope.ServiceQueues = ServiceQueues;
            responseEnvelope.ServiceQueues[0] = response;
            responseEnvelope.SyncResponseTime = DateTime.UtcNow.ToString();
            return JsonConvert.SerializeObject(responseEnvelope);
           
        }
    }
}

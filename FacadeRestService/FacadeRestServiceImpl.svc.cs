using DataLayer.domains;
using Newtonsoft.Json;
using System;
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
        #region IRestService Members

        public string GUID()
        {
            return "Your requested product";
        }

        public string JsonData(string id)
        {
            return "Your requested product " + id;
        }

        public string CreateSession()
        {
            string bodyRequest = OperationContext.Current.RequestContext.RequestMessage.ToString();
            string guid = null;

            String user = "";
            String pass = "";

            XmlDocument payload = new XmlDocument();
            payload.LoadXml(bodyRequest);

            JsonEnvelope jenvelope = new JsonEnvelope();
            XmlSerializer xmlSer = new XmlSerializer(typeof(JsonEnvelope));

            jenvelope = (JsonEnvelope)xmlSer.Deserialize(new StringReader(payload.OuterXml));
            //if (jenvelope. != null && nPassword != null)
            //{
            //    user = nEmail.InnerText;
            //    pass = nPassword.InnerText;
            //}
            //else
            //    return "Unable to authenticate user";

            iTraycerDeviceInfo device = new iTraycerDeviceInfo();
            device.DeviceId = 
            
           // before you run this we need to pass a device to the json envelope to fill in the device info 
            iTarycerSection.Session.Session.CreateUserSession(user, pass, device);

            return guid;
        }

        #endregion
    }
}

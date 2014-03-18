using System;
using System.ServiceModel;
using System.Xml;

namespace FacadeRestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FacadeRestServiceImpl" in code, svc and config file together.
    public class FacadeRestServiceImpl : IFacadeRestServiceImpl
    {
        #region IRestService Members

        public string XmlData(string id)
        {
            return "Your requested product " + id;
        }

        public string JsonData(string id)
        {
            return "Your requested product " + id;
        }

        public string JsonDataPost()
        {
            string x = OperationContext.Current.RequestContext.RequestMessage.ToString();
            String CoID = "";
            String RepID = "";

            XmlDocument payload = new XmlDocument();
            payload.LoadXml(x);
            XmlNode nCoID = payload.SelectSingleNode("//CoID");
            XmlNode nRepID = payload.SelectSingleNode("//RepID");

            if(nCoID!=null)
                CoID = nCoID.InnerText;
            if(nRepID!=null)
                RepID = nRepID.InnerText;

            return "Your requested CoID is: " + CoID + ". Your requested RepID is: " + RepID;
        }

        #endregion
    }
}

using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace FacadeRestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFacadeRestServiceImpl" in both code and config file together.
    [ServiceContract]
    public interface IFacadeRestServiceImpl
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "xml/{id}")]
        string XmlData(string id);

/*         [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "json/{id}")]
        string JsonData(string id);
*/
        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "json/")]
        string JsonDataPost();
    }
}

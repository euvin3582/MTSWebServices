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
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "MTSMobileService/")]
        string CreateSession();

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "ReceiverManager/")]
        string ReceiverUpdate();
    }
}

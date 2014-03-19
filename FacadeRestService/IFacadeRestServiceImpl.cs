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
        [WebGet(UriTemplate = "/",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GUID();

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
            UriTemplate = "jcreate/")]
        string CreateSession();

        //[OperationContract]
        //[WebGet(UriTemplate = "/"),
        //ResponseFormat = WebMessageFormat.Json]
        //FacadeRestServiceImpl[] GetAllProducts();

        //[OperationContract]
        //[WebGet(UriTemplate = "/{id}")]
        //FacadeRestServiceImpl GetProductById(string id);

        //[OperationContract]
        //[WebInvoke(UriTemplate = "/create", Method = "POST")]//, RequestFormat=WebMessageFormat.Xml, BodyStyle=WebMessageBodyStyle.Bare)]
        //void CreateProduct(FacadeRestServiceImpl facade);

        //[OperationContract]
        //[WebInvoke(UriTemplate = "/{id}", Method = "PUT")]
        //void UpdateProduct(string id, FacadeRestServiceImpl facade);

        //[OperationContract]
        //[WebInvoke(UriTemplate = "/{id}", Method = "DELETE")]
        //void DeleteProduct(string id);
    }
}

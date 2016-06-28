using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Web;

namespace NetworkAPI
{
    [ServiceContract]
    public interface INetwork
    {
        [WebGet(UriTemplate = "assemblies/{assemblyName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetAssemblyTypes(string assemblyName);

        [WebGet(UriTemplate = "managers", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagers();

        [WebGet(UriTemplate = "managers/{managername}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerTypes(string managername);

        [WebGet(UriTemplate = "managers/{managername}/{type}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerProperties(string managername, string type);

        [WebGet(UriTemplate = "managers/{managername}/{type}/{propertyname}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        string GetManagerProperty(string managername, string type, string propertyname);
    }
}

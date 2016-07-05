using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

using System.ServiceModel;
using System.ServiceModel.Web;

namespace NetworkAPI
{
    [DataContract]
    public class MethodParameter
    {
        string type = string.Empty;
        string name = string.Empty;
        string value = string.Empty;

        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [ServiceContract]
    public interface INetwork
    {
        [WebInvoke(UriTemplate = "call",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<MethodParameter> CallManagerMethod(List<MethodParameter> parameters);

        [WebInvoke(UriTemplate = "/managers/{managername}/call/{methodname}",
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        string testMethod(string managername, string methodname, System.IO.Stream data);

        [WebGet(UriTemplate = "assemblies/{assemblyName}", 
            BodyStyle = WebMessageBodyStyle.Bare, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetAssemblyTypes(string assemblyName);

        [WebGet(UriTemplate = "managers", 
            BodyStyle = WebMessageBodyStyle.Bare, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagers();

        [WebGet(UriTemplate = "managers/{managername}", 
            BodyStyle = WebMessageBodyStyle.Bare, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerTypes(string managername);

        [WebGet(UriTemplate = "managers/{managername}/{type}", 
            BodyStyle = WebMessageBodyStyle.Bare, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerProperties(string managername, string type);

        [WebGet(UriTemplate = "managers/{managername}/{type}/{propertyname}", 
            BodyStyle = WebMessageBodyStyle.Bare, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        string GetManagerProperty(string managername, string type, string propertyname);
    }
}

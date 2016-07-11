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
        [WebGet(UriTemplate = "/managers/{managername}/call/{methodname}?params={paramdata}",
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract(Name = "testMethod")]
        object CallManagerMethod(string managername, string methodname, string paramdata);

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
        object GetManagerProperty(string managername, string type, string propertyname);
    }
}

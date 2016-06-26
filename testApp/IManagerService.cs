using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace testApp
{
    [ServiceContract]
    public interface IManagerService
    {
        [WebGet(UriTemplate = "data/{value}", BodyStyle = WebMessageBodyStyle.Bare)]
        [OperationContract]
        string GetData(string value);
    }
}

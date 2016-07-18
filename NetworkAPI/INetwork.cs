using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetworkAPI
{
    public enum MethodType {
        GET, SET, EXECUTE
    };

    public enum ObjectType {
        CLASS, MEMBER, PARAMETER, FIELD, PROPERTY, METHOD
    };

    public class Request
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MethodType Method { get; set; }
        public NetworkObject Object { get; set; }
        public string Data { get; set; }
    }

    public class Response
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public NetworkObject Object { get; set; }
    }

    public class NetworkObject
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ObjectType Type { get; set; }
        public string Name { get; set; }
        public NetworkObject Dependency { get; set; }
        public string Assembly { get; set; }
        public bool IsStatic { get; set; }
        public string Value { get; set; }
        public IList<NetworkObject> Parameters { get; set; }
    }
}

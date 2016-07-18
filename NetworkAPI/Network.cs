using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Web.Script.Serialization;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace NetworkAPI
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
		     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Network
    {
        JavaScriptSerializer serializer;

        public Network()
        {
            serializer = new JavaScriptSerializer();
        }

        /* Takes a json-formatted string containing:
         *  {
         *    "get": [
         *      {
         *      },
         *    ]
         *  }
         *  Where "get" contains an array of object data to be 
         *  called sequentially within heirarchical scope
         *  Object data is formatted according to GetObject(...)
         */
        public object HandleRequest(string jsonRequest)
        {
            object retObj = null;
            var jsonDict = serializer.DeserializeObject(jsonRequest) as Dictionary<string, object>;
            if (jsonDict.ContainsKey("get"))
            {
                var objDictArray = jsonDict["get"] as Array;
                for (int i = 0; i < objDictArray.Length; i++)
                {
                    var objDict = objDictArray.GetValue(i) as Dictionary<string, object>;
                    retObj = GetObject(objDict, retObj);
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                        "Got object: " + (string)objDict["name"] + ": " + retObj.ToString());
                }
            }
            else
            {
                throw new Exception("Format of the JSON must be: { \"get\": [ {}, ... ] }");
            }
            return retObj;
        }

        /* Takes a Dictionary<string, object> which must contain:
         *  - name
         *  - type (object, method, ...)
         *  and may contain:
         *  - assembly
         *  - index
         *  - parameters
         *  - isStatic
         *  Where parameters is a list of objects (of arbitrary depth) 
         *  ctx is an object provided as the context
         */
        public object GetObject(Dictionary<string, object> objDict, object ctx)
        {
            object retObj = null;

            // always required to have 'name', 'type'
            string objName = objDict["name"] as string;
            string typeName = typeName = objDict["type"] as string;

            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                "Getting " + objName + " of type " + typeName + " from " + ctx);

            // optionally may have 'assembly'
            string assemblyName = "";
            if (objDict.ContainsKey("assembly"))
                assemblyName = objDict["assembly"] as string;

            // get parameters (if required)
            List<object> parameters = new List<object>();
            if (objDict.ContainsKey("parameters"))
            {
                var paramDictArray = objDict["parameters"] as Array;
                for (int i=0; i< paramDictArray.Length; i++)
                {
                    var paramDict = paramDictArray.GetValue(i) as Dictionary<string, object>;
                    object param = GetObject(paramDict, null);
                    parameters.Add(param);
                }
            }

            // get type from assembly or scope
            if (assemblyName.Length > 0)
            {
                Type t = GetAssemblyType(assemblyName, objName);
                retObj = t;
            }
            else if (ctx != null)
            {
                Type contextType = ctx as Type;
                if (contextType != null)
                {
                    ctx = null;
                }
                else
                {
                    contextType = ctx.GetType();
                    if (objDict.ContainsKey("isStatic") && (bool)objDict["isStatic"])
                        ctx = null;
                }

                if (typeName == "method")
                {
                    MethodInfo mi = (MethodInfo)contextType.GetMember(objName)[0];
                    retObj = mi.Invoke(ctx, null); // parameters
                }
                else if (typeName == "field")
                {
                    FieldInfo fi = (FieldInfo)contextType.GetMember(objName)[0];
                    retObj = fi.GetValue(ctx);
                }
                else if (typeName == "property")
                {
                    PropertyInfo pi = (PropertyInfo)contextType.GetMember(objName)[0];
                    MethodInfo mi = pi.GetAccessors()[0];
                    retObj = mi.Invoke(ctx, null); // parameters
                }
                else if (typeName == "member")
                {
                    MemberInfo[] mia = contextType.GetMember(objName);
                    foreach (var mi in mia)
                    {
                        if (mi.MemberType == MemberTypes.Method)
                        {
                            MethodInfo methodInfo = (MethodInfo)mi;
                            retObj = methodInfo.Invoke(ctx, null); // parameters
                            break;
                        }
                        else if (mi.MemberType == MemberTypes.Property)
                        {
                            PropertyInfo pi = (PropertyInfo)mi;
                            MethodInfo methodInfo = pi.GetAccessors()[0];
                            retObj = methodInfo.Invoke(ctx, null); // parameters
                            break;
                        }
                        else if (mi.MemberType == MemberTypes.Field)
                        {
                            FieldInfo fi = (FieldInfo)mi;
                            retObj = fi.GetValue(ctx);
                            break;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Must provide assembly, type or context!");
            }

            // finalize object, e.g. using the 'index' parameter
            if (objDict.ContainsKey("index"))
            {
                var arrObj = retObj as Array;
                retObj = arrObj.GetValue((int)objDict["index"]);
            }

            return retObj;
        }

        /* 
         * Handles commands of the following forms:
         * 
         *         assemblies
         *         assemblies/{assemblyName}
         *         managers
         *         managers/{managername}
         *         managers/{managername}/{type}
         *         managers/{managername}/{type}/{propertyname}
         *         managers/{managername}/call/{methodname}?params={paramdata}
         *         
         * And always returns an object which can be JSON stringified
        */
        public object ParseCommand(string command)
        {
            string[] splits = command.Split(new char[] { '?' }, 2);
            string[] commands = splits[0].Trim('/').Split('/');
            string paramData = "{}";
            Dictionary<string, object> inputParams = new Dictionary<string, object>();
            if (splits.Length > 1)
            {
                string[] d = splits[1].Split(new char[] { '=' }, 2);
                if (d.Length == 2)
                    paramData = d[1];
            }
            inputParams = serializer.DeserializeObject(paramData) as Dictionary<string, object>;

            if (commands.Length > 0)
            {
                string root = commands[0];
                if (commands.Length > 1 && commands[1] != null)
                {
                    string obj = commands[1];
                    if (commands.Length > 2 && commands[2] != null)
                    {
                        string type = commands[2];
                        if (commands.Length > 3 && commands[3] != null)
                        {
                            if (type == "call")
                            {
                                string method = commands[3];
                                return CallObjectMethod(root, obj, method, inputParams);
                            }
                            return GetObjectProperty(root, obj, type, commands[3], inputParams);
                        }
                        if (type == "call")
                        {
                            return "Error: You must provide method name and arguments!";
                        }
                        return GetObjectProperties(root, obj, type, inputParams);
                    }
                    return GetObjectTypes(root, obj, inputParams);
                }
                return GetAssemblyTypes(root, inputParams);
            }
            return new List<string> { "Assembly-CSharp", "ICities", "ColossalManaged" };
        }

        public List<string> GetAssemblyTypes(string assemblyName, Dictionary<string, object> inputParams)
        {
            List<string> types = new List<string>();
            Assembly assembly = Assembly.Load(assemblyName);
            types = assembly.GetTypes().Select(x => x.Name).ToList<string>();
            return types;
        }

        public List<string> GetObjectTypes(string assemblyName, string managername, Dictionary<string, object> inputParams)
        {
            return new List<string> { "members", "methods", "properties", "fields", "events", "nestedTypes" };
        }

        public List<string> GetObjectProperties(string assemblyName, string objName, string type, Dictionary<string, object> inputParams)
        {
            List<string> properties = new List<string>();
            try
            {
                Type t = GetAssemblyType(assemblyName, objName);
                if (type == "properties")
                {
                    properties = t.GetProperties().Select(x => x.Name).ToList<string>();
                }
                else if (type == "methods")
                {
                    properties = t.GetMethods().Select(x => x.Name).ToList<string>();
                }
                else if (type == "members")
                {
                    properties = t.GetMembers().Select(x => x.Name).ToList<string>();
                }
                else if (type == "fields")
                {
                    properties = t.GetFields().Select(x => x.Name).ToList<string>();
                }
                else if (type == "events")
                {
                    properties = t.GetEvents().Select(x => x.Name).ToList<string>();
                }
                else if (type == "nestedTypes")
                {
                    properties = t.GetNestedTypes().Select(x => x.Name).ToList<string>();
                }
            }
            catch (Exception e)
            {
                properties.Add(e.Message);
            }
            return properties;
        }

        public Type GetAssemblyType(string assemblyName, string typeName)
        {
            Type t;
            try
            {
                t = Assembly.Load(assemblyName).GetType(typeName);
            }
            catch (Exception e)
            {
                throw new Exception("Assembly "+assemblyName+" does not contain " + typeName + "!");
            }
            return t;
        }

        public object GetInstance(string assemblyName, string typeName)
        {
            // get the instance of the object:
            Type t = GetAssemblyType(assemblyName, typeName);
            PropertyInfo instancePropInfo = (PropertyInfo)t.GetMember("instance")[0];
            MethodInfo instanceMethodInfo = instancePropInfo.GetAccessors()[0];
            return instanceMethodInfo.Invoke(null, null);
        }

        public object GetPropertyValue(string assemblyName, string objName, string name, Dictionary<string, object> inputParams)
        {
            object retObj;
            object manager = GetInstance(assemblyName, objName);
            Type t = GetAssemblyType(assemblyName, objName);
            MethodInfo mi = t.GetProperty(name).GetGetMethod();
            retObj = mi.Invoke(manager, null);
            if (inputParams.ContainsKey("index"))
            {
                int index = (int)inputParams["index"];
                retObj = ((Array)retObj).GetValue(index);
            }
            return retObj;
        }

        public object GetObjectMethod(string assemblyName, string objName, string methodname, Dictionary<string, object> inputParams)
        {
            Type t = GetAssemblyType(assemblyName, objName);
            MemberInfo[] m;
            try
            {
                m = t.GetMember(methodname);
            }
            catch (Exception e)
            {
                throw new Exception("Object " + objName + " does not have member: " + methodname);
            }
            List<Dictionary<string, string>> parameters = new List<Dictionary<string, string>>();
            try
            {
                foreach (ParameterInfo pi in ((MethodInfo)m[0]).GetParameters())
                {
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("name", pi.Name);
                    param.Add("position", pi.Position.ToString());
                    param.Add("type", pi.ParameterType.ToString());
                    param.Add("default value", pi.DefaultValue.ToString());
                    parameters.Add(param);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Method " + methodname + " is not a valid method!");
            }
            return parameters;
        }

        public object GetObjectProperty(string assemblyName, string objName, string type, string propertyname, Dictionary<string, object> inputParams)
        {
            try
            {
                if (type == "properties")
                {
                    return GetPropertyValue(assemblyName, objName, propertyname, inputParams);
                }
                else if (type == "methods")
                {
                    return GetObjectMethod(assemblyName, objName, propertyname, inputParams);
                }
                else if (type == "members")
                {
                    Type t = GetAssemblyType(assemblyName, objName);
                    MemberInfo[] pa = t.GetMember(propertyname);
                    List<object> retDict = new List<object>();
                    foreach (var p in pa)
                    {
                        object[] attrs = p.GetCustomAttributes(false);
                        foreach (object o in attrs)
                        {
                            retDict.Add(o.ToString());
                        }
                        if (p.MemberType == MemberTypes.Method)
                        {
                            retDict.Add(GetObjectMethod(assemblyName, objName, p.Name, inputParams));
                        }
                        if (p.MemberType == MemberTypes.Property)
                        {
                            retDict.Add(GetPropertyValue(assemblyName, objName, propertyname, inputParams));
                        }
                    }
                    return retDict;
                }
                else if (type == "fields")
                {
                    Type t = GetAssemblyType(assemblyName, objName);
                    FieldInfo p = t.GetField(propertyname);
                    object manager = GetInstance(assemblyName, objName);
                    return p.GetValue(manager);
                }
                else if (type == "events")
                {
                }
                else if (type == "nestedTypes")
                {
                    Type t = GetAssemblyType(assemblyName, objName);
                    Type nt = t.GetNestedType(propertyname);
                    return nt.GetMembers().Select(x => x.Name).ToList<string>();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Unhandled method!";
        }

        public object CallObjectMethod(string assemblyName, string objName, string methodname, Dictionary<string, object> inputParams)
        {
            List<Dictionary<string, string>> paramDefs = new List<Dictionary<string, string>>();

            Type t = GetAssemblyType(assemblyName, objName);
            MethodInfo mi = t.GetMethod(methodname);
            object instance = null;

            /*
            Type rbai = getAssemblyType("Assembly-CSharp", "RoadBaseAI");
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, rbai.ToString());

            ushort nodeId;
            ushort segmentId;
            var currentFrameIndex = SimulationManager.instance.m_currentFrameIndex;
            RoadBaseAI.TrafficLightState vehicleLightState;
            RoadBaseAI.TrafficLightState pedestrianLightState;
            bool vehicles;
            bool pedestrians;
            RoadBaseAI.GetTrafficLightState(
                nodeID,
                ref NetManager.instance.m_segments.m_buffer[segmentId],
                currentFrameIndex - 256u,
                out vehicleLightState,
                out pedestrianLightState,
                out vehicles,
                out pedestrians
                );
            */

            try
            {
                paramDefs = GetObjectMethod(assemblyName, objName, methodname, null) as List<Dictionary<string, string>>;
                if (inputParams.ContainsKey("useInstance") && (bool)inputParams["useInstance"])
                {
                    instance = GetInstance(assemblyName, objName);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            if (paramDefs == null || paramDefs.Count == 0)
            {
                return mi.Invoke(instance, null);
            }
            else if (inputParams != null && inputParams.Count > 0 && ParametersMatchDefinitions(paramDefs, inputParams))
            {
                List<object> parameters = ConvertParameters(paramDefs, inputParams);
                return mi.Invoke(instance, parameters.ToArray());
            }
            else
            {
                return "Method requires " + paramDefs.Count + " parameters!";
            }
        }

        public object ResolveParameter(Dictionary<string, object> param)
        {
            object parameter = null;
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                (string)param["name"] + ": " +(string)param["type"]);

            param["type"] = ((string)param["type"]).Trim('&');
            if (param.ContainsKey("assembly"))
            {
                parameter = Activator.CreateInstance((string)param["assembly"], (string)param["type"]);
            }
            else
            {
                Type t = Type.GetType((string)param["type"]);
                if (t != null)
                    parameter = Activator.CreateInstance(t);
            }
            if (parameter != null)
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    (string)param["type"] + ": " + serializer.Serialize(parameter));
            return parameter;
        }

        public List<object> ConvertParameters(List<Dictionary<string, string>> defs, Dictionary<string, object> parameters)
        {
            List<object> parameterObjects = new List<object>();
            var paramArray = parameters["parameters"] as Array;
            for (int i=0; i < paramArray.Length; i++)
            {
                Dictionary<string, object> p = (Dictionary<string, object>)paramArray.GetValue(i);
                object o = ResolveParameter(p);
                //parameterObjects.Add(o);
            }
            return parameterObjects;
        }

        public bool ParametersMatchDefinitions(List<Dictionary<string, string>> defs, Dictionary<string, object> parameters)
        {
            // check here if the method's parameter definitions match the input parameter spec
            int numDefs = defs.Count;
            var paramArray = parameters["parameters"] as Array;
            int numParams = paramArray.Length;
            if (numDefs > numParams)
                return false;
            for (int i=0; i < paramArray.Length; i++)
            {
                Dictionary<string, object> t = (Dictionary<string, object>)paramArray.GetValue(i);

                if ((string)t["type"] != (string)defs[i]["type"] ||
                    (string)t["name"] != (string)defs[i]["name"])
                {
                    string msg = "Parameter: " + t["name"] + ", " + t["type"] + " is incorrect, should be: " + defs[i]["name"] + ", " + defs[i]["type"];
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, msg);
                    throw new Exception(msg);
                }
            }
            return true;
        }
    }
}

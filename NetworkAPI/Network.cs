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
            if (splits.Length > 1)
            {
                string[] d = splits[1].Split(new char[] { '=' }, 2);
                if (d.Length == 2)
                    paramData = d[1];
            }
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
                                return CallObjectMethod(root, obj, method, paramData);
                            }
                            return GetObjectProperty(root, obj, type, commands[3]);
                        }
                        if (type == "call")
                        {
                            return "Error: You must provide method name and arguments!";
                        }
                        return GetObjectProperties(root, obj, type);
                    }
                    return GetObjectTypes(root, obj);
                }
                return GetAssemblyTypes(root);
            }
            return new List<string> { "Assembly-CSharp", "ICities", "ColossalManaged" };
        }

        public List<string> GetAssemblyTypes(string assemblyName)
        {
            List<string> types = new List<string>();
            Assembly assembly = Assembly.Load(assemblyName);
            types = assembly.GetTypes().Select(x => x.Name).ToList<string>();
            return types;
        }

        public List<string> GetObjectTypes(string assemblyName, string managername)
        {
            return new List<string> { "members", "methods", "properties", "fields", "events", "nestedTypes" };
        }

        public List<string> GetObjectProperties(string assemblyName, string objName, string type)
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

        public object GetPropertyValue(string assemblyName, string objName, string name)
        {
            object retObj;
            object manager = GetInstance(assemblyName, objName);
            Type t = GetAssemblyType(assemblyName, objName);
            MethodInfo mi = t.GetProperty(name).GetGetMethod();
            retObj = mi.Invoke(manager, null);
            return retObj;
        }

        public object GetObjectMethod(string assemblyName, string objName, string methodname)
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

        public object GetObjectProperty(string assemblyName, string objName, string type, string propertyname)
        {
            try
            {
                if (type == "properties")
                {
                    return GetPropertyValue(assemblyName, objName, propertyname);
                }
                else if (type == "methods")
                {
                    return GetObjectMethod(assemblyName, objName, propertyname);
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
                            retDict.Add(GetObjectMethod(assemblyName, objName, p.Name));
                        }
                        if (p.MemberType == MemberTypes.Property)
                        {
                            retDict.Add(GetPropertyValue(assemblyName, objName, propertyname));
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

        public object CallObjectMethod(string assemblyName, string objName, string methodname, string paramdata)
        {
            List<Dictionary<string, string>> paramDefs = new List<Dictionary<string, string>>();
            Dictionary<string, object> inputParams = new Dictionary<string, object>();

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
                paramDefs = GetObjectMethod(assemblyName, objName, methodname) as List<Dictionary<string, string>>;
                inputParams = serializer.DeserializeObject(paramdata) as Dictionary<string, object>;
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

        public List<object> ConvertParameters(List<Dictionary<string, string>> defs, Dictionary<string, object> parameters)
        {
            List<object> parameterObjects = new List<object>();
            var paramArray = parameters["parameters"] as Array;
            for (int i=0; i < paramArray.Length; i++)
            {
                Dictionary<string, object> p = (Dictionary<string, object>)paramArray.GetValue(i);
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    (string)p["name"] + ": " +(string)p["type"]);
                Type t = Type.GetType((string)p["type"]);
                /*
                object o = Activator.CreateInstance(t);
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                (string)p["type"] + ": " + serializer.Serialize(o));
                parameterObjects.Add(o);
                */
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

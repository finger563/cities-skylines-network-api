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
    public class Network : INetwork
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
            string[] commands = command.Trim('/').Split('/');
            if (commands.Length > 0)
            {
                string root = commands[0];
                if (root == "managers")
                {
                    if (commands.Length > 1 && commands[1] != null)
                    {
                        string mgr = commands[1];
                        if (commands.Length > 2 && commands[2] != null)
                        {
                            string type = commands[2];
                            if (commands.Length > 3 && commands[3] != null)
                            {
                                if (type == "call")
                                {
                                    string[] data = commands[3].Split(new char[]{'?'}, 2);
                                    string method = data[0];
                                    string paramData = "{}";
                                    if (data.Length > 1)
                                    {
                                        if (data.Length == 2)
                                        {
                                            string[] d = data[1].Split(new char[] { '=' }, 2);
                                            if (d.Length == 2)
                                                paramData = d[1];
                                        }
                                    }
                                    return CallManagerMethod(mgr, method, paramData);
                                }
                                return GetManagerProperty(mgr, type, commands[3]);
                            }
                            if (type == "call")
                            {
                                return "Error: You must provide method name and arguments!";
                            }
                            return GetManagerProperties(mgr, type);
                        }
                        return GetManagerTypes(mgr);
                    }
                    return GetManagers();
                }
                else if (root == "assemblies")
                {
                    if (commands.Length > 1 && commands[1] != null)
                    {
                        return GetAssemblyTypes(commands[1]);
                    }
                    return new List<string> { "Assembly-CSharp", "ICities", "ColossalManaged" };
                }
            }
            return new List<string> { "managers", "assemblies"};
        }

        public List<string> GetAssemblyTypes(string assemblyName)
        {
            List<string> types = new List<string>();
            Assembly assembly = Assembly.Load(assemblyName);
            types = assembly.GetTypes().Select(x => x.Name).ToList<string>();
            return types;
        }

        public List<string> GetManagers()
        {
            List<string> managers = new List<string>();
            try
            {
                Assembly assembly = Assembly.Load("Assembly-CSharp");
                managers = assembly.GetTypes()
                    .Where(x => x.Name.IndexOf("Manager") > -1)
                    .Select(x => x.Name).ToList<string>();
            }
            catch (Exception e)
            {
                managers.Add(e.Message);
            }
            return managers;
        }

        public List<string> GetManagerTypes(string managername)
        {
            return new List<string> { "members", "methods", "properties", "fields", "events", "nestedTypes" };
        }
    
    public List<string> GetManagerProperties(string managername, string type)
        {
            List<string> properties = new List<string>();
            properties.Add("GetManagerProperties:: ");
            try
            {
                Assembly assembly = Assembly.Load("Assembly-CSharp");
                Type t = assembly.GetType(managername);
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

        public Type getAssemblyType(string assemblyName, string typeName)
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

        public object getInstance(string assemblyName, string typeName)
        {
            // get the instance of the manager here:
            Type t = getAssemblyType( assemblyName, typeName);
            PropertyInfo instancePropInfo = (PropertyInfo)t.GetMember("instance")[0];
            MethodInfo instanceMethodInfo = instancePropInfo.GetAccessors()[0];
            return instanceMethodInfo.Invoke(null, null);
        }

        public object getPropertyValue(string managername, string name)
        {
            object retObj;
            object manager = getInstance("Assembly-CSharp", managername);
            Type t = getAssemblyType("Assembly-CSharp", managername);
            MethodInfo mi = t.GetProperty(name).GetGetMethod();
            retObj = mi.Invoke(manager, null);
            return retObj;
        }

        public object GetManagerMethod(string managername, string methodname)
        {
            Type t = getAssemblyType("Assembly-CSharp", managername);
            MemberInfo[] m;
            try
            {
                m = t.GetMember(methodname);
            }
            catch (Exception e)
            {
                throw new Exception("Manager " + managername + " does not have member: " + methodname);
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

        public object GetManagerProperty(string managername, string type, string propertyname)
        {
            try
            {
                Type t = getAssemblyType("Assembly-CSharp", managername);
                object manager = getInstance("Assembly-CSharp", managername);

                if (type == "properties")
                {
                    return getPropertyValue(managername, propertyname);
                }
                else if (type == "methods")
                {
                    return GetManagerMethod(managername, propertyname);
                }
                else if (type == "members")
                {
                    MemberInfo[] pa = t.GetMember(propertyname);
                    Dictionary<string, object> retDict = new Dictionary<string, object>();
                    foreach (var p in pa)
                    {
                        object[] attrs = p.GetCustomAttributes(false);
                        foreach (object o in attrs)
                        {
                            retDict.Add(o.ToString(), o.ToString());
                        }
                        if (p.MemberType == MemberTypes.Method)
                        {
                            retDict.Add(p.Name, GetManagerMethod(managername, p.Name));
                        }
                        if (p.MemberType == MemberTypes.Property)
                        {
                            retDict.Add(p.Name, getPropertyValue(managername, propertyname));
                        }
                    }
                    return retDict;
                }
                else if (type == "fields")
                {
                    FieldInfo p = t.GetField(propertyname);
                    return p.GetValue(manager);
                }
                else if (type == "events")
                {
                }
                else if (type == "nestedTypes")
                {
                    //Type nt = t.GetNestedType(propertyname);
                    Citizen testObj = new Citizen();
                    return testObj;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Unhandled method!";
        }

        public object CallManagerMethod(string managername, string methodname, string paramdata)
        {
            string debugMessage = "Received GET for CallManagerMethod: " + paramdata;
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, debugMessage);
            Console.WriteLine(debugMessage);

            List<Dictionary<string, string>> paramDefs = new List<Dictionary<string, string>>();
            Dictionary<string, object> inputParams = new Dictionary<string, object>();

            try
            {
                paramDefs = GetManagerMethod(managername, methodname) as List<Dictionary<string, string>>;
                inputParams = serializer.DeserializeObject(paramdata) as Dictionary<string, object>;
            }
            catch (Exception e)
            {
                return e.Message;
            }

            if (paramDefs == null || paramDefs.Count == 0)
            {
                return "Method doesn't require parameters";
            }
            else if (inputParams != null && inputParams.Count > 0 && ParametersMachDefinitions(paramDefs, inputParams))
            {
                return inputParams;
            }
            else
            {
                return "Method requires " + paramDefs.Count + " parameters!";
            }
        }

        public bool ParametersMachDefinitions(List<Dictionary<string, string>> defs, Dictionary<string, object> parameters)
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

                if ((string)t["type"] != (string)defs[i]["type"])
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
                        "Parameter: " + t["name"] + " has wrong type: " + t["type"] + ", should be: " + defs[i]["type"]);
                    return false;
                }
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.ObjectModel;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace NetworkAPI
{
    public class Network
    {
        public object HandleRequest(string jsonRequest)
        {
            object retObj = null;
            Request request;
            try
            {
                request = JsonConvert.DeserializeObject<Request>(jsonRequest);
                if (request.Method == MethodType.GET)
                {
                    retObj = GetObject(request.Object);
                }
                else if (request.Method == MethodType.SET)
                {
                    
                }
                else if (request.Method == MethodType.EXECUTE)
                {

                }
                else
                {
                    throw new Exception("Error: unsupported method type!");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error: request not properly formatted: " + e.Message);
            }
            return retObj;
        }

        public object GetObject(NetworkObject obj)
        {
            object retObj = null;

            // get required/dependent context now (recursively)
            object ctx = null;
            if (obj.Dependency != null)
            {
                ctx = GetObject(obj.Dependency);
            }

            /*
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
            "Getting: " + obj.Name + " of type: " + obj.Type + " from context:" + ctx);
            */

            // get parameter data now
            
            // get object data now
            if (obj.Type == ObjectType.CLASS)
            {
                Type t = GetAssemblyType(obj.Assembly, obj.Name);
                retObj = t;
            }
            else if (obj.Type == ObjectType.MEMBER)
            {
                if (ctx != null)
                {
                    Type contextType = ctx as Type;
                    if (contextType != null)
                    {
                        ctx = null;
                    }
                    else
                    {
                        contextType = ctx.GetType();
                    }
                    if (obj.IsStatic)
                    {
                        ctx = null;
                    }
                    MemberInfo[] mia = contextType.GetMember(obj.Name);
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
            else if (obj.Type == ObjectType.PARAMETER)
            {
            }
            else if (obj.Type == ObjectType.METHOD)
            {
                // get parameters (if required)
                List<object> parameters = new List<object>();
            }
            else
            {
            }

            // set the value of the object if it exists

            return retObj;
        }

        public Type GetAssemblyType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}

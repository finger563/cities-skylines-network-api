using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

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
            Type contextType = null;
            object ctx = null;
            if (obj.Dependency != null)
            {
                ctx = GetObject(obj.Dependency);
            }
            if (ctx != null)
            {
                contextType = ctx as Type;
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
            }

            /*
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
            "Getting: " + obj.Name + " of type: " + obj.Type + " from context:" + ctx);
            */

            // get object data now
            if (obj.Type == ObjectType.CLASS)
            {
                Type t = GetAssemblyType(obj.Assembly, obj.Name);
                retObj = t;
            }
            else if (obj.Type == ObjectType.MEMBER)
            {
                if (contextType != null)
                {
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
            else if (obj.Type == ObjectType.PARAMETER) // do we need this type?
            {
            }
            else if (obj.Type == ObjectType.METHOD)
            {
                if (contextType != null)
                {
                    // get parameters (if they exist)
                    List<object> parameters = new List<object>();
                    for (int i = 0; i < obj.Parameters.Count; i++)
                    {
                        object param = GetObject(obj.Parameters.ElementAt(i));
                        parameters.Add(param);
                    }
                    // call the method here
                    MethodInfo mi = contextType.GetMethod(obj.Name);
                    MethodInfo methodInfo = (MethodInfo)mi;
                    retObj = methodInfo.Invoke(ctx, parameters.ToArray()); // parameters
                }
            }
            else
            {
            }

            // set the value of the object if it exists
            if (obj.Value != null)
            {
                Type t = Type.GetType(obj.ValueType);
                if (t == null)
                {
                    t = GetAssemblyType(obj.Assembly, obj.ValueType);
                }
                retObj = Convert.ChangeType(obj.Value, t);  // need to figure out here how to decide what to do
                retObj = Enum.Parse(t, obj.Value);
            }

            return retObj;
        }

        public Type GetAssemblyType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        public void SetValueFromString(object target, string propertyName, string propertyValue)
        {
            PropertyInfo pi = target.GetType().GetProperty(propertyName);
            Type t = pi.PropertyType;

            if (t.IsGenericType &&
                t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (propertyValue == null)
                {
                    pi.SetValue(target, null, null);
                    return;
                }
                t = new NullableConverter(pi.PropertyType).UnderlyingType;
            }
            pi.SetValue(target, Convert.ChangeType(propertyValue, t), null);
        }
    }
}

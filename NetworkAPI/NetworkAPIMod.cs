using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

using System.ServiceModel.Channels;

namespace NetworkAPI
{
    public class NetworkAPIMod : IUserMod
    {
        public string Name { get { return "Network API"; } }
        public string Description { get { return "This mod exposes the Cities: Skylines Data and Interfaces Through Sockets."; } }
    }

    public class ThreadingExension : ThreadingExtensionBase
    {
        WebServiceHost server;

        public class MyMapper : WebContentTypeMapper
        {
            public override WebContentFormat GetMessageFormatForContentType(string contentType)
            {
                if (contentType.IndexOf("application/json") > -1)
                    return WebContentFormat.Json;
                else if (contentType.IndexOf("application/xml") > -1)
                    return WebContentFormat.Xml;
                else
                    return WebContentFormat.Raw;
            }
        }

        static Binding GetBinding()
        {
            WebHttpBinding b = new WebHttpBinding();
            b.TransferMode = TransferMode.Streamed;
            CustomBinding result = new CustomBinding(b);
            WebMessageEncodingBindingElement webMEBE = result.Elements.Find<WebMessageEncodingBindingElement>();
            webMEBE.ContentTypeMapper = new MyMapper();
            return result;
        }

        public override void OnCreated(IThreading threading)
        {
            try
            {
                Uri baseAddress = new Uri("http://localhost:8080/");
                
                server = new WebServiceHost(typeof(NetworkAPI.Network), baseAddress);

                ServiceDebugBehavior sdb = server.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = true;
                sdb.HttpHelpPageUrl = new Uri("http://localhost:8080/help");
                sdb.IncludeExceptionDetailInFaults = true;

                server.AddServiceEndpoint(typeof(NetworkAPI.INetwork), GetBinding(), "").Behaviors.Add(new WebHttpBehavior());

                server.Open();

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Server open on " + baseAddress.ToString());
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
                    "Error: " + e.Message);
                Console.WriteLine("Error: " + e.Message);
            }
            base.OnCreated(threading);
        }

        public override void OnReleased()
        {
            base.OnReleased();
            server.Close();
        }

    }

} 
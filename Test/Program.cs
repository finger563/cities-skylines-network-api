using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System.Linq;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

using System.ServiceModel.Channels;

using NetworkAPI;

namespace Test
{
    class Program
    {
        public class MyMapper : WebContentTypeMapper
        {
            public override WebContentFormat GetMessageFormatForContentType(string contentType)
            {
                return WebContentFormat.Raw;
            }
        }

        static Binding GetBinding()
        {
            CustomBinding result = new CustomBinding(new WebHttpBinding());
            WebMessageEncodingBindingElement webMEBE = result.Elements.Find<WebMessageEncodingBindingElement>();
            webMEBE.ContentTypeMapper = new MyMapper();
            return result;
        }

        static void Main(string[] args)
        {

            WebServiceHost server;

            try
            {
                Uri baseAddress = new Uri("http://localhost:4040/");
                server = new WebServiceHost(typeof(NetworkAPI.Network), baseAddress);

                ServiceDebugBehavior sdb = server.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = true;
                sdb.HttpHelpPageUrl = new Uri("http://localhost:4040/help");
                sdb.IncludeExceptionDetailInFaults = true;

                server.AddServiceEndpoint(typeof(INetwork), GetBinding(), "").Behaviors.Add(new WebHttpBehavior());

                server.Open();
                Console.WriteLine("Server up.");
                Console.Read();
                server.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.Read();
            }
        }
    }
}

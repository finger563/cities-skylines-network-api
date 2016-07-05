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

using NetworkAPI;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            WebServiceHost server;
            ServiceEndpoint ep;
            WebHttpBehavior behavior;
            WebHttpBinding binding;

            try
            {
                Uri baseAddress = new Uri("http://localhost:4040/");
                server = new WebServiceHost(typeof(NetworkAPI.Network), baseAddress);
                ServiceDebugBehavior sdb = server.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = true;
                sdb.HttpHelpPageUrl = new Uri("http://localhost:4040/help");
                sdb.IncludeExceptionDetailInFaults = true;
                binding = new WebHttpBinding();
                //binding.ContentTypeMapper = new NetworkAPI.JsonContentTypeMapper();
                ep = server.AddServiceEndpoint(typeof(INetwork), binding, "");
                behavior = new WebHttpBehavior();
                behavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
                behavior.DefaultOutgoingResponseFormat = WebMessageFormat.Json;
                ep.Behaviors.Add(behavior);
                //ep.Behaviors.Add(new WebScriptEnablingBehavior());
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

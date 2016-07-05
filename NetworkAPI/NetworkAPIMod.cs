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
        ServiceEndpoint ep;
        WebHttpBehavior behavior;
        WebHttpBinding binding;

        public override void OnCreated(IThreading threading)
        {
            try
            {
                Uri baseAddress = new Uri("http://localhost:8080/");

                binding = new WebHttpBinding();

                behavior = new WebHttpBehavior();
                //behavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
                //behavior.DefaultOutgoingResponseFormat = WebMessageFormat.Json;

                server = new WebServiceHost(typeof(Network), baseAddress);

                //ServiceDebugBehavior sdb = server.Description.Behaviors.Find<ServiceDebugBehavior>();
                //sdb.HttpHelpPageEnabled = false;
                //sdb.HttpHelpPageUrl = new Uri("http://localhost:8080/help");
                //sdb.HttpHelpPageBinding = binding;
                //sdb.IncludeExceptionDetailInFaults = true;

                ep = server.AddServiceEndpoint(typeof(INetwork), binding, "");
                ep.Behaviors.Add(behavior);
                //ep.Behaviors.Add(new WebScriptEnablingBehavior());

                server.Open();
            }
            catch (Exception e)
            {
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
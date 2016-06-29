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

        public override void OnCreated(IThreading threading)
        {
            try
            {
                Uri baseAddress = new Uri("http://localhost:8080/");
                server = new WebServiceHost(typeof(Network), baseAddress);
                ServiceDebugBehavior sdb = server.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = false;
                ep = server.AddServiceEndpoint(typeof(INetwork), new WebHttpBinding(), "");
                //ep.Behaviors.Add(new WebHttpBehavior());
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
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
using System.ServiceModel.Web;

using System.Web.Script.Serialization;

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

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);
            try
            {
                server = new WebServiceHost(typeof(Network),
                    new Uri("http://localhost:8080/managerservice"));
                server.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            server.Close();
        }

    }

} 
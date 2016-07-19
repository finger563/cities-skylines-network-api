using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using Newtonsoft.Json;

using System.Linq;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

namespace NetworkAPI
{
    public class NetworkAPIMod : IUserMod
    {
        public string Name { get { return "Network API"; } }
        public string Description { get { return "This mod exposes the Cities: Skylines Data and Interfaces Through Sockets."; } }
    }

    public class ThreadingExension : ThreadingExtensionBase
    {
        UdpClient listener;
        Thread listenerThread;
        NetworkAPI.Network networkAPI;

        public void ListenerThreadFunc()
        {
            while (true)
            {
                byte[] data = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    data = listener.Receive(ref sender);
                }
                catch (Exception e)
                {
                    continue;
                }

                string command = Encoding.ASCII.GetString(data, 0, data.Length);

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Got connection from: " + sender.ToString());

                string response = "";
                try
                {
                    response = JsonConvert.SerializeObject(networkAPI.HandleRequest(command));
                }
                catch (Exception e)
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
                        e.Message);
                    Debug.Log(e.Message);
                    response = JsonConvert.SerializeObject(e.Message);
                }
                
                data = Encoding.ASCII.GetBytes(response);
                listener.Send(data, data.Length, sender);
            }
        }

        public override void OnCreated(IThreading threading)
        {
            try
            {
                networkAPI = new NetworkAPI.Network();

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 11000);
                listener = new UdpClient(ipep);
                listener.Client.ReceiveTimeout = 50;
                listenerThread = new Thread(new ThreadStart(this.ListenerThreadFunc));
                listenerThread.Start();
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Server up");
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
            listenerThread.Abort();
            listener.Close();
        }

    }

} 

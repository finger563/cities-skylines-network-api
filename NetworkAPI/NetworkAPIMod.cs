using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System;
using System.Reflection;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using NetworkAPI;

//many parts taken from:
//https://github.com/AlexanderDzhoganov/Skylines-DynamicResolution/

namespace NetworkAPI
{

    public class NetworkAPIMod : IUserMod
    {
        public string Name { get { return "Network API"; } }
        public string Description { get { return "This mod exposes the Cities: Skylines Data and Interfaces Through Sockets."; } }
    }

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }

            // this seems to get the default UIView
            UIView uiView = UIView.GetAView();

            // example for adding a button

            // Add a new button to the view.
            var button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

            // Set the text to show on the button.
            button.text = "Start Server";

            // Set the button dimensions.
            button.width = 200;
            button.height = 30;

            // Style the button to look like a menu button.
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);

            // Enable button sounds.
            button.playAudioEvents = true;

            //set button position
            button.transformPosition = new Vector3(0.8f, 0.95f);
            button.BringToFront();

            try
            {
                //get the names of any districts in the city
                DistrictManager dm = DistrictManager.instance;

                // example for iterating through the structures
                int dCount = 0;
                uint maxDCount = dm.m_districts.m_size;

                Debug.Log ("District maxDCount: " + maxDCount);
                for (int i = 0; i < maxDCount; i++) {
                    String d = dm.GetDistrictName(i);
                    if (d != null && !d.Equals("")) {
                        dCount += 1;
                    }
                }

            } catch (Exception e) {
                Debug.Log("Error in detecting district names");
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }


            try
            {
                //get the names of any citizens in the city
                CitizenManager cm = CitizenManager.instance;

                // example for iterating through the structures
                int cCount = 0;
                int maxCCount = cm.m_citizenCount;

                Debug.Log ("Citizen maxCCount: " + maxCCount);

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Citizen maxCCount: " + maxCCount);

                for (int i = 0; i < maxCCount; i++) {
                    String c = cm.GetCitizenName((uint)i);
                    if (c != null && !c.Equals("")) {
                        cCount += 1;
                    }
                }

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Actual Citizens: " + cCount);

            } catch (Exception e) {
                Debug.Log("Error in detecting citizen names");
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }

        }

    }

    public class ThreadingExension : ThreadingExtensionBase
    {

        UdpClient listener;

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 11000);
                listener = new UdpClient(ipep);
                listener.Client.ReceiveTimeout = 50;
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Error creating listener: " + e.Message);
            }
        }

        public override void OnAfterSimulationTick()
        {
            try
            {
                byte[] data = new byte[1024];

                //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Receiving!");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                data = listener.Receive(ref sender);

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Got connection from: " + sender.ToString()  + ", message: " +
                    Encoding.ASCII.GetString(data, 0, data.Length));
                
                string welcome = "Welcome to test server";
                data = Encoding.ASCII.GetBytes(welcome);
                listener.Send(data, data.Length, sender);

            }
            catch (Exception e)
            {
                //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                //"Exception: " + e.Message);
            }
        }

    }

} 
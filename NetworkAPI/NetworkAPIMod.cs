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
using System.Threading.Tasks;

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
                for (int i = 0; i < maxCCount; i++) {
                    String c = cm.GetCitizenName((uint)i);
                    if (c != null && !c.Equals("")) {
                        cCount += 1;
                    }
                }

            } catch (Exception e) {
                Debug.Log("Error in detecting citizen names");
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }

        }

    }

    public struct Received
    {
        public IPEndPoint Sender;
        public string Message;
    }

    abstract class UdpBase
    {
        protected UdpClient Client;

        protected UdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }
    }

    //Server
    class UdpListener : UdpBase
    {
        private IPEndPoint _listenOn;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, 32123))
        {
        }

        public UdpListener(IPEndPoint endpoint)
        {
            _listenOn = endpoint;
            Client = new UdpClient(_listenOn);
        }
        
        public void Reply(string message,IPEndPoint endpoint)
        {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length, endpoint);
        }

    }

    public class ThreadingExension : ThreadingExtensionBase
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        string host;
        int port;

        public void AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            listener.Bind(localEndPoint);
        }

        public override void OnAfterSimulationTick()
        {
        }
    }

} 
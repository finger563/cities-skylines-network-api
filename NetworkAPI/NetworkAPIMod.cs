using System;

using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Reflection;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Web;

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

    [ServiceContract]
    public interface IManagerService
    {
        [WebGet(UriTemplate = "managers/{id}", BodyStyle = WebMessageBodyStyle.Bare)]
        [OperationContract]
        string GetManagerNameById(string id);
    }

    public class ManagerService : IManagerService
    {
        public string GetManagerNameById(string id)
        {
            System.Random r = new System.Random();
            string returnString = "";
            int idNum = Convert.ToInt32(id);
            for (int i = 0; i < idNum; i++)
                returnString += char.ConvertFromUtf32(r.Next(65, 85));
            return returnString;
        }
    }

    public class Server
    {
        WebServiceHost host;

        public Server()
        {
            Start();
        }

        public void Start()
        {
            try
            {
                host = new WebServiceHost(typeof(ManagerService),
                    new Uri("http://localhost:8080/managerservice"));
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        ~Server()
        {
            host.Close();
        }
    }

    public class ThreadingExension : ThreadingExtensionBase
    {

        UdpClient listener;
        string assemblyString;

        public void InspectType(Type t)
        {
            assemblyString += t.Name + ":" + Environment.NewLine;
            PropertyInfo[] pia = t.GetProperties();
            foreach (PropertyInfo pi in pia)
            {
                assemblyString += "\t" + pi.PropertyType + " " +  pi.Name + " { get; set; }\n";
            }
            MethodInfo[] mia = t.GetMethods();
            foreach (MethodInfo mi in mia)
            {
                assemblyString += "\t" + mi.ReturnType + " " + mi.Name + "(";
                ParameterInfo[] paramia = mi.GetParameters();
                foreach (ParameterInfo parami in paramia)
                {
                    assemblyString += parami.ParameterType + " "
                        + parami.Name + ",";
                }
                assemblyString += ");\n";
            }
            MemberInfo[] memia = t.GetMembers();
            foreach (MemberInfo memi in memia)
            {
                assemblyString += "\t" + memi.MemberType + " " + memi.Name + ";\n";
            }
        }

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);

            Server server = new Server();

            try
            {
                NetManager nm = NetManager.instance;
                InspectType(nm.GetType());
                VehicleManager vm = VehicleManager.instance;
                InspectType(vm.GetType());
                CitizenManager cm = CitizenManager.instance;
                InspectType(cm.GetType());
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Error inspecting class: " + e.Message);
            }

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

        public override void OnReleased()
        {
            base.OnReleased();
            listener.Close();
            //_serviceHost.Close();
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
                
                string welcome = "Welcome to test server:"+Environment.NewLine+ assemblyString;
                data = Encoding.ASCII.GetBytes(welcome);
                listener.Send(data, data.Length, sender);

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                /*
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                "Exception: " + e.Message);
                */
            }
        }

    }

} 
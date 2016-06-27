using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using testApp;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(args.Length);
            Server server = new Server();
            Console.Read();
        }
    }
    class Server
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
}

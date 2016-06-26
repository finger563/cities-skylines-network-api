using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using testApp;

using System.ServiceModel;

using System.ServiceModel.Web;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(args.Length);
            WebServiceHost host = new WebServiceHost(typeof(ManagerService),
                new Uri("http://localhost:8080/managerservice"));
            host.Open();

            Console.Read();

            host.Close();
        }
    }
}

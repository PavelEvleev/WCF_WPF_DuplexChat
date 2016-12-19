using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using WCF_Duplex_Chat_Svc;

namespace WCF_Host
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(Duplex_ChatService));
            Console.WriteLine("'''''''''''''''");
            host.Open();
            Console.WriteLine("enter any key to stop");
            Console.ReadLine();
            host.Close();

        }
    }
}

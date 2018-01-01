using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nancy;
using Nancy.Hosting.Self;



namespace ZopaTest.Nancy.SelfHost
{
    // a simple module to be hosted in the console app
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = _ => { return "Hello World from Nancy!"; };

            Get["/{loan_amount}"] = 
                parameters => 
                {
                    var market_file = string.IsNullOrEmpty((string)this.Request.Query["market_file"]) ? @"..\..\..\ZopaTest\market_offers\Market Data for Exercise.csv" : (string)this.Request.Query["market_file"];
                    var quote = ZopaTest.Interop.getQuote(market_file, parameters.loan_amount);

                    return View["QuoteView", quote];
                };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HostConfiguration hostConfigs = new HostConfiguration();
            hostConfigs.UrlReservations.CreateAutomatically = true;

            // initialize an instance of NancyHost (found in the Nancy.Hosting.Self package)
            using (var host = new NancyHost(hostConfigs, new Uri("http://localhost:1234")))
            {
                host.Start(); // start hosting

                Console.WriteLine("Running on http://localhost:1234");

                Console.ReadKey();
            }
        }
    }
}

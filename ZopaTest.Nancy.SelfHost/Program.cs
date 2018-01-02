using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nancy;
using Nancy.Hosting.Self;

using ZopaTest.Interop;
using static ZopaTest.Interop.InteropModule;
using static ZopaTest.Interop.ExtensionsModule;

using ZopaTest.Nancy.SelfHost.Models;

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
                    Quote.Quote quote = InteropModule.getQuote(market_file, parameters.loan_amount);

                    // options showed explicitely e.g. Some(38.90343873)
                    //return View["QuoteView", quote];

                    // £ shows as Â£
                    // return InteropModule.sprintfQuote(quote).Replace("\r\n", "<br/>");

                    // does NOT work, C# sees the original Quote type: Message = 'ZopaTest.Quote.Quote' does not contain a definition for 'PrintView'
                    //return View["QuotePrintView", quote];
                    //return quote.PrintView; 

                    // WORKS - string formatting on F# side + post processing to replace \r\n with <br/> in model
                    var quoteModel = new QuotePrintViewModel(quote.PrintView());
                    return View["QuotePrintView", quoteModel];

                    // WORKS - formatting on C# side
                    //var quoteModel = new QuoteModel(quote);
                    //return View["QuoteModelView", quoteModel];
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

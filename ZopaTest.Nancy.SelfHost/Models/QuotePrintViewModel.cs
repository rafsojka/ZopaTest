using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZopaTest.Nancy.SelfHost.Models
{
    public class QuotePrintViewModel
    {
        public string PrintView { get; set; }

        public QuotePrintViewModel(string printView)
        {
            this.PrintView = printView.Replace("\r\n", "<br/>");
        }
    }
}

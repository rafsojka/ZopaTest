using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZopaTest.Interop;
using static ZopaTest.Interop.ExtensionsModule;


namespace ZopaTest.Nancy.SelfHost.Models
{
    public class QuoteModel
    {
        private int requestedAmount;
        private decimal? rate;
        private decimal? monthlyRepayment;
        private decimal? totalRepayment;

        public string NoRateMessage = "It is not possible to provide a quote at this time.";

        public bool HasRate
        {
            get
            {
                return this.rate.HasValue;
            }
        }

        public string RequestedAmountFormatted
        {
            get
            {
                return String.Format("£{0}", this.requestedAmount);
            }
        }

        public string RateFormatted
        {
            get
            {
                return this.rate.HasValue ? String.Format("{0:0.0}%", this.rate.Value * 100.0M) : null;
            }
        }

        public string MonthlyRepaymentFormatted
        {
            get
            {
                return this.monthlyRepayment.HasValue ? String.Format("£{0:0.00}", this.monthlyRepayment) : null;
            }
        }

        public string TotalRepaymentFormatted
        {
            get
            {
                return this.totalRepayment.HasValue ? String.Format("£{0:0.00}", this.totalRepayment) : null;
            }
        }

        public QuoteModel(string marketFile, int loanAmount) : this(InteropModule.getQuote(marketFile, loanAmount))
        { }

        public QuoteModel(Quote.Quote quote) // using only "Quote" causes Error    CS0721	'Quote': static types cannot be used as parameters - compiler taking module for type??
        {
            this.requestedAmount = quote.RequestedAmount;
            // this.rate = quote.Rate.Exists() ? quote.Rate.Value : null; // Error CS0173 Type of conditional expression cannot be determined because there is no implicit conversion between 'decimal' and '<null>'
            if (quote.Rate.Exists())
            {
                this.rate = quote.Rate.Value;
            }
            else
            {
                this.rate = null;
            }

            if (quote.MonthlyRepayment.Exists())
            {
                this.monthlyRepayment = quote.MonthlyRepayment.Value;
            }
            else
            {
                this.monthlyRepayment = null;
            }

            if (quote.TotalRepayment.Exists())
            {
                this.totalRepayment = quote.TotalRepayment.Value;
            }
            else
            {
                this.totalRepayment = null;
            }
        }
    }
}

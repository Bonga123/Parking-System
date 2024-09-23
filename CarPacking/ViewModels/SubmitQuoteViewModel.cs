using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.ViewModels
{
    public class SubmitQuoteViewModel
    {
        public int QuoteRequestId { get; set; }
        public int SupplierId { get; set; }
        public List<QuoteItemViewModel> QuoteItems { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.ViewModels
{
    public class QuoteItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
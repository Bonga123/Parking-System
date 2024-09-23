using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int StockOrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }

        // Navigation properties
        public virtual StockOrder StockOrder { get; set; }
    }
}
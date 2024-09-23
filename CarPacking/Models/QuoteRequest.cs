// File: Models/QuoteRequest.cs
using System;
using System.Collections.Generic;

namespace CarPacking.Models
{
    public class QuoteRequest
    {
        public int QuoteRequestId { get; set; }
        public int StockOrderRequestId { get; set; }
        public int SupplierId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Pending, Submitted, etc.

        public virtual StockOrderRequest StockOrderRequest { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}

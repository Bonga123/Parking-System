// File: Models/Quote.cs
using System;
using System.Collections.Generic;

namespace CarPacking.Models
{
    public class Quote
    {
        public int QuoteId { get; set; }
       // public int SupplierId { get; set; }
        public int QuoteRequestId { get; set; }
        public DateTime QuoteDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsSelected { get; set; }

       // public virtual Supplier Supplier { get; set; }
        public virtual QuoteRequest QuoteRequest { get; set; }
        public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    }

    public class QuoteItem
    {
        public int QuoteItemId { get; set; }
        public int QuoteId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Quote Quote { get; set; }
        public virtual Product Product { get; set; }
    }
}

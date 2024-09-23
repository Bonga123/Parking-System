// File: Models/StockOrderRequest.cs
using System;
using System.Collections.Generic;

namespace CarPacking.Models
{
    public class StockOrderRequest
    {
        public int StockOrderRequestId { get; set; }
        public int PersonnelId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public bool IsProcessed { get; set; }

        public virtual Personnel Personnel { get; set; }
        public virtual ICollection<RequestedItem> RequestedItems { get; set; } = new List<RequestedItem>();
    }

    public class RequestedItem
    {
        public int RequestedItemId { get; set; }
        public int StockOrderRequestId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public virtual StockOrderRequest StockOrderRequest { get; set; }
        public virtual Product Product { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class OrderedProduct
    {
        public int OrderedProductId { get; set; }
        public int StockOrderId { get; set; } // Foreign key to StockOrder
        public int ProductId { get; set; }    // Foreign key to Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductDescription { get; set; }
        public virtual StockOrder StockOrder { get; set; }
        public virtual Product Product { get; set; }
    }
}
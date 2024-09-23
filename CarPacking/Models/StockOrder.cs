using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarPacking.Models
{
   
        public class StockOrder
        {
            public int StockOrderId { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime OrderDate { get; set; }

            public int SupplierId { get; set; }
            public string Status { get; set; }
          
            
            // Navigation property for one-to-one relationship with Delivery
            public virtual Delivery Delivery { get; set; }

            
            public virtual Supplier Supplier { get; set; }
            public virtual List<OrderedProduct> OrderedProducts { get; set; } = new List<OrderedProduct>();
        }
    


}

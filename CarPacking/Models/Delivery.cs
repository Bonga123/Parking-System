using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class Delivery
    {
        // StockOrderId is both the primary key and the foreign key
        [Key, ForeignKey("StockOrder")]
        public int StockOrderId { get; set; }

        // Navigation property to the principal StockOrder
        public virtual StockOrder StockOrder { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DeliveryDate { get; set; }

        public byte[] ReceiverSignature { get; set; }
        public byte[] DelivererSignature { get; set; }

        public virtual List<DeliveryItem> DeliveryItems { get; set; }
    }


    public class DeliveryItem
    {
        public int DeliveryItemId { get; set; }
        public int DeliveryId { get; set; }
        public virtual Delivery Delivery { get; set; }
        public int OrderedProductId { get; set; }
        public virtual OrderedProduct OrderedProduct { get; set; }
        public int QuantityDelivered { get; set; } // Must be <= OrderedProduct.Quantity
    }
}
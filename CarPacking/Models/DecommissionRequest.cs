using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class DecommissionRequest
    {
        public int DecommissionRequestId { get; set; }
        public int EquipmentId { get; set; }
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public string ApprovalStatus { get; set; } 
        public DateTime? ApprovalDate { get; set; }
        public virtual Equipment Equipment { get; set; }
    }
}
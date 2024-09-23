using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class MaintenanceUpdate
    {
        public int MaintenanceUpdateId { get; set; }
        public int MaintenanceTaskId { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateDescription { get; set; }

        public virtual MaintenanceTask MaintenanceTask { get; set; }
    }
}
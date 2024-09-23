using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime LastMaintenanceDate { get; set; }
        public bool IsDecommissioned { get; set; }

        public virtual List<MaintenanceTask> MaintenanceTasks { get; set; }
    }

}
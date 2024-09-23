using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class MaintenanceTask
    {
        public int MaintenanceTaskId { get; set; }
        public int EquipmentId { get; set; }
        public string TaskDescription { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsCompleted { get; set; }
        public decimal Cost { get; set; }
        public virtual Equipment Equipment { get; set; }
        public virtual List<AssignedPersonnel> AssignedPersonnel { get; set; }
        public virtual List<MaintenanceUpdate> MaintenanceUpdates { get; set; } 
    }
}
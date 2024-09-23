using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class AssignedPersonnel
    {
        public int AssignedPersonnelId { get; set; }
        public int MaintenanceTaskId { get; set; }
        public string PersonnelName { get; set; }
        public string SkillLevel { get; set; } // New property to reflect the personnel's skill level
        public bool IsAvailable { get; set; } // Availability status

        public virtual MaintenanceTask MaintenanceTask { get; set; }
    }
}
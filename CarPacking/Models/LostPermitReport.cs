using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class LostPermitReport
    {
        public int LostPermitReportId { get; set; }
        public int ParkingPermitId { get; set; }
        public string email { get; set; } // Assuming you have a Student model
        public DateTime ReportedDate { get; set; }
        public bool IsProcessed { get; set; } // To track if the admin has processed the report
        public virtual ParkingPermit ParkingPermit { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    // File: Models/ParkingSpot.cs
    public class ParkingSpot
    {
        public int ParkingSpotId { get; set; }
        public string Location { get; set; }
        public bool IsOccupied { get; set; }
        public int? ParkingPermitId { get; set; }
        public virtual ParkingPermit ParkingPermit { get; set; }
    }

}
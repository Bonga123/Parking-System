using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    // File: Models/Vehicle.cs
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string StudEmail { get; set; }
        public string LicensePlate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string PicturePath { get; set; }  // Path to the vehicle picture
    }


}
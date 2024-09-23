using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace CarPacking.Models
{
    public class ParkingPermit
    {
        public int ParkingPermitId { get; set; }
        public string PermitType { get; set; }
        public decimal Price { get; set; }
        public string QRCode { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime IssuedDate { get; set; }
        public int VehicleId { get; set; }
        public bool IsActive { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }
}
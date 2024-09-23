using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPacking.Models
{
    public class Personnel
    {
        public int PersonnelId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public virtual ICollection<MaintenanceTask> AssignedTasks { get; set; }
    }

}
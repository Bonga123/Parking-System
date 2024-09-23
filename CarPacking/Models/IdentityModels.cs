using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CarPacking.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<ParkingPermit> ParkingPermits { get; set; }
        public virtual DbSet<ParkingSpot> ParkingSpots { get; set; }
        public virtual DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
        public virtual DbSet<StockOrder> StockOrders { get; set; }
        public virtual DbSet<Personnel> Personnel { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<SuppliedProduct> SuppliedProducts { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<OrderedProduct> OrderedProducts { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<Equipment> Equipments { get; set; }
        public virtual DbSet<DecommissionRequest> DecommissionRequests { get; set; }
        public virtual DbSet<AssignedPersonnel> AssignedPersonnels { get; set; }
        public virtual DbSet<MaintenanceUpdate> MaintenanceUpdates { get; set; }
        public virtual DbSet<LostPermitReport> LostPermitReports { get; set; }
        public virtual DbSet<StockOrderRequest> StockOrderRequests { get; set; }
        public virtual DbSet<RequestedItem> RequestedItems { get; set; }
        public virtual DbSet<Quote> Quotes { get; set; }
        public virtual DbSet<QuoteItem> QuoteItems { get; set; }
        public virtual DbSet<QuoteRequest> QuoteRequests { get; set; }
        

        // Additional DbSets for other entities...
    }
}
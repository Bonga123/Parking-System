using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPacking.Controllers
{
    // File: Controllers/ParkingSpotController.cs
    using CarPacking.Models;

    // File: Controllers/ParkingSpotController.cs
   

    public class ParkingSpotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingSpotController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult AllocateSpot(int permitId)
        {
            var permit = _context.ParkingPermits.Find(permitId);
            if (permit == null) return HttpNotFound();

            var availableSpots = _context.ParkingSpots.Where(s => !s.IsOccupied).ToList();

            ViewBag.Permit = permit;
            return View(availableSpots);
        }

        [HttpPost]
        public ActionResult AllocateSpot(int spotId, int permitId)
        {
            var spot = _context.ParkingSpots.Find(spotId);
            if (spot == null || spot.IsOccupied) return HttpNotFound();

            spot.ParkingPermitId = permitId;
            spot.IsOccupied = true;

            _context.SaveChanges();

            // Redirect to SpotAllocated view after successfully allocating the spot
            return RedirectToAction("SpotAllocated", new { id = spot.ParkingSpotId });
        }

        public ActionResult SpotAllocated(int id)
        {
            var spot = _context.ParkingSpots.Find(id);
            if (spot == null) return HttpNotFound();

            return View(spot);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }


}
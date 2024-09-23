// File: Controllers/VehicleController.cs
using CarPacking.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class VehicleController : Controller
{
    private readonly ApplicationDbContext _context;

    public VehicleController()
    {
        _context = ApplicationDbContext.Create();
    }

    [HttpGet]
    public ActionResult ListVehicles()
    {
        var vehicles = _context.Vehicles.ToList();
        return View(vehicles);
    }

    [HttpGet]
    public ActionResult MyVehicle()
    {
        var vehicle = _context.Vehicles.Where(x=>x.StudEmail == User.Identity.Name).FirstOrDefault();
        if (vehicle == null) return HttpNotFound();

        return View(vehicle);
    }

    [HttpGet]
    public ActionResult CreateVehicle()
    {
        return View();
    }

    [HttpPost]
    public ActionResult CreateVehicle(Vehicle vehicle, HttpPostedFileBase vehicleImage)
    {
        if (ModelState.IsValid)
        {
            // Handle the file upload
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var fileName = Path.GetFileName(vehicleImage.FileName);
                var path = Path.Combine(Server.MapPath("~/assets/images"), fileName);
                vehicleImage.SaveAs(path);

                // Save the relative path to the PicturePath property
                vehicle.PicturePath = "/assets/images/" + fileName;
            }
            vehicle.StudEmail = User.Identity.Name;
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();

            // Redirect to the VehicleCreated view
            return RedirectToAction("VehicleCreated", new { id = vehicle.VehicleId });
        }

        return View(vehicle);
    }

    public ActionResult VehicleCreated(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null) return HttpNotFound();

        return View(vehicle);
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

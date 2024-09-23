// File: Controllers/LostPermitController.cs
using CarPacking.Models;
using System;
using System.Web.Mvc;

public class LostPermitController : Controller
{
    private readonly ApplicationDbContext _context;

    public LostPermitController()
    {
        _context = ApplicationDbContext.Create();
    }

    [HttpPost]
    public ActionResult ReportLost(int permitId)
    {
        var permit = _context.ParkingPermits.Find(permitId);
        if (permit == null) return HttpNotFound();

        // Create a new lost permit report
        var lostPermitReport = new LostPermitReport
        {
            ParkingPermitId = permit.ParkingPermitId,
            email = permit.Vehicle.StudEmail, // Assume this is part of the permit or session
            ReportedDate = DateTime.Now,
            IsProcessed = false
        };

        _context.LostPermitReports.Add(lostPermitReport);
        _context.SaveChanges();

        return RedirectToAction("ReportSubmitted");
    }

    public ActionResult ReportSubmitted()
    {
        return View(); // Confirm that the report has been submitted
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

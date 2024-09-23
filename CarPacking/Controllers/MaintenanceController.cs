using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPacking.Controllers
{
    // File: Controllers/MaintenanceController.cs
    using CarPacking.Models;

    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult Schedule()
        {
            return View(new MaintenanceTask());
        }

        [HttpPost]
        public ActionResult Schedule(MaintenanceTask task)
        {
            if (ModelState.IsValid)
            {
                _context.MaintenanceTasks.Add(task);
                _context.SaveChanges();

                return RedirectToAction("ScheduledTasks");
            }

            return View(task);
        }

        [HttpGet]
        public ActionResult ScheduledTasks()
        {
            var tasks = _context.MaintenanceTasks.Where(t => !t.IsCompleted).ToList();
            return View(tasks);
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
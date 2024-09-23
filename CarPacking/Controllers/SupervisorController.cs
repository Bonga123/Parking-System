using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPacking.Controllers
{
    // File: Controllers/SupervisorController.cs
    using CarPacking.Models;

    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult ConfirmCompletion(int taskId)
        {
            var task = _context.MaintenanceTasks.Find(taskId);
            if (task == null) return HttpNotFound();

            return View(task);
        }

        [HttpPost]
        public ActionResult ConfirmCompletion(int taskId, bool isConfirmed)
        {
            var task = _context.MaintenanceTasks.Find(taskId);
            if (task == null) return HttpNotFound();

            if (isConfirmed)
            {
                task.IsCompleted = true;
                _context.SaveChanges();
            }

            return RedirectToAction("ScheduledTasks", "Maintenance");
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
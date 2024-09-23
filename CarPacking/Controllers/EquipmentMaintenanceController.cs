using CarPacking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CarPacking.Controllers
{
    public class EquipmentMaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipmentMaintenanceController()
        {
            _context = ApplicationDbContext.Create();
        }

        // 1. Schedule Equipment Maintenance with Optimization
        // GET: Schedule Equipment Maintenance
        [HttpGet]
        public ActionResult ScheduleMaintenance()
        {
            var equipmentList = _context.Equipments.Where(e => !e.IsDecommissioned).ToList();
            ViewBag.OptimalDates = GetOptimalMaintenanceDates(equipmentList); // Suggest optimal dates
            return View(equipmentList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScheduleMaintenance(string taskDescription, DateTime scheduledDate, int[] SelectedEquipment, Dictionary<string, string> Quantities)
        {
            if (SelectedEquipment == null || !SelectedEquipment.Any())
            {
                TempData["Error"] = "No equipment selected.";
                return RedirectToAction("ScheduleMaintenance");
            }

            foreach (var equipmentId in SelectedEquipment)
            {
                var equipment = _context.Equipments.Find(equipmentId);
                if (equipment == null)
                {
                    TempData["Error"] = "Equipment not found.";
                    return RedirectToAction("ScheduleMaintenance");
                }

                // Convert the equipmentId to string to match the dictionary keys
                string equipmentIdStr = equipmentId.ToString();

                // Ensure a quantity was provided
                if (!Quantities.ContainsKey(equipmentIdStr) || string.IsNullOrEmpty(Quantities[equipmentIdStr]))
                {
                    TempData["Error"] = $"Please specify a quantity for {equipment.Name}.";
                    return RedirectToAction("ScheduleMaintenance");
                }

                // Parse the quantity and validate
                if (!int.TryParse(Quantities[equipmentIdStr], out int quantityNeeded) || quantityNeeded <= 0)
                {
                    TempData["Error"] = $"Invalid quantity specified for {equipment.Name}.";
                    return RedirectToAction("ScheduleMaintenance");
                }

                if (quantityNeeded > equipment.Quantity)
                {
                    TempData["Error"] = $"Not enough quantity available for {equipment.Name}. Only {equipment.Quantity} available.";
                    return RedirectToAction("ScheduleMaintenance");
                }

                equipment.Quantity -= quantityNeeded;
                _context.Entry(equipment).State = EntityState.Modified;

                var maintenanceTask = new MaintenanceTask
                {
                    EquipmentId = equipmentId,
                    TaskDescription = taskDescription,
                    ScheduledDate = scheduledDate,
                    IsCompleted = false
                };

                _context.MaintenanceTasks.Add(maintenanceTask);
                _context.SaveChanges();
                Session["TaskID"] = maintenanceTask.MaintenanceTaskId.ToString();
            }

           
            string taskID = Session["TaskID"] as string;
            if (taskID != null)
            {
                int taskId = int.Parse(taskID);
                TempData["Success"] = "Maintenance tasks scheduled successfully.";
                return RedirectToAction("AssignTask", "Personnel", new { taskId = taskId });
            }
            TempData["Error"] = "Something Went wrong!!";
            return RedirectToAction("ScheduleMaintenance");

        }



        private Dictionary<int, DateTime> GetOptimalMaintenanceDates(List<Equipment> equipmentList)
        {
            // Example: Suggest dates based on last maintenance + average interval
            return equipmentList.ToDictionary(e => e.EquipmentId, e => e.LastMaintenanceDate.AddMonths(6)); // Assuming 6-month intervals
        }

        // 2. Assign Personnel with Skill Matching
        [HttpGet]
        public ActionResult AssignPersonnel(int maintenanceTaskId)
        {
            var task = _context.MaintenanceTasks.Include(t => t.Equipment).FirstOrDefault(t => t.MaintenanceTaskId == maintenanceTaskId);
            if (task == null)
            {
                return HttpNotFound();
            }

            ViewBag.PersonnelList = GetAvailablePersonnel(task); // Get available personnel based on skills
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignPersonnel(int maintenanceTaskId, string personnelName)
        {
            var personnel = _context.AssignedPersonnels.FirstOrDefault(p => p.PersonnelName == personnelName && p.IsAvailable);
            if (personnel == null)
            {
                TempData["Error"] = "Personnel not available or does not exist.";
                return RedirectToAction("AssignPersonnel", new { maintenanceTaskId });
            }

            personnel.MaintenanceTaskId = maintenanceTaskId;
            personnel.IsAvailable = false; // Mark as unavailable

            _context.Entry(personnel).State = EntityState.Modified;
            _context.SaveChanges();

            TempData["Message"] = "Personnel assigned successfully.";
            return RedirectToAction("AssignPersonnel", new { maintenanceTaskId });
        }

        // 3. Track Equipment Maintenance Progress with Real-Time Updates
        [HttpGet]
        public ActionResult TrackMaintenance()
        {
            var tasks = _context.MaintenanceTasks.Include(t => t.Equipment).Include(t => t.AssignedPersonnel).Where(t => !t.IsCompleted).ToList();
            return View(tasks);
        }

        [HttpPost]
        public ActionResult AddMaintenanceUpdate(int maintenanceTaskId, string updateDescription)
        {
            var maintenanceUpdate = new MaintenanceUpdate
            {
                MaintenanceTaskId = maintenanceTaskId,
                UpdateTime = DateTime.Now,
                UpdateDescription = updateDescription
            };

            _context.MaintenanceUpdates.Add(maintenanceUpdate);
            _context.SaveChanges();

            TempData["Message"] = "Maintenance update added successfully.";
            return RedirectToAction("TrackMaintenance");
        }

        // 4. Confirm Equipment Maintenance Completion with Cost Analysis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCompletion(int maintenanceTaskId)
        {
            var task = _context.MaintenanceTasks.Find(maintenanceTaskId);
            if (task == null)
            {
                return HttpNotFound();
            }

            task.IsCompleted = true;
            task.CompletionDate = DateTime.Now;

            // Perform cost analysis (e.g., compare with budget)
            PerformCostAnalysis(task);

            _context.Entry(task).State = EntityState.Modified;
            _context.SaveChanges();

            TempData["Message"] = "Maintenance task marked as completed.";
            return RedirectToAction("TrackMaintenance");
        }

        // 5. Decommission Equipment with Approval Workflow
        [HttpGet]
        public ActionResult DecommissionRequest(int equipmentId)
        {
            var equipment = _context.Equipments.Find(equipmentId);
            if (equipment == null)
            {
                return HttpNotFound();
            }

            return View(new DecommissionRequest { EquipmentId = equipmentId, RequestDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitDecommissionRequest(DecommissionRequest request)
        {
            request.ApprovalStatus = "Pending";
            _context.DecommissionRequests.Add(request);
            _context.SaveChanges();

            NotifyAdminForApproval(request); // Notify admin for approval

            TempData["Message"] = "Decommission request submitted successfully.";
            return RedirectToAction("ScheduleMaintenance");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveDecommission(int requestId)
        {
            var request = _context.DecommissionRequests.Include(r => r.Equipment).FirstOrDefault(r => r.DecommissionRequestId == requestId);
            if (request == null)
            {
                return HttpNotFound();
            }

            request.ApprovalStatus = "Approved";
            request.ApprovalDate = DateTime.Now;
            request.Equipment.IsDecommissioned = true;

            _context.Entry(request).State = EntityState.Modified;
            _context.SaveChanges();

            TempData["Message"] = "Decommission request approved and equipment decommissioned.";
            return RedirectToAction("ScheduleMaintenance");
        }

        

        private List<AssignedPersonnel> GetAvailablePersonnel(MaintenanceTask task)
        {
            // Example: Fetch personnel with relevant skills who are available
            return _context.AssignedPersonnels.Where(p => p.IsAvailable && p.SkillLevel == "Expert").ToList();
        }

        private void NotifyPersonnelAboutTask(MaintenanceTask task)
        {
            // Example: Notify assigned personnel via email or internal notification
            foreach (var personnel in task.AssignedPersonnel)
            {
                // Send notification (implementation not shown)
            }
        }

        private void PerformCostAnalysis(MaintenanceTask task)
        {
            // Example: Analyze maintenance costs
            var budget = 1000m; // Example budget
            if (task.Cost > budget)
            {
                // Log a warning or take action (implementation not shown)
            }
        }

        private void NotifyAdminForApproval(DecommissionRequest request)
        {
            // Example: Notify admin for decommission approval
            // Send notification (implementation not shown)
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

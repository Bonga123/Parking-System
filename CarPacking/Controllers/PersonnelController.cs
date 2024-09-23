using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CarPacking.Controllers
{
    // File: Controllers/PersonnelController.cs
    using CarPacking.Models;
    using Org.BouncyCastle.Asn1.Ocsp;
    using System.IO;

    public class PersonnelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonnelController()
        {
            _context = ApplicationDbContext.Create();
        }
        [HttpGet]
        public ActionResult RequestStockOrder()
        {
            var products = _context.Products.ToList();
            ViewBag.Products = new SelectList(products, "ProductId", "Name");
            return View(new StockOrderRequest { RequestDate = DateTime.Now });
        }



        [HttpPost]
        public ActionResult RequestStockOrder(StockOrderRequest orderRequest, int[] productIds, int[] quantities)
        {
            if (ModelState.IsValid && productIds.Length == quantities.Length)
            {
                var personell = _context.Personnel.Where(x=>x.Email == User.Identity.Name).FirstOrDefault();
                orderRequest.IsProcessed = false;
                orderRequest.RequestDate = DateTime.Now;
                orderRequest.Personnel = personell;
                _context.StockOrderRequests.Add(orderRequest);
                _context.SaveChanges();

                for (int i = 0; i < productIds.Length; i++)
                {
                    var requestedItem = new RequestedItem
                    {
                        StockOrderRequestId = orderRequest.StockOrderRequestId,
                        ProductId = productIds[i],
                        Quantity = quantities[i]
                    };
                    _context.RequestedItems.Add(requestedItem);
                }
                _context.SaveChanges();

                return RedirectToAction("RequestSubmitted", new { requestId = orderRequest.StockOrderRequestId });
            }
            ViewBag.Products = new SelectList(_context.Products.ToList(), "ProductId", "Name");
            return View(orderRequest);
        }

        public ActionResult RequestSubmitted(int requestId)
        {
            var request = _context.StockOrderRequests
                                  .Include(r => r.RequestedItems.Select(ri => ri.Product))
                                  .Include(r => r.Personnel)
                                  .FirstOrDefault(r => r.StockOrderRequestId == requestId);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // GET: Drivers
        public ActionResult Index()
        {
           

            return View(_context.Personnel.ToList());
        }
        public ActionResult MyProfile(string email = "Email")
        {
            if (email == "Email")
            {
                email = User.Identity.Name;
                ViewBag.Title = "MyProfile";
            }
            else
            {
                ViewBag.Title = "Single";
            }

            return View(_context.Personnel.Where(x => x.Email == email).ToList());
        }

        // GET: Drivers/Create
        public ActionResult Create()
        {
            Personnel b = new Personnel()
            {
                Email = User.Identity.Name
            };
            return View(b);
        }

        // POST: Drivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PersonnelId,Name,Surname,PhoneNumber,Address,Email,Picture")] Personnel personnel, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                


                // Save the picture file on the server
                string pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string picturePath = Path.Combine(Server.MapPath("~/assets/images/"), pictureFileName);
                file.SaveAs(picturePath);

                // Set the picture path in the record
                personnel.Picture = pictureFileName;
                _context.Personnel.Add(personnel);
                _context.SaveChanges();
                return RedirectToAction("MyProfile");
            }

            return View(personnel);
        }



        [HttpGet]
        public ActionResult AssignTask(int taskId)
        {
            var task = _context.MaintenanceTasks.Find(taskId);
            if (task == null) return HttpNotFound();

            ViewBag.Personnel = new SelectList(_context.Personnel.ToList(), "PersonnelId", "Name");
            return View(task);
        }

        [HttpPost]
        public ActionResult AssignTask(int taskId, int personnelId)
        {
            var task = _context.MaintenanceTasks.Find(taskId);
            if (task == null) return HttpNotFound();

            var personnel = _context.Personnel.Find(personnelId);
            if (personnel == null) return HttpNotFound();

            personnel.AssignedTasks.Add(task);
            _context.SaveChanges();

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
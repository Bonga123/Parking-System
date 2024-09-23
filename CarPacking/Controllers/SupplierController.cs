using System.Data.Entity; // Make sure to include this namespace for the Include method
using System.Linq;
using System.Web.Mvc;
using CarPacking.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using CarPacking.ViewModels;
using System.Collections.Generic;

namespace CarPacking.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController()
        {
            _context = ApplicationDbContext.Create();
        }
        // Supplier views the pending quote requests
        public ActionResult ViewQuoteRequests(int supplierId)
        {
            var quoteRequests = _context.QuoteRequests
                                        .Where(qr => qr.SupplierId == supplierId && qr.Status == "Pending")
                                        .Include(qr => qr.StockOrderRequest.RequestedItems.Select(ri => ri.Product))
                                        .ToList();

            return View(quoteRequests);
        }
        public ActionResult GetQuoteRequestDetails(int id)
        {
            var quoteRequestDetails = _context.QuoteRequests
                                              .Where(qr => qr.QuoteRequestId == id)
                                              .Select(qr => qr.StockOrderRequest.RequestedItems.Select(ri => new
                                              {
                                                  ProductName = ri.Product.Name,
                                                  Quantity = ri.Quantity
                                              }))
                                              .FirstOrDefault();

            if (quoteRequestDetails == null)
            {
                return HttpNotFound();
            }

            return Json(quoteRequestDetails, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SubmitQuote(int quoteRequestId)
        {
            var quoteRequest = _context.QuoteRequests
                                       .Include(qr => qr.StockOrderRequest.RequestedItems.Select(ri => ri.Product))
                                       .FirstOrDefault(qr => qr.QuoteRequestId == quoteRequestId);

            if (quoteRequest == null || quoteRequest.Status != "Pending")
            {
                return HttpNotFound();
            }

            var viewModel = new SubmitQuoteViewModel
            {
                QuoteRequestId = quoteRequestId,
                SupplierId = quoteRequest.SupplierId,
                QuoteItems = quoteRequest.StockOrderRequest.RequestedItems.Select(ri => new QuoteItemViewModel
                {
                    ProductId = ri.ProductId,
                    ProductName = ri.Product.Name,
                    Quantity = ri.Quantity,
                    UnitPrice = 0 // Default unit price, to be entered by supplier
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SubmitQuote(SubmitQuoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var quote = new Quote
                {
                    QuoteRequestId = model.QuoteRequestId,
                    QuoteDate = DateTime.Now,
                    TotalPrice = model.QuoteItems.Sum(qi => qi.Quantity * qi.UnitPrice),
                    QuoteItems = model.QuoteItems.Select(qi => new QuoteItem
                    {
                        ProductId = qi.ProductId,
                        Quantity = qi.Quantity,
                        UnitPrice = qi.UnitPrice
                    }).ToList()
                };
                var quotereq = _context.QuoteRequests.Find(model.QuoteRequestId);
                quotereq.Status = "Approved";
                _context.Entry(quotereq).State = EntityState.Modified;
                _context.Quotes.Add(quote);
                _context.SaveChanges();

                return RedirectToAction("ViewQuoteRequests", new { supplierId = model.SupplierId });
            }

            return View(model);
        }








        [HttpGet]
        public ActionResult AddSupplier()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddSupplier(Supplier supplier)
        {
            if (ModelState.IsValid)
            {

                string role = "Supplier";
                string email = supplier.Email;
                string name = supplier.Name;
                string password = supplier.Password;

                ApplicationDbContext db = new ApplicationDbContext();

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                try
                {

                    if (!roleManager.RoleExists(role))
                    {
                        roleManager.Create(new IdentityRole(role));
                    }
                    var user = new ApplicationUser();

                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;
                    string pwd = password;

                    var newuser = userManager.Create(user, pwd);
                    if (newuser.Succeeded)
                    {
                        userManager.AddToRole(user.Id, role);
                    }
                    TempData["Success"] = "User created successfully!!!";
                }
                catch
                {
                    TempData["Error"] = "Something went wrong, Please try again later.";
                }




                _context.Suppliers.Add(supplier);
                _context.SaveChanges();

                return RedirectToAction("AddProduct", new { supplierId = supplier.SupplierId });
            }

            return View(supplier);
        }

        [HttpGet]
        public ActionResult AddProduct(int supplierId)
        {
            var supplier = _context.Suppliers.Find(supplierId);
            if (supplier == null) return HttpNotFound();

            ViewBag.SupplierId = supplierId;
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(SuppliedProduct suppliedProduct)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = Request.Form["Product.Name"],
                    Description = Request.Form["Product.Description"],
                    Price = decimal.Parse(Request.Form["Product.Price"])
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                suppliedProduct.ProductId = product.ProductId;
                _context.SuppliedProducts.Add(suppliedProduct);
                _context.SaveChanges();

                return RedirectToAction("AddProduct", new { supplierId = suppliedProduct.SupplierId });
            }

            var supplier = _context.Suppliers.Find(suppliedProduct.SupplierId);
            if (supplier == null) return HttpNotFound();

            ViewBag.SupplierId = suppliedProduct.SupplierId;
            return View(suppliedProduct);
        }

        // New Action Method to List Suppliers and Their Products
        public ActionResult ListSuppliers()
        {
            var suppliers = _context.Suppliers
                                     .Include(s => s.SuppliedProducts.Select(sp => sp.Product))
                                     .ToList();

            return View(suppliers);
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

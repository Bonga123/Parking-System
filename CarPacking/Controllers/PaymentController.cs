using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPacking.Controllers
{
    // File: Controllers/PaymentController.cs
    using CarPacking.Models;

    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult IssuePayment(int orderId)
        {
            var order = _context.StockOrders.Find(orderId);
            if (order == null) return HttpNotFound();

            return View(order);
        }

        //[HttpPost]
        //public ActionResult IssuePayment(int orderId, decimal amount)
        //{
        //    var order = _context.StockOrders.Find(orderId);
        //    if (order == null) return HttpNotFound();

        //    // Implement payment logic with PayPal API
        //    var paymentSuccess = _context.ParkingSpots.Find(1);

        //    if (paymentSuccess)
        //    {
        //        return RedirectToAction("PaymentSuccess");
        //    }

        //    ModelState.AddModelError("", "Payment failed. Please try again.");
        //    return View(order);
        //}

        private bool ProcessPayment(string email, decimal amount)
        {
            // Placeholder: Implement PayPal payment processing logic
            return true;
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
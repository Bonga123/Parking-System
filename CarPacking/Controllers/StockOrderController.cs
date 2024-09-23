using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using CarPacking.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CarPacking.Controllers
{
    public class StockOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockOrderController()
        {
            _context = ApplicationDbContext.Create();
        }

        public ActionResult Index()
        {
            var orders = _context.StockOrders
                                 .Include(o => o.OrderedProducts.Select(op => op.Product))
                                 .Include(o => o.Supplier)
                                 .ToList();

            return View(orders);
        }

        [HttpGet]
        public ActionResult GetQuoteDetails(int id)
        {
            var quote = _context.Quotes
                .Include("QuoteItems.Product")
                .FirstOrDefault(q => q.QuoteId == id);

            if (quote == null)
            {
                return HttpNotFound();
            }

            return PartialView("_QuoteDetailsPartial", quote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveQuote(int quoteId)
        {
            try
            {
                // Retrieve the quote and associated items
                var quote = _context.Quotes
                    .Include("QuoteItems")
                    .FirstOrDefault(q => q.QuoteId == quoteId);

                // Check if the quote exists
                if (quote == null)
                {
                    return Json(new { success = false, message = "Quote not found." });
                }

                // Create a new stock order
                var order = new StockOrder
                {
                    SupplierId = quote.QuoteRequest.SupplierId,
                    OrderDate = DateTime.Now,
                    Status = "Requested",
                    OrderedProducts = quote.QuoteItems.Select(qi => new OrderedProduct
                    {
                        ProductId = qi.ProductId,
                        Quantity = qi.Quantity,
                        UnitPrice = qi.UnitPrice
                    }).ToList()
                };

                // Add the new order to the database
                _context.StockOrders.Add(order);
                _context.SaveChanges();

                // Generate PDF and send email (optional)
                var pdfFilePath = GenerateOrderPdf(order);
                var pdfDocument = System.IO.File.ReadAllBytes(pdfFilePath);
                SendOrderEmailToSupplier(order, pdfDocument);

                return RedirectToAction("OrderRequested", new {id= order.StockOrderId });
                
            }
            catch (Exception ex)
            {
                // Log the error for debugging purposes (replace with your logging mechanism)
                Console.WriteLine("Error in ApproveQuote: " + ex.ToString());

                // Return a JSON response with the error message
                return Json(new { success = false, message = "An error occurred while approving the quote: " + ex.Message });
            }
        }





        public ActionResult OrderRequested(int id)
        {
            var order = _context.StockOrders
                                .Include(o => o.OrderedProducts.Select(op => op.Product))
                                .Include(o => o.Supplier)
                                .SingleOrDefault(x => x.StockOrderId == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }

        public ActionResult ListOrders()
        {
            var orders = _context.StockOrders
                                 .Include(o => o.OrderedProducts.Select(op => op.Product))
                                 .Include(o => o.Supplier)
                                 .ToList();

            return View(orders);
        }

        private string GenerateOrderPdf(StockOrder order)
        {
            string assetsPath = Server.MapPath("~/assets");
            string fileName = $"Order_{order.StockOrderId}.pdf";
            string filePath = Path.Combine(assetsPath, fileName);

            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph("Stock Order", titleFont));
                document.Add(new Paragraph($"Order ID: {order.StockOrderId}", bodyFont));
                document.Add(new Paragraph($"Order Date: {order.OrderDate:dd-MM-yyyy}", bodyFont));

                foreach (var orderedProduct in order.OrderedProducts)
                {
                    var product = _context.Products.Find(orderedProduct.ProductId);
                    document.Add(new Paragraph($"Product: {product.Name}", bodyFont));
                    var supplier = _context.Suppliers.Find(order.SupplierId);
                    document.Add(new Paragraph($"Supplier: {supplier.Name}", bodyFont));
                    document.Add(new Paragraph($"Supplier Email: {supplier.Email}", bodyFont));
                    document.Add(new Paragraph($"Quantity: {orderedProduct.Quantity}", bodyFont));
                    document.Add(new Paragraph("------------------------------------------------------", bodyFont));
                }

                document.Close();
            }

            return filePath;
        }

        public ActionResult DownloadPdf(int orderId)
        {
            string assetsPath = Server.MapPath("~/assets");
            string fileName = $"Order_{orderId}.pdf";
            string filePath = Path.Combine(assetsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("PDF file not found");
            }

            return File(filePath, "application/pdf", fileName);
        }

        private void SendOrderEmailToSupplier(StockOrder order, byte[] pdfDocument)
        {
            var fromAddress = new MailAddress("AviNashKhem2004@outlook.com", "Car Parking");
            var toAddress = new MailAddress(order.Supplier.Email, order.Supplier.Name);
            const string subject = "New Stock Order";
            string body = $"Dear {order.Supplier.Name},\n\nPlease find attached the stock order.\n\nThank you.";

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                message.Attachments.Add(new Attachment(new MemoryStream(pdfDocument), $"Order_{order.StockOrderId}.pdf"));

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("AviNashKhem2004@outlook.com", "avinash1")
                })
                {
                    smtp.Send(message);
                }
            }
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

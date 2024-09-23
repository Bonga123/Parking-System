using CarPacking.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Data.Entity.Infrastructure;

namespace CarPacking.Controllers
{
    public class ConfirmDeliveryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfirmDeliveryController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult ConfirmDelivery(int orderId)
        {
            var order = _context.StockOrders
                .Include(o => o.OrderedProducts.Select(op => op.Product))
                .SingleOrDefault(o => o.StockOrderId == orderId);

            if (order == null) return HttpNotFound();

            var delivery = new Delivery
            {
                StockOrderId = orderId,
                StockOrder = order,
                DeliveryDate = DateTime.Now,
                DeliveryItems = order.OrderedProducts.Select(op => new DeliveryItem
                {
                    OrderedProductId = op.OrderedProductId,
                    OrderedProduct = op,
                    QuantityDelivered = 0 // Initialize with zero
                }).ToList()
            };

            return View(delivery);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelivery(Delivery delivery, string DelivererSignature, string ReceiverSignature)
        {
            if (delivery == null || delivery.DeliveryItems == null || !delivery.DeliveryItems.Any())
            {
                TempData["Error"] = "Delivery information is missing or incomplete.";
                return RedirectToAction("ConfirmDelivery", new { orderId = delivery?.StockOrderId });
            }

            // Ensure the StockOrderId exists in the StockOrders table
            var order = _context.StockOrders
                .Include(o => o.OrderedProducts)
                .SingleOrDefault(o => o.StockOrderId == delivery.StockOrderId);

            if (order == null)
            {
                TempData["Error"] = "Invalid StockOrderId.";
                return RedirectToAction("ConfirmDelivery", new { orderId = delivery.StockOrderId });
            }

            if (ModelState.IsValid)
            {
                // Ensure that each delivery item references a valid OrderedProduct in the context of the current StockOrder
                foreach (var deliveryItem in delivery.DeliveryItems)
                {
                    var orderedProduct = order.OrderedProducts
                        .SingleOrDefault(op => op.OrderedProductId == deliveryItem.OrderedProductId);

                    if (orderedProduct == null)
                    {
                        TempData["Error"] = $"The product with ID {deliveryItem.OrderedProductId} is not associated with this order.";
                        return RedirectToAction("ConfirmDelivery", new { orderId = delivery.StockOrderId });
                    }

                    // Validate delivered quantity
                    if (deliveryItem.QuantityDelivered > orderedProduct.Quantity)
                    {
                        TempData["Error"] = "Delivered quantity cannot exceed ordered quantity.";
                        return RedirectToAction("ConfirmDelivery", new { orderId = delivery.StockOrderId });
                    }

                    // Associate the DeliveryItem with the existing OrderedProduct
                    deliveryItem.OrderedProduct = orderedProduct;
                }

                // Proceed with saving the delivery and related entities
                delivery.StockOrder = order;

                // Convert signatures from Base64 to byte arrays, assuming they are provided in Base64 format
                delivery.DelivererSignature = !string.IsNullOrEmpty(DelivererSignature) ? ConvertBase64ToBytes(DelivererSignature) : null;
                delivery.ReceiverSignature = !string.IsNullOrEmpty(ReceiverSignature) ? ConvertBase64ToBytes(ReceiverSignature) : null;

                _context.Deliveries.Add(delivery);

                // Update StockOrder status
                order.Status = "Received";
                foreach(var product in order.OrderedProducts)
                {
                    var Equip = _context.Equipments.Where(x => x.Name == product.Product.Name).FirstOrDefault();
                    
                    if(Equip!= null)
                    {
                        Equip.Quantity += product.Quantity;
                        _context.Entry(Equip).State = EntityState.Modified;
                    }
                    else
                    {
                        Equipment equipment = new Equipment()
                        {
                            Name = product.Product.Name,
                            Description = product.Product.Description,
                            Quantity = product.Quantity
                        };
                        _context.Equipments.Add(equipment);
                    }
                    
                }
                

                try
                {
                    _context.SaveChanges();

                    // Generate Invoice after successful delivery save
                    var invoice = GenerateInvoice(order);

                    // Generate and save PDF invoice
                    var pdfPath = SaveInvoicePdf(invoice);

                    TempData["Success"] = "Delivery confirmed and invoice generated successfully.";
                    TempData["PdfPath"] = pdfPath;

                    return RedirectToAction("ViewInvoice", new { id = invoice.InvoiceId });
                }
                catch (DbUpdateException ex)
                {
                    // Log exception (not shown here) and provide feedback to the user
                    TempData["Error"] = "An error occurred while saving the delivery: " + ex.InnerException?.Message ?? ex.Message;
                    return RedirectToAction("ConfirmDelivery", new { orderId = delivery.StockOrderId });
                }
            }

            TempData["Error"] = "Something went wrong.";
            return RedirectToAction("ConfirmDelivery", new { orderId = delivery.StockOrderId });
        }

        private Invoice GenerateInvoice(StockOrder order)
        {
            // Calculate the total amount based on delivered items and their prices
            var totalAmount = order.Delivery.DeliveryItems
                .Sum(di => di.QuantityDelivered * di.OrderedProduct.Product.Price);

            // Create a new invoice
            var invoice = new Invoice
            {
                StockOrderId = order.StockOrderId,
                StockOrder = order,
                InvoiceDate = DateTime.Now,
                TotalAmount = totalAmount,
                IsPaid = false
            };

            // Save the invoice to the database
            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return invoice;
        }

        private byte[] GenerateInvoicePdf(Invoice invoice)
        {
            using (var ms = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add the invoice content
                document.Add(new Paragraph($"Invoice ID: {invoice.InvoiceId}"));
                document.Add(new Paragraph($"Date: {invoice.InvoiceDate.ToShortDateString()}"));
                document.Add(new Paragraph($"Total Amount: {invoice.TotalAmount:C}"));
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("Delivered Products:"));

                foreach (var item in invoice.StockOrder.Delivery.DeliveryItems)
                {
                    document.Add(new Paragraph($"{item.OrderedProduct.Product.Name}: {item.QuantityDelivered} @ {item.OrderedProduct.Product.Price:C}")
                    {

                    });
                }

                document.Close();

                return ms.ToArray();
            }
        }

        private string SaveInvoicePdf(Invoice invoice)
        {
            // Generate PDF
            var pdfBytes = GenerateInvoicePdf(invoice);

            // Ensure the ~/assets directory exists
            var assetsDir = Server.MapPath("~/assets");
            if (!Directory.Exists(assetsDir))
            {
                Directory.CreateDirectory(assetsDir);
            }

            // Generate a unique file name
            var pdfFileName = $"Invoice_{invoice.InvoiceId}.pdf";
            var pdfPath = Path.Combine(assetsDir, pdfFileName);

            // Save the PDF to the ~/assets directory
            System.IO.File.WriteAllBytes(pdfPath, pdfBytes);

            return pdfPath;
        }

        public ActionResult ViewInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.StockOrder.Delivery.DeliveryItems.Select(di => di.OrderedProduct.Product))
                .Include(i => i.StockOrder.Supplier)
                .SingleOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return HttpNotFound();

            return View(invoice);
        }

        public ActionResult DownloadInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.StockOrder.Delivery.DeliveryItems.Select(di => di.OrderedProduct.Product))
                .SingleOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return HttpNotFound();

            var pdfDocument = GenerateInvoicePdf(invoice);
            return File(pdfDocument, "application/pdf", $"Invoice_{invoice.InvoiceId}.pdf");
        }

        public ActionResult PayInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.StockOrder.Delivery.DeliveryItems.Select(di => di.OrderedProduct.Product))
                .SingleOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return HttpNotFound();

            invoice.IsPaid = true;
            _context.SaveChanges();

            // Generate PDF and send email to supplier
            var pdfDocument = GenerateInvoicePdf(invoice);
            SendInvoiceEmailToSupplier(invoice.StockOrder, pdfDocument);

            return RedirectToAction("InvoicePaid", new { id = invoice.InvoiceId });
        }

        private void SendInvoiceEmailToSupplier(StockOrder order, byte[] pdfDocument)
        {
            var fromAddress = new MailAddress("AviNashKhem2004@outlook.com", "Car Parking");
            var toAddress = new MailAddress(order.Supplier.Email, order.Supplier.Name);
            const string subject = "Invoice for Stock Order";
            string body = $"Dear {order.Supplier.Name},\n\nPlease find attached the invoice for the delivered stock order.\n\nThank you.";

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                message.Attachments.Add(new Attachment(new MemoryStream(pdfDocument), $"Invoice_{order.StockOrderId}.pdf"));

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

        private byte[] ConvertBase64ToBytes(string base64String)
        {
            // Clean and convert Base64 string to byte array
            if (base64String.Contains(","))
            {
                base64String = base64String.Substring(base64String.IndexOf(",") + 1);
            }
            base64String = base64String.Replace(" ", "").Replace("\r", "").Replace("\n", "");

            return Convert.FromBase64String(base64String);
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

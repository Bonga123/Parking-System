// File: Controllers/AdminController.cs
using CarPacking.Models;
using QRCoder;
using System;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController()
    {
        _context = ApplicationDbContext.Create();
    }

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

    public ActionResult GetRequestDetails(int requestId)
    {
        var requestDetails = _context.RequestedItems
                                     .Where(ri => ri.StockOrderRequestId == requestId)
                                     .Select(ri => new
                                     {
                                         ProductName = ri.Product.Name,
                                         Quantity = ri.Quantity
                                     })
                                     .ToList();

        return Json(requestDetails, JsonRequestBehavior.AllowGet);
    }

    public ActionResult ViewRequests()
    {
        var requests = _context.StockOrderRequests
                               .Where(r => !r.IsProcessed)
                               .Include(r => r.RequestedItems.Select(ri => ri.Product))
                               .Include(r => r.Personnel)
                               .ToList();
        return View(requests);
    }

    public ActionResult ViewQuotes()
    {
        var quotes = _context.Quotes
                             .Include(q => q.QuoteItems.Select(qi => qi.Product))
                             .Include(q => q.QuoteRequest)
                             .ToList();

        if (!quotes.Any())
        {
            return HttpNotFound("No quotes found for this request.");
        }

        return View(quotes);
    }

    [HttpPost]
    public ActionResult RequestQuotes(int requestId)
    {
        var request = _context.StockOrderRequests.Find(requestId);
        if (request == null) return HttpNotFound();

        foreach (var supplier in _context.Suppliers)
        {
            var quoteRequest = new QuoteRequest
            {
                StockOrderRequestId = requestId,
                SupplierId = supplier.SupplierId,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };
            _context.QuoteRequests.Add(quoteRequest);
        }

        request.IsProcessed = true;
        _context.SaveChanges();

        return RedirectToAction("ViewQuoteRequests", new { requestId });
    }

    public ActionResult ViewQuoteRequests(int requestId)
    {
        var quoteRequests = _context.QuoteRequests
                                    .Where(qr => qr.StockOrderRequestId == requestId)
                                    .Include(qr => qr.Supplier)
                                    .ToList();

        if (!quoteRequests.Any())
        {
            return HttpNotFound("No quote requests found for this stock order request.");
        }

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

    [HttpPost]
    public ActionResult SelectQuote(int quoteId)
    {
        var quote = _context.Quotes.Find(quoteId);
        if (quote == null) return HttpNotFound();

        quote.IsSelected = true;
        _context.SaveChanges();

        // Additional logic to proceed with the selected quote (e.g., notify the supplier)

        return RedirectToAction("ViewRequests");
    }

    public ActionResult ViewLostReports()
    {
        var reports = _context.LostPermitReports
                              .Include("ParkingPermit")
                              .Where(r => !r.IsProcessed)
                              .ToList();

        return View(reports);
    }

    [HttpPost]
    public ActionResult ReassignPermit(int reportId)
    {
        var report = _context.LostPermitReports.Find(reportId);
        if (report == null) return HttpNotFound();

        var oldPermit = report.ParkingPermit;
        oldPermit.IsActive = false;

        var newPermit = new ParkingPermit
        {
            PermitType = oldPermit.PermitType,
            Price = oldPermit.Price,
            VehicleId = oldPermit.VehicleId,
            IssuedDate = DateTime.Now,
            QRCode = GenerateQRCode(oldPermit),
            IsActive = true
        };

        _context.ParkingPermits.Add(newPermit);
        report.IsProcessed = true;
        _context.SaveChanges();

        SendEmailWithQRCode(newPermit);

        return RedirectToAction("PermitReassigned", new { id = newPermit.ParkingPermitId });
    }

    public ActionResult PermitReassigned(int id)
    {
        var permit = _context.ParkingPermits.Find(id);
        if (permit == null) return HttpNotFound();

        return View(permit);
    }

    private string GenerateQRCode(ParkingPermit permit)
    {
        string qrContent = $"Permit ID: {permit.ParkingPermitId}, Vehicle ID: {permit.VehicleId}, Issued Date: {permit.IssuedDate}";

        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new QRCode(qrCodeData))
            {
                using (var qrBitmap = qrCode.GetGraphic(20))
                {
                    string directoryPath = Server.MapPath("~/assets/images");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string fileName = $"QR_{permit.ParkingPermitId}.png";
                    string filePath = Path.Combine(directoryPath, fileName);

                    try
                    {
                        qrBitmap.Save(filePath, ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        throw new Exception("Failed to save the QR code image.", ex);
                    }

                    return $"/assets/images/{fileName}";
                }
            }
        }
    }

    private void SendEmailWithQRCode(ParkingPermit permit)
    {
        try
        {
            var fromAddress = new MailAddress("noreply@yourdomain.com", "Your Parking System");
            var toAddress = new MailAddress("student@example.com", "Student");
            const string subject = "Your New Parking Permit";
            string body = $"Dear Student,\n\nYour new parking permit has been issued. " +
                          $"Please find the attached QR code for your records.\n\n" +
                          $"Permit Type: {permit.PermitType}\n" +
                          $"Issued Date: {permit.IssuedDate}\n" +
                          $"Vehicle ID: {permit.VehicleId}\n\nThank you.";

            using (var smtp = new SmtpClient
            {
                Host = "smtp.yourdomain.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("username", "password")
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    var qrFilePath = Server.MapPath(permit.QRCode);
                    if (System.IO.File.Exists(qrFilePath))
                    {
                        message.Attachments.Add(new Attachment(qrFilePath));
                        smtp.Send(message);
                    }
                    else
                    {
                        throw new FileNotFoundException("QR code file not found.", qrFilePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle exceptions
            // Implement a logging mechanism here to capture the exception details.
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

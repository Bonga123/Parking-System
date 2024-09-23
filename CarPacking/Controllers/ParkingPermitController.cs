using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CarPacking.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;

namespace CarPacking.Controllers
{
    public class ParkingPermitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingPermitController()
        {
            _context = ApplicationDbContext.Create();
        }

        [HttpGet]
        public ActionResult MyParkingPermit(int id)
        {
            var permits = _context.ParkingPermits.Include("Vehicle").Where(x => x.VehicleId == id).FirstOrDefault();
            return View(permits);
        }

        [HttpGet]
        public ActionResult AllIssuedPermits()
        {
            var permits = _context.ParkingPermits.Include("Vehicle").ToList();
            return View(permits);
        }

        [HttpGet]
        public ActionResult IssuePermit(int vehicleId)
        {
            var vehicle = _context.Vehicles.Find(vehicleId);
            if (vehicle == null) return HttpNotFound();

            var model = new ParkingPermit
            {
                VehicleId = vehicleId,
                IssuedDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult IssuePermit(ParkingPermit permit)
        {
            if (ModelState.IsValid)
            {
                // Generate QR code for the permit and save it to the images folder
                permit.QRCode = GenerateQRCode(permit);
                permit.IssuedDate = DateTime.Now;
                _context.ParkingPermits.Add(permit);
                _context.SaveChanges();

                // Redirect to PermitIssued view after successfully issuing the permit
                return RedirectToAction("PermitIssued", new { id = permit.ParkingPermitId });
            }

            return View(permit);
        }

        public ActionResult PermitIssued(int id)
        {
            var permit = _context.ParkingPermits.Where(x => x.VehicleId == id).FirstOrDefault();
            if (permit == null) return HttpNotFound();

            return View(permit);
        }

        [HttpGet]
        public ActionResult DownloadPermitPdf(int permitId)
        {
            var permit = _context.ParkingPermits.Find(permitId);
            if (permit == null)
            {
                return HttpNotFound();
            }

            using (var memoryStream = new MemoryStream())
            {
                // Create a new PDF document
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Add title and permit details
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph("Parking Permit", titleFont));
                document.Add(new Paragraph($"Permit ID: {permit.ParkingPermitId}", bodyFont));
                document.Add(new Paragraph($"Permit Type: {permit.PermitType}", bodyFont));
                document.Add(new Paragraph($"Price: {permit.Price} Rands", bodyFont));
                document.Add(new Paragraph($"Issued Date: {permit.IssuedDate:dd-MM-yyyy}", bodyFont));

                // Add QR Code
                if (!string.IsNullOrEmpty(permit.QRCode))
                {
                    string qrPath = Server.MapPath(permit.QRCode);
                    if (System.IO.File.Exists(qrPath))
                    {
                        iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(qrPath);
                        qrImage.Alignment = Element.ALIGN_CENTER;
                        qrImage.ScaleAbsolute(100, 100); // Resize the QR code as needed
                        document.Add(qrImage);
                    }
                }

                document.Close();

                // Return the PDF as a file download
                return File(memoryStream.ToArray(), "application/pdf", $"ParkingPermit_{permit.ParkingPermitId}.pdf");
            }
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
                        string fileName = $"QR_{permit.ParkingPermitId}.png";
                        string filePath = Path.Combine(Server.MapPath("~/assets/images"), fileName);

                        qrBitmap.Save(filePath, ImageFormat.Png);

                        return $"/assets/images/{fileName}";
                    }
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

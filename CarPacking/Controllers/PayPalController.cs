using CarPacking.Models;
using Microsoft.Ajax.Utilities;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;
using System.Net;

namespace CarPacking.Controllers
{
    public class PayPalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult CreatePayment(double total, int InvId)
        {
            CookieHelper.SetIdCookie(InvId);
            var CurrentUser = User.Identity.Name;
            double convertedTot = Math.Round(total / 14.352);
            int Rem = (int)(total % 14.352);
            string Cost = convertedTot.ToString() + "." + Rem;

            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Retrieve the API credentials from configuration
            var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
            var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
            apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
            var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
            apiContext.AccessToken = accessToken;

            // Create a new payment object
            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Shop Payment"
            }
        },
                redirect_urls = new RedirectUrls
                {
                    return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                    cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                }
            };

            // Create the payment and get the approval URL
            var createdPayment = payment.Create(apiContext);
            var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

            // Redirect the user to the PayPal approval URL
            return Redirect(approvalUrl);

        }


        public ActionResult CompletePayment(string paymentId, string token, string PayerID)
        {
            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Execute the payment
            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var executedPayment = new Payment { id = paymentId }.Execute(apiContext, paymentExecution);

            // Process the payment completion
            // You can save the transaction details or perform other necessary actions

            // Redirect the user to a success page
            return RedirectToAction("PaymentSuccess");
        }

        public ActionResult CancelPayment()
        {
           
            return RedirectToAction("PaymentCancelled");
        }

        public ActionResult PaymentSuccess()
        {
            int? id = CookieHelper.GetIdFromCookie();

            if (id == null)
            {
                TempData["Error"] = "Invoice ID not found. Please try again.";
                return RedirectToAction("ViewInvoice", "ConfirmDelivery", new { id = id });
            }

            var invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("ViewInvoice", "ConfirmDelivery", new { id = id });
            }

            invoice.IsPaid = true;

            try
            {
                var assetsDir = Server.MapPath("~/assets");
                var pdfFileName = $"Invoice_{invoice.InvoiceId}.pdf";
                var pdfPath = Path.Combine(assetsDir, pdfFileName);
                var email = new MailMessage
                {
                    From = new MailAddress("AviNashKhem2004@outlook.com"),
                    Subject = $"Payment Confirmation | Invoice #{invoice.InvoiceId}",
                    Body = $"Dear {invoice.StockOrder.Supplier.Name},\n\n" +
                           "We are pleased to inform you that the payment for the received stock has been successfully processed.\n\n" +
                           "Please find the attached invoice which confirms the payment.\n\n" +
                           "If you have any questions or need further assistance, feel free to reach out to us.\n\n" +
                           "Thank you for your prompt service and cooperation.\n\n" +
                           "Best Regards,\n" +
                           "Car Parking Team",
                    IsBodyHtml = false // Set to true if you prefer HTML formatting
                };
                email.To.Add(new MailAddress(invoice.StockOrder.Supplier.Email));
                email.Attachments.Add(new Attachment(pdfPath));
                using (var smtpClient = new SmtpClient("smtp.office365.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("AviNashKhem2004@outlook.com", "avinash1"); // Replace with actual credentials
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(email);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send email: " + ex.Message;
                return RedirectToAction("ViewInvoice", "ConfirmDelivery", new { id = id });
            }
            db.Entry(invoice).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Payment successful. Invoice details have been sent to the supplier.";
            return RedirectToAction("ViewInvoice", "ConfirmDelivery", new { id = id });
        }


    }
}
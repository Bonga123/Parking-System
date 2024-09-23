using System;
using System.Web;

namespace CarPacking.Models
{
    public static class CookieHelper
    {
        // Method to set a cookie with an id
        public static void SetIdCookie(int id)
        {
            HttpCookie cookie = new HttpCookie("Id");
            cookie.Value = id.ToString();
            cookie.Expires = DateTime.Now.AddDays(1); // Set the expiration to 1 day from now
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        // Method to retrieve the id from the cookie
        public static int? GetIdFromCookie()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Id"];
            if (cookie != null && int.TryParse(cookie.Value, out int id))
            {
                return id;
            }
            return null;
        }

        // Method to delete the cookie
        public static void DeleteIdCookie()
        {
            HttpCookie cookie = new HttpCookie("Id");
            cookie.Expires = DateTime.Now.AddDays(-1); // Set the expiration to the past to delete it
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}

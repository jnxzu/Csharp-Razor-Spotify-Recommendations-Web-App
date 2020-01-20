using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyR
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int users { get; set; }
        [BindProperty]
        public int neww { get; set; }
        [BindProperty]
        public int discover { get; set; }
        [BindProperty]
        public string parames { get; set; }

        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public IActionResult OnGet()
        {
            Random r = new Random();
            var randomVal = r.Next(9000, 100000);
            users = randomVal;
            neww = randomVal * r.Next(23, 161);
            discover = randomVal * r.Next(16, 112);
            SpotifyAuth sAuth = new SpotifyAuth();
            var state = RandomString(8);
            var qb = new QueryBuilder();
            qb.Add("client_id", sAuth.clientID);
            qb.Add("response_type", "code");
            qb.Add("redirect_uri", sAuth.redirectURL);
            qb.Add("scope", "user-follow-read");
            qb.Add("state", state);
            TempData["state"] = state;
            parames = qb.ToQueryString().ToString();
            return Page();
        }
    }
}
using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyR{
    public class IndexModel : PageModel{
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public IActionResult OnGet(){
            SpotifyAuth sAuth = new SpotifyAuth();
            var state = RandomString(8);
            var qb = new QueryBuilder();
            qb.Add("client_id", sAuth.clientID);
            qb.Add("response_type", "code");
            qb.Add("redirect_uri", sAuth.redirectURL);
            qb.Add("scope", "user-follow-read");
            qb.Add("state", state);
            TempData["state"] = state;
            ViewData["params"] = qb.ToQueryString().ToString();
            return Page();
        }
    }
}
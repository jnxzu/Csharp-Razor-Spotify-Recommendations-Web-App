using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyR.Pages
{
    public class CallbackModel : PageModel{
        public IActionResult OnGet(string code, string state)
        {
            if ((string)TempData["state"] == state)
            {
                @ViewData["state"] = "Authentication Successfull";
                @ViewData["status"] = "ok";
            }
            else
            {
                @ViewData["state"] = "Authentication Failed: Invalid State";
                @ViewData["status"] = null;
            }
            TempData["state"] = null;
            return Page();
        }
    }
}
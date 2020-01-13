using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyR
{
    public class CallbackModel : PageModel
    {
        public IActionResult OnGet(string code, string state)
        {
            if ((string)TempData["state"] == state)
            {
                @ViewData["state"] = "authentication successfull";
                @ViewData["status"] = true;
            }
            else
            {
                @ViewData["state"] = "invalid state";
                @ViewData["status"] = false;
            }
            TempData["state"] = null;
            return Page();
        }
    }
}
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
                @ViewData["status"] = true;
            }
            else
            {
                @ViewData["status"] = false;
            }
            TempData["state"] = null;
            return Page();
        }
    }
}
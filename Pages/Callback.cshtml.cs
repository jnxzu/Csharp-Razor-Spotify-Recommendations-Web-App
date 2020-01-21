using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyR
{
    public class CallbackModel : PageModel
    {
        [BindProperty]
        public bool status { get; set; }

        public IActionResult OnGet(string code, string state)
        {
            if ((string)TempData["state"] == state)
            {
                @TempData["code"] = (string)HttpContext.Request.Query["code"];
                status = true;
            }
            else
            {
                status = false;
            }
            TempData["state"] = null;
            return Page();
        }
    }
}
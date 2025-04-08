
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Pages.Shared.Models;
using System.Threading.Tasks;
using Production.Data;

namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public FormData Form { get; set; }

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _db.FormDataEntries.Add(Form);
            await _db.SaveChangesAsync();

            return RedirectToPage("Results"); // Redirect to table page after submitting
        }
    }
}
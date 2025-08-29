using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages
{
    public class WellTestFormModel : PageModel
       
    {
        private readonly ApplicationDbContext _db;

        public List<Wells> WellsList { get; set; } = new List<Wells>();
        public List<WellData> WellDataList { get; set; } = new List<WellData>();
        
        [BindProperty]
        public WellData Form { get; set; }
        public WellTestFormModel (ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            WellsList = await _db.WellsEntries.ToListAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            // Debug: Check what's being received
            Console.WriteLine($"Received well_id: {Form.well_id}");

            if (!ModelState.IsValid)
            {
                // Add this to see binding errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                WellsList = await _db.WellsEntries.ToListAsync(); // Reload the list
                return Page();
            }
            _db.WellDataEntries.Add(Form);
            await _db.SaveChangesAsync();
            return RedirectToPage("Well");
        }

    }

}

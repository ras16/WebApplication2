using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages.Shared
{
    public class Results : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<DailyFieldProduction> FormDataList { get; set; } = new List<DailyFieldProduction>();

        public Results(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {

            FormDataList = await _db.DailyFieldProductionEntries.ToListAsync();
        
        }
    }
}
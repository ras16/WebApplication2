using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages.Shared
{
    public class Results : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<FormData> FormDataList { get; set; } = new List<FormData>();

        public Results(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {

            FormDataList = await _db.FormDataEntries.ToListAsync();
        
        }
    }
}
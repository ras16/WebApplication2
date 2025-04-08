using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Production.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Pages.Shared.Models
{
    public class MPA
    {
        public class MPAModel : PageModel
        {
            private readonly ApplicationDbContext _db;
            public List<WellData> WellDataList { get; set; } = new List<WellData>();
            public List<Wells> WellsList { get; set; } = new List<Wells>();
            public MPAModel(ApplicationDbContext db)
            {
                _db = db;
            }
            public async Task OnGetAsync()
            {
                WellsList = await _db.WellsEntries.ToListAsync();
            }
            public async Task<JsonResult> OnGetWellData(int wellID, int month, int year)
            {
                // Get all records that match the selected well_id
                var wellDataList = await _db.WellDataEntries                    
                    .Where(w => w.well_id == wellID &&                            
                        w.test_date.HasValue &&
                        w.test_date.Value.Month == month &&
                        w.test_date.Value.Year == year)
                    .ToListAsync();
                if (wellDataList == null || !wellDataList.Any())
                {
                    return new JsonResult(null);
                }

                // Return the full list of matching records
                return new JsonResult(wellDataList);
            }
        }
    }
}

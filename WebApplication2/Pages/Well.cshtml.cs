// First, let's update your WellModel.cs page model to return all matching records
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication2.Pages
{
    public class WellModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<WellData> WellDataList { get; set; } = new List<WellData>();
        public List<Wells> WellsList { get; set; } = new List<Wells>();
        public WellModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGetAsync()
        {
            WellsList = await _db.WellsEntries.ToListAsync();
        }
        public async Task<JsonResult> OnGetGetWellData(int wellID)
        {
            // Get all records that match the selected well_id
            var wellDataList = await _db.WellDataEntries
                                        .Where(w => w.well_id == wellID)
                                        .ToListAsync();
            if (wellDataList == null || !wellDataList.Any())
            {
                return new JsonResult(null);
            }

            // Return the full list of matching records
            return new JsonResult(wellDataList);
        }
        public void OnAddItemButtonClick()
        {

        }
    }
}
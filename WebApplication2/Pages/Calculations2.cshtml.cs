using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages
{
    public class Calculations2Model : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<AcceptedParameters> AcceptedParametersList { get; set; } = new List<AcceptedParameters>();
        public List<Calculations2ViewModel> Calculations2Data { get; set; } = new List<Calculations2ViewModel>();
        public Calculations2Model(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGetAsync()
        {
            var data = await _db.AcceptedParametersEntries
                .Include(ap => ap.Wells)
                .Include(ap => ap.ProdAllocationMain)
                .ToListAsync();

            Calculations2Data = data
                .Select(ap => new Calculations2ViewModel
                {
                    WellName = ap.Wells?.WellName,
                    PAID = ap.PAID,
                    WellType = ap.WellType,
                    DaysOn = ap.DaysOn,
                    FlowingTo = ap.FlowingTo,
                    Choke = ap.Choke,
                    THP = ap.THP,

                    // Calculations
                    Qo = (ap.Qo ?? 0) * (ap.ProdAllocationMain?.Oil_Mult ?? 0),
                    Qw = ((ap.Qo ?? 0) * (ap.ProdAllocationMain?.Oil_Mult ?? 0) * (ap.BSW ?? 0)) / (1 - (ap.BSW ?? 0)) * (ap.ProdAllocationMain?.Water_Mult ?? 0),
                    OilMT = (ap.Qo ?? 0) * (ap.DaysOn ?? 0) * (ap.SG ?? 0) * (ap.ProdAllocationMain?.Oil_Mult ?? 0),
                    WaterMT = ((ap.Qo ?? 0) * (ap.ProdAllocationMain?.Oil_Mult ?? 0) * (ap.DaysOn ?? 0) * (ap.BSW ?? 0)) / (1 - (ap.BSW ?? 0)) * (ap.ProdAllocationMain?.WaterSG ?? 0) * (ap.ProdAllocationMain?.Water_Mult ?? 0),
                    GasMm3 = ((ap.Qo ?? 0) * (ap.ProdAllocationMain?.Oil_Mult ?? 0) * (ap.DaysOn ?? 0) * (ap.TGOR ?? 0)) / 1000 * (ap.ProdAllocationMain?.LP_Gas_Mult ?? 0)
                })
                .Where(r =>
                    (r.Qo ?? 0) != 0 ||
                    (r.Qw ?? 0) != 0 ||
                    (r.OilMT ?? 0) != 0 ||
                    (r.WaterMT ?? 0) != 0 ||
                    (r.GasMm3 ?? 0) != 0
                )
                .OrderBy(r => r.WellName)
                .ToList();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages
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
        public async Task<JsonResult> OnGetGetWellData(int wellID)
        {
            // Get all records that match the selected well_id and join with Wells table
            var wellDataList = await _db.WellDataEntries
                                        .Where(w => w.well_id == wellID)
                                        .Join(_db.WellsEntries,
                                            wd => wd.well_id,
                                            w => w.WellID,
                                            (wd, w) => new {
                                                WellData = wd,
                                                WellName = w.WellName
                                            })
                                        .Select(result => new {
                                            result.WellData.id,
                                            result.WellData.well_id,
                                            well_name = result.WellName,
                                            result.WellData.test_date,
                                            result.WellData.prod_int,
                                            result.WellData.test_type,
                                            result.WellData.flow_type,
                                            result.WellData.hours_tested,
                                            result.WellData.avg_tbg_choke_64,
                                            result.WellData.avg_csg_choke_64,
                                            result.WellData.avg_thp_barg,
                                            result.WellData.avg_tht_f,
                                            result.WellData.avg_chp_barg,
                                            result.WellData.avg_oil_m3_per_day,
                                            result.WellData.avg_water_m3_per_day,
                                            result.WellData.avg_gas_10_6_m3_per_day,
                                            result.WellData.gor,
                                            result.WellData.avg_inj_gas_rate,
                                            result.WellData.oil_sg,
                                            result.WellData.gas_sg,
                                            result.WellData.oil_mt_per_day,
                                            result.WellData.nacl_ppm,
                                            result.WellData.bsw,
                                            result.WellData.test_company,
                                            result.WellData.rig_kit,
                                            result.WellData.entered_by,
                                            result.WellData.no_prod,
                                            result.WellData.comments,
                                            result.WellData.interval_top,
                                            result.WellData.interval_bottom,
                                            result.WellData.representative_ind
                                        })
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

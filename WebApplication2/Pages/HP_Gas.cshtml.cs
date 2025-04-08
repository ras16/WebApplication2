using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using System.Dynamic;
using System.Text.Json;
using WebApplication2.Pages.Shared.Models;
namespace WebApplication2.Pages
{
    public class GazModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<HP_Gas> HP_GasDataList { get; set; } = new List<HP_Gas>();
        public List<Wells> WellsList { get; set; } = new List<Wells>();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public GazModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            HP_GasDataList = await _db.HP_GasEntries.ToListAsync();
            
            //To collect ID
            WellsList = await _db.WellsEntries
               .Select(w => new Wells
               {
                   WellID = w.WellID,
                   WellName = w.WellName
               })
               .ToListAsync();

            // Get total count for pagination
            TotalRecords = await _db.HP_GasEntries.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);


            // Ensure CurrentPage is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get only the records for the current page
            HP_GasDataList = await _db.HP_GasEntries
                .OrderByDescending(c => c.Date)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
        public class HP_GasDTO
        {
            public int ID { get; set; }
            public int WellID { get; set; }
            public DateTime? Date { get; set; }
            public double? Flow_rate_m3_per_day { get; set; }
            public double? Tbg_chk_mm { get; set; }
            public double? Fthp_bar { get; set; }
            public double? Pipe_id_mm { get; set; }
            public double? Orifice_mm { get; set; }
            public double? Diff_p_in_H2O { get; set; }
            public double? Ftht_f { get; set; }
            public double? Down_time_hrs { get; set; }
            public double? Sep_liq_rate_m3_per_day { get; set; }
            public string? Comments { get; set; }
        }
        public async Task<IActionResult> OnGetPagedData([FromQuery] int page)
        {
            try
            {
                if (page < 1) page = 1;
                Console.WriteLine($"Received request for page: {page}");
                var totalRecords = await _db.HP_GasEntries.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                if (page > totalPages && totalPages > 0) page = totalPages;

                var data = await _db.HP_GasEntries
                    .OrderByDescending(c => c.Date)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
                Console.WriteLine($"Returning data. Current page: {page}, Total pages: {totalPages}");

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    wells = WellsList,
                    pagination = new
                    {
                        currentPage = page, 
                        pageSize = PageSize,
                        totalPages = totalPages,
                        totalRecords = totalRecords
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
       public async Task<IActionResult> OnPostUpdateDataAsync([FromBody] List<HP_GasDTO> gasData)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid data" });
            }
            try
            {
                foreach (var item in gasData)
                {
                    if (item.ID > 0)
                    {
                        // Update existing record
                        var existingRecord = await _db.HP_GasEntries.FindAsync(item.ID);
                        if (existingRecord != null)
                        {
                            existingRecord.WellID = item.WellID;
                            existingRecord.Date = item.Date ?? DateTime.Now;
                            existingRecord.Flow_rate_m3_per_day = item.Flow_rate_m3_per_day;
                            existingRecord.Tbg_chk_mm = item.Tbg_chk_mm;
                            existingRecord.Fthp_bar = item.Fthp_bar;
                            existingRecord.Pipe_id_mm = item.Pipe_id_mm;
                            existingRecord.Orifice_mm = item.Orifice_mm;
                            existingRecord.Diff_p_in_H2O = item.Diff_p_in_H2O;
                            existingRecord.Ftht_f = item.Ftht_f;
                            existingRecord.Down_time_hrs = item.Down_time_hrs;
                            existingRecord.Sep_liq_rate_m3_per_day = item.Sep_liq_rate_m3_per_day;
                            existingRecord.Comments = item.Comments;
                        }
                    }
                    else
                    {
                        // Add new record
                        var newRecord = new HP_Gas
                        {
                            // ID is auto-generated
                            WellID = item.WellID,
                            Date = item.Date ?? DateTime.Now,
                            Flow_rate_m3_per_day = item.Flow_rate_m3_per_day,
                            Tbg_chk_mm = item.Tbg_chk_mm,
                            Fthp_bar = item.Fthp_bar,
                            Pipe_id_mm = item.Pipe_id_mm,
                            Orifice_mm = item.Orifice_mm,
                            Diff_p_in_H2O = item.Diff_p_in_H2O,
                            Ftht_f = item.Ftht_f,
                            Down_time_hrs = item.Down_time_hrs,
                            Sep_liq_rate_m3_per_day = item.Sep_liq_rate_m3_per_day,
                            Comments = item.Comments ?? ""
                        };
                        _db.HP_GasEntries.Add(newRecord);
                    }
                }

                await _db.SaveChangesAsync();

                // Return success response
                var updatedData = await _db.HP_GasEntries.ToListAsync();
                return new JsonResult(new { success = true, data = updatedData });
            }
            catch (Exception ex)
            {
                // Log the full exception
                Console.WriteLine($"Error details: {ex.ToString()}");
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new JsonResult(new { success = false, message = errorMessage });
            }
        }
    }
}
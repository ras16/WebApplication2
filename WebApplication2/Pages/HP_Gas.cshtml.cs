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
        public List<HPGasTBL> HP_GasDataList { get; set; } = new List<HPGasTBL>();
        public List<Wells> WellsList { get; set; } = new List<Wells>();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        // Date filter properties
        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        // Well filter property
        [BindProperty(SupportsGet = true)]
        public List<int> SelectedWells { get; set; } = new List<int>();

        public GazModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // Collect Wells for dropdown
            WellsList = await _db.WellsEntries
               .Select(w => new Wells
               {
                   WellID = w.WellID,
                   WellName = w.WellName
               })
               .ToListAsync();

            // Build query with date filter
            var query = _db.HPGasTBLEntries.AsQueryable();

            // Apply date filters if provided
            if (DateFrom.HasValue)
            {
                query = query.Where(x => x.date >= DateFrom.Value.Date);
            }

            if (DateTo.HasValue)
            {
                // Include the entire end date by adding one day and using less than
                var endDate = DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.date < endDate);
            }

            // Apply well filter if provided
            if (SelectedWells != null && SelectedWells.Any())
            {
                query = query.Where(x => SelectedWells.Contains(x.WellID));
            }

            // Get total count for pagination (after filtering)
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            // Ensure CurrentPage is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get only the records for the current page (after filtering)
            HP_GasDataList = await query
                .OrderByDescending(c => c.date)//--last updates come first 
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

        public async Task<IActionResult> OnGetPagedData(
    [FromQuery] int page,
    [FromQuery] DateTime? dateFrom,
    [FromQuery] DateTime? dateTo,
    [FromQuery] List<int> selectedWells)
        {
            try
            {
                if (page < 1) page = 1;
                Console.WriteLine($"Received request for page: {page}, DateFrom: {dateFrom}, DateTo: {dateTo}");
                Console.WriteLine($"Selected wells: {string.Join(", ", selectedWells ?? new List<int>())}");

                // Build query with date filter
                var query = _db.HPGasTBLEntries.AsQueryable();

                // Apply date filters if provided
                if (dateFrom.HasValue)
                {
                    query = query.Where(x => x.date >= dateFrom.Value.Date);
                    Console.WriteLine($"Applied DateFrom filter: {dateFrom.Value.Date}");
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date by adding one day and using less than
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(x => x.date < endDate);
                    Console.WriteLine($"Applied DateTo filter: {endDate}");
                }

                // Apply well filter if provided
                if (selectedWells != null && selectedWells.Any())
                {
                    query = query.Where(x => selectedWells.Contains(x.WellID));
                    Console.WriteLine($"Applied well filter. Wells: {string.Join(", ", selectedWells)}");
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                if (page > totalPages && totalPages > 0) page = totalPages;

                var data = await query
                    .OrderByDescending(c => c.date)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                Console.WriteLine($"Returning filtered data. Total records: {totalRecords}, Current page: {page}, Total pages: {totalPages}");

                // Get wells list for dropdown
                var wellsList = await _db.WellsEntries
                   .Select(w => new Wells
                   {
                       WellID = w.WellID,
                       WellName = w.WellName
                   })
                   .ToListAsync();

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    wells = wellsList,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = PageSize,
                        totalPages = totalPages,
                        totalRecords = totalRecords
                    },
                    filters = new
                    {
                        dateFrom = dateFrom?.ToString("yyyy-MM-dd"),
                        dateTo = dateTo?.ToString("yyyy-MM-dd"),
                        selectedWells = selectedWells ?? new List<int>()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetPagedData: {ex.Message}");
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
                        var existingRecord = await _db.HPGasTBLEntries.FindAsync(item.ID);
                        if (existingRecord != null)
                        {
                            existingRecord.WellID = item.WellID;
                            existingRecord.date = item.Date ?? DateTime.Now;
                            existingRecord.flow_rate_m3_per_day = item.Flow_rate_m3_per_day;
                            existingRecord.tbg_chk_mm = item.Tbg_chk_mm;
                            existingRecord.fthp_bar = item.Fthp_bar;
                            existingRecord.pipe_id_mm = item.Pipe_id_mm;
                            existingRecord.orifice_mm = item.Orifice_mm;
                            existingRecord.diff_p_in_H2O = item.Diff_p_in_H2O;
                            existingRecord.ftht_f = item.Ftht_f;
                            existingRecord.down_time_hrs = item.Down_time_hrs;
                            existingRecord.Sep_liq_rate_m3_per_day = item.Sep_liq_rate_m3_per_day;
                            existingRecord.Comments = item.Comments;
                        }
                    }
                    else
                    {
                        // Add new record
                        var newRecord = new HPGasTBL
                        {
                            // ID is auto-generated
                            WellID = item.WellID,
                            date = item.Date ?? DateTime.Now,
                            flow_rate_m3_per_day = item.Flow_rate_m3_per_day,
                            tbg_chk_mm = item.Tbg_chk_mm,
                            fthp_bar = item.Fthp_bar,
                            pipe_id_mm = item.Pipe_id_mm,
                            orifice_mm = item.Orifice_mm,
                            diff_p_in_H2O = item.Diff_p_in_H2O,
                            ftht_f = item.Ftht_f,
                            down_time_hrs = item.Down_time_hrs,
                            Sep_liq_rate_m3_per_day = item.Sep_liq_rate_m3_per_day,
                            Comments = item.Comments ?? ""
                        };
                        _db.HPGasTBLEntries.Add(newRecord);
                    }
                }

                await _db.SaveChangesAsync();

                // Return success response
                return new JsonResult(new { success = true, message = "Data updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the full exception
                Console.WriteLine($"Error details: {ex.ToString()}");
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new JsonResult(new { success = false, message = errorMessage });
            }
        }

        // Helper method to get filtered query (can be reused)
        private IQueryable<HPGasTBL> GetFilteredQuery(DateTime? dateFrom, DateTime? dateTo, List<int>? selectedWells = null)
        {
            var query = _db.HPGasTBLEntries.AsQueryable();

            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.date >= dateFrom.Value.Date);
            }

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.date < endDate);
            }

            // Apply well filter if provided
            if (selectedWells != null && selectedWells.Any())
            {
                query = query.Where(x => selectedWells.Contains(x.WellID));
            }

            return query;
        }

        public async Task<IActionResult> OnGetAllData(
     [FromQuery] DateTime? dateFrom,
     [FromQuery] DateTime? dateTo,
     [FromQuery] List<int> selectedWells)
        {
            try
            {
                Console.WriteLine($"Received request for all data. DateFrom: {dateFrom}, DateTo: {dateTo}");
                Console.WriteLine($"Selected wells for export: {string.Join(", ", selectedWells ?? new List<int>())}");

                // Build query with all filters (date and well)
                var query = _db.HPGasTBLEntries.AsQueryable();

                // Apply date filters if provided
                if (dateFrom.HasValue)
                {
                    query = query.Where(x => x.date >= dateFrom.Value.Date);
                    Console.WriteLine($"Applied DateFrom filter: {dateFrom.Value.Date}");
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date by adding one day and using less than
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(x => x.date < endDate);
                    Console.WriteLine($"Applied DateTo filter: {endDate}");
                }

                // Apply well filter if provided
                if (selectedWells != null && selectedWells.Any())
                {
                    query = query.Where(x => selectedWells.Contains(x.WellID));
                    Console.WriteLine($"Applied well filter for export. Wells: {string.Join(", ", selectedWells)}");
                }

                // Get ALL records (no pagination) - ordered by date for consistency
                var data = await query
                    .OrderByDescending(x => x.date) // Match the same ordering as paginated data
                    .ToListAsync();

                Console.WriteLine($"Returning all filtered HP Gas data. Total records: {data.Count}");

                // Get wells list for dropdown (for wellId display names)
                var wellsList = await _db.WellsEntries
                   .Select(w => new Wells
                   {
                       WellID = w.WellID,
                       WellName = w.WellName
                   })
                   .ToListAsync();

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    wells = wellsList,
                    totalRecords = data.Count,
                    filter = new
                    {
                        dateFrom = dateFrom?.ToString("yyyy-MM-dd"),
                        dateTo = dateTo?.ToString("yyyy-MM-dd"),
                        selectedWells = selectedWells ?? new List<int>()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetAllData: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        //Method to get summary statistics for filtered data
        public async Task<IActionResult> OnGetFilteredSummaryAsync(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] List<int> selectedWells)
        {
            try
            {
                var query = GetFilteredQuery(dateFrom, dateTo, selectedWells);

                var summaryData = await query
                    .GroupBy(x => 1) // Group all records together
                    .Select(g => new
                    {
                        TotalRecords = g.Count(),
                        TotalFlowRate = g.Sum(x => x.flow_rate_m3_per_day ?? 0),
                        AvgTbgChk = g.Average(x => x.tbg_chk_mm ?? 0),
                        AvgFthp = g.Average(x => x.fthp_bar ?? 0),
                        AvgFtht = g.Average(x => x.ftht_f ?? 0),
                        TotalDownTime = g.Sum(x => x.down_time_hrs ?? 0),
                        TotalSepLiqRate = g.Sum(x => x.Sep_liq_rate_m3_per_day ?? 0)
                    })
                    .FirstOrDefaultAsync();

                return new JsonResult(new
                {
                    success = true,
                    summary = summaryData ?? new
                    {
                        TotalRecords = 0,
                        TotalFlowRate = 0.0,
                        AvgTbgChk = 0.0,
                        AvgFthp = 0.0,
                        AvgFtht = 0.0,
                        TotalDownTime = 0.0,
                        TotalSepLiqRate = 0.0
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
    }
}
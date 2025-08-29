using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using WebApplication2.Pages.Shared.Models;
using static WebApplication2.Pages.CompressorInfoModel;

namespace WebApplication2.Pages
{
    public class DailyWellDataModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<DailyWellData> DailyWellDataList { get; set; } = new List<DailyWellData>();
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

        public DailyWellDataModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public class SummaryStatistics
        {
            public int TotalWells { get; set; }
            public decimal TotalWellsChange { get; set; }
            public string TotalWellsChangeType { get; set; } = "increase";

            public decimal DailyProduction { get; set; }
            public decimal DailyProductionChange { get; set; }
            public string DailyProductionChangeType { get; set; } = "increase";

            public decimal AverageTHP { get; set; }
            public decimal AverageTHPChange { get; set; }
            public string AverageTHPChangeType { get; set; } = "increase";

            public decimal TotalDowntime { get; set; }
            public decimal DowntimeChange { get; set; }
            public string DowntimeChangeType { get; set; } = "decrease";
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
            var query = _db.DailyWellData.AsQueryable();

            // Apply date filters if provided
            if (DateFrom.HasValue)
            {
                query = query.Where(x => x.Date >= DateFrom.Value.Date);
            }

            if (DateTo.HasValue)
            {
                // Include the entire end date by adding one day and using less than
                var endDate = DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < endDate);
            }

            // Get total count for pagination (after filtering)
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            // Ensure CurrentPage is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get only the records for the current page (after filtering)
            DailyWellDataList = await query
                .OrderByDescending(c => c.Date) //last updates come first
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetPagedDataAsync([FromQuery] int page, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                if (page < 1) page = 1;
                Console.WriteLine($"Received request for page: {page}, DateFrom: {dateFrom}, DateTo: {dateTo}");

                // Build query with date filter
                var query = _db.DailyWellData.AsQueryable();

                // Apply date filters if provided
                if (dateFrom.HasValue)
                {
                    query = query.Where(x => x.Date >= dateFrom.Value.Date);
                    Console.WriteLine($"Applied DateFrom filter: {dateFrom.Value.Date}");
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date by adding one day and using less than
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(x => x.Date < endDate);
                    Console.WriteLine($"Applied DateTo filter: {endDate}");
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                if (page > totalPages && totalPages > 0) page = totalPages;

                var data = await query
                    .OrderByDescending(c => c.Date)
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

                // Use camelCase property names for JSON to match JavaScript expectations
                var formattedData = data.Select(w => new
                {
                    id = w.ID,
                    date = w.Date,
                    wellID = w.WellID,
                    wellType = w.WellType,
                    horizon = w.Horizon,
                    flow = w.Flow,
                    tbg_Choke_mm = w.Tbg_Choke_mm,
                    csg_Choke_mm = w.Csg_Choke_mm,
                    thP_barg = w.THP_barg,
                    thT_C = w.THT_C,
                    chP_barg = w.CHP_barg,
                    flowing_To = w.Flowing_To,
                    sep_press_bar = w.Sep_press_bar,
                    sep_temp_C = w.Sep_temp_C,
                    liq_rate_m3d = w.Liq_rate_m3d,
                    bsW_pcnt = w.BSW_pcnt,
                    fl_Line_Pres_barg = w.Fl_Line_Pres_barg,
                    liquid_lvl_csg_m = w.Liquid_lvl_csg_m,
                    pumping_Speed_spm = w.Pumping_Speed_spm,
                    stroke_Length_m = w.Stroke_Length_m,
                    pump_Depth_m = w.Pump_Depth_m,
                    vol_Inj_m3 = w.Vol_Inj_m3,
                    hrs_Inj_hrs = w.Hrs_Inj_hrs,
                    gas_rate_mm_m3d = w.Gas_rate_mm_m3d,
                    specific_gravity_oil = w.Specific_gravity_oil,
                    specific_gravity_gas = w.Specific_gravity_gas,
                    howc = w.HOWC,
                    gor = w.GOR,
                    field1 = w.Field1,
                    downTime = w.DownTime,
                    remarks = w.Remarks
                });

                return new JsonResult(new
                {
                    success = true,
                    data = formattedData,
                    wells = wellsList,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = PageSize,
                        totalPages = totalPages,
                        totalRecords = totalRecords
                    },
                    filter = new
                    {
                        dateFrom = dateFrom?.ToString("yyyy-MM-dd"),
                        dateTo = dateTo?.ToString("yyyy-MM-dd")
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetPagedDataAsync: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error fetching data: {ex.Message}"
                });
            }
        }

        // Add method to get all data for CSV export
        public async Task<IActionResult> OnGetAllData([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                Console.WriteLine($"Received request for all data. DateFrom: {dateFrom}, DateTo: {dateTo}");

                // Build query with date filter (same logic as OnGetPagedData but without pagination)
                var query = _db.DailyWellData.AsQueryable();

                // Apply date filters if provided
                if (dateFrom.HasValue)
                {
                    query = query.Where(x => x.Date >= dateFrom.Value.Date);
                    Console.WriteLine($"Applied DateFrom filter: {dateFrom.Value.Date}");
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date by adding one day and using less than
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(x => x.Date < endDate);
                    Console.WriteLine($"Applied DateTo filter: {endDate}");
                }

                // Get ALL records (no pagination) - ordered by date for consistency
                var data = await query
                    .OrderBy(x => x.Date) // You can change this to OrderByDescending if preferred
                    .ToListAsync();

                Console.WriteLine($"Returning all filtered data. Total records: {data.Count}");

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
                    totalRecords = data.Count,
                    filter = new
                    {
                        dateFrom = dateFrom?.ToString("yyyy-MM-dd"),
                        dateTo = dateTo?.ToString("yyyy-MM-dd")
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

        // UPDATED SUMMARY STATISTICS METHOD WITH PERIOD SUPPORT
        public async Task<IActionResult> OnGetSummaryStatisticsAsync([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] string period = "month")
        {
            try
            {
                Console.WriteLine($"Getting summary statistics. DateFrom: {dateFrom}, DateTo: {dateTo}, Period: {period}");

                // Validate inputs
                if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Invalid date range: 'From' date cannot be after 'To' date"
                    });
                }

                // Calculate date ranges based on period type
                DateTime currentStart, currentEnd, previousStart, previousEnd;
                string periodDescription;

                switch (period.ToLower())
                {
                    case "week":
                        CalculateWeeklyPeriods(dateFrom, dateTo, out currentStart, out currentEnd, out previousStart, out previousEnd);
                        periodDescription = "from last week";
                        break;

                    case "month":
                        CalculateMonthlyPeriods(dateFrom, dateTo, out currentStart, out currentEnd, out previousStart, out previousEnd);
                        periodDescription = "from last month";
                        break;

                    case "quarter":
                        CalculateQuarterlyPeriods(dateFrom, dateTo, out currentStart, out currentEnd, out previousStart, out previousEnd);
                        periodDescription = "from last quarter";
                        break;

                    case "year":
                        CalculateYearlyPeriods(dateFrom, dateTo, out currentStart, out currentEnd, out previousStart, out previousEnd);
                        periodDescription = "from last year";
                        break;

                    default: // "30days" or any other value
                        Calculate30DayPeriods(dateFrom, dateTo, out currentStart, out currentEnd, out previousStart, out previousEnd);
                        periodDescription = "from last period";
                        break;
                }

                Console.WriteLine($"Current period: {currentStart:yyyy-MM-dd} to {currentEnd:yyyy-MM-dd}");
                Console.WriteLine($"Previous period: {previousStart:yyyy-MM-dd} to {previousEnd:yyyy-MM-dd}");

                // Get current period statistics with null handling
                var currentStats = await _db.DailyWellData
                    .Where(x => x.Date >= currentStart && x.Date <= currentEnd.AddDays(1))
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        UniqueWells = g.Select(x => x.WellID).Distinct().Count(),
                        TotalProduction = g.Sum(x => (decimal)(x.Liq_rate_m3d ?? 0)),
                        AvgTHP = g.Where(x => x.THP_barg.HasValue).Average(x => (decimal)(x.THP_barg ?? 0)),
                        TotalDowntime = g.Sum(x => (decimal)(x.DownTime ?? 0)),
                        RecordCount = g.Count()
                    })
                    .FirstOrDefaultAsync();

                // Get previous period statistics with null handling
                var previousStats = await _db.DailyWellData
                    .Where(x => x.Date >= previousStart && x.Date <= previousEnd.AddDays(1))
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        UniqueWells = g.Select(x => x.WellID).Distinct().Count(),
                        TotalProduction = g.Sum(x => (decimal)(x.Liq_rate_m3d ?? 0)),
                        AvgTHP = g.Where(x => x.THP_barg.HasValue).Average(x => (decimal)(x.THP_barg ?? 0)),
                        TotalDowntime = g.Sum(x => (decimal)(x.DownTime ?? 0)),
                        RecordCount = g.Count()
                    })
                    .FirstOrDefaultAsync();

                // Set defaults if no data
                currentStats = currentStats ?? new { UniqueWells = 0, TotalProduction = 0m, AvgTHP = 0m, TotalDowntime = 0m, RecordCount = 0 };
                previousStats = previousStats ?? new { UniqueWells = 0, TotalProduction = 0m, AvgTHP = 0m, TotalDowntime = 0m, RecordCount = 0 };

                // Calculate percentage changes with better precision
                var summary = new SummaryStatistics
                {
                    TotalWells = currentStats.UniqueWells,
                    TotalWellsChange = CalculatePercentageChange(currentStats.UniqueWells, previousStats.UniqueWells),
                    TotalWellsChangeType = GetChangeType(currentStats.UniqueWells, previousStats.UniqueWells),

                    DailyProduction = Math.Round(currentStats.TotalProduction, 1),
                    DailyProductionChange = CalculatePercentageChange(currentStats.TotalProduction, previousStats.TotalProduction),
                    DailyProductionChangeType = GetChangeType(currentStats.TotalProduction, previousStats.TotalProduction),

                    AverageTHP = Math.Round(currentStats.AvgTHP, 1),
                    AverageTHPChange = CalculatePercentageChange(currentStats.AvgTHP, previousStats.AvgTHP),
                    AverageTHPChangeType = GetChangeType(currentStats.AvgTHP, previousStats.AvgTHP),

                    TotalDowntime = Math.Round(currentStats.TotalDowntime, 1),
                    DowntimeChange = CalculatePercentageChange(currentStats.TotalDowntime, previousStats.TotalDowntime),
                    DowntimeChangeType = GetChangeType(currentStats.TotalDowntime, previousStats.TotalDowntime, true)
                };

                Console.WriteLine($"Summary calculated successfully");

                return new JsonResult(new
                {
                    success = true,
                    summary = summary,
                    currentPeriod = new { start = currentStart.ToString("yyyy-MM-dd"), end = currentEnd.ToString("yyyy-MM-dd") },
                    previousPeriod = new { start = previousStart.ToString("yyyy-MM-dd"), end = previousEnd.ToString("yyyy-MM-dd") },
                    periodDescription = periodDescription,
                    metadata = new
                    {
                        currentRecords = currentStats.RecordCount,
                        previousRecords = previousStats.RecordCount,
                        periodType = period
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetSummaryStatisticsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error calculating statistics: {ex.Message}"
                });
            }
        }

        // PERIOD CALCULATION HELPER METHODS
        private void CalculateMonthlyPeriods(DateTime? dateFrom, DateTime? dateTo, out DateTime currentStart, out DateTime currentEnd, out DateTime previousStart, out DateTime previousEnd)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                // Use provided date range as current period
                currentStart = dateFrom.Value.Date;
                currentEnd = dateTo.Value.Date;

                // Calculate equivalent previous period
                var periodMonths = ((currentEnd.Year - currentStart.Year) * 12) + currentEnd.Month - currentStart.Month;
                if (periodMonths <= 0) periodMonths = 1;

                previousStart = currentStart.AddMonths(-periodMonths);
                previousEnd = currentEnd.AddMonths(-periodMonths);
            }
            else if (dateFrom.HasValue)
            {
                // From date to end of current month
                currentStart = dateFrom.Value.Date;
                currentEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

                // Previous month
                var prevMonth = currentStart.AddMonths(-1);
                previousStart = new DateTime(prevMonth.Year, prevMonth.Month, 1);
                previousEnd = new DateTime(prevMonth.Year, prevMonth.Month, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
            }
            else if (dateTo.HasValue)
            {
                // Full month ending at dateTo
                currentEnd = dateTo.Value.Date;
                currentStart = new DateTime(currentEnd.Year, currentEnd.Month, 1);

                // Previous month
                previousEnd = currentStart.AddDays(-1);
                previousStart = new DateTime(previousEnd.Year, previousEnd.Month, 1);
            }
            else
            {
                // Current month vs previous month
                var today = DateTime.Today;
                currentStart = new DateTime(today.Year, today.Month, 1);
                currentEnd = today;

                // Previous month
                var prevMonth = currentStart.AddMonths(-1);
                previousStart = prevMonth;
                previousEnd = currentStart.AddDays(-1);
            }
        }

        private void CalculateWeeklyPeriods(DateTime? dateFrom, DateTime? dateTo, out DateTime currentStart, out DateTime currentEnd, out DateTime previousStart, out DateTime previousEnd)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                currentStart = dateFrom.Value.Date;
                currentEnd = dateTo.Value.Date;
                var days = (currentEnd - currentStart).Days + 1;
                previousStart = currentStart.AddDays(-days);
                previousEnd = currentStart.AddDays(-1);
            }
            else
            {
                // Current week vs previous week (Monday to Sunday)
                var today = DateTime.Today;
                var daysFromMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
                currentStart = today.AddDays(-daysFromMonday); // Monday of current week
                currentEnd = today;

                previousStart = currentStart.AddDays(-7);
                previousEnd = currentStart.AddDays(-1);
            }
        }

        private void CalculateQuarterlyPeriods(DateTime? dateFrom, DateTime? dateTo, out DateTime currentStart, out DateTime currentEnd, out DateTime previousStart, out DateTime previousEnd)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                currentStart = dateFrom.Value.Date;
                currentEnd = dateTo.Value.Date;
                var months = ((currentEnd.Year - currentStart.Year) * 12) + currentEnd.Month - currentStart.Month;
                previousStart = currentStart.AddMonths(-Math.Max(months, 3));
                previousEnd = currentEnd.AddMonths(-Math.Max(months, 3));
            }
            else
            {
                // Current quarter vs previous quarter
                var today = DateTime.Today;
                var currentQuarter = (today.Month - 1) / 3 + 1;
                currentStart = new DateTime(today.Year, (currentQuarter - 1) * 3 + 1, 1);
                currentEnd = today;

                previousStart = currentStart.AddMonths(-3);
                previousEnd = currentStart.AddDays(-1);
            }
        }

        private void CalculateYearlyPeriods(DateTime? dateFrom, DateTime? dateTo, out DateTime currentStart, out DateTime currentEnd, out DateTime previousStart, out DateTime previousEnd)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                currentStart = dateFrom.Value.Date;
                currentEnd = dateTo.Value.Date;
                previousStart = currentStart.AddYears(-1);
                previousEnd = currentEnd.AddYears(-1);
            }
            else
            {
                // Current year vs previous year
                var today = DateTime.Today;
                currentStart = new DateTime(today.Year, 1, 1);
                currentEnd = today;

                previousStart = new DateTime(today.Year - 1, 1, 1);
                previousEnd = new DateTime(today.Year - 1, 12, 31);
            }
        }

        private void Calculate30DayPeriods(DateTime? dateFrom, DateTime? dateTo, out DateTime currentStart, out DateTime currentEnd, out DateTime previousStart, out DateTime previousEnd)
        {
            // Your existing 30-day logic
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                currentStart = dateFrom.Value.Date;
                currentEnd = dateTo.Value.Date;
                var periodDays = Math.Max(1, (currentEnd - currentStart).Days + 1);
                previousStart = currentStart.AddDays(-periodDays);
                previousEnd = currentStart.AddDays(-1);
            }
            else
            {
                currentEnd = DateTime.Today;
                currentStart = currentEnd.AddDays(-29);
                previousEnd = currentStart.AddDays(-1);
                previousStart = previousEnd.AddDays(-29);
            }
        }

        // Enhanced helper methods
        private decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            try
            {
                var change = ((current - previous) / previous) * 100;
                return Math.Round(change, 1);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string GetChangeType(decimal current, decimal previous, bool lowerIsBetter = false)
        {
            if (Math.Abs(current - previous) < 0.01m) return "neutral";

            var isIncrease = current > previous;

            if (lowerIsBetter)
            {
                return isIncrease ? "decrease" : "increase";
            }

            return isIncrease ? "increase" : "decrease";
        }

        // Helper method to get filtered query (can be reused)
        private IQueryable<DailyWellData> GetFilteredQuery(DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _db.DailyWellData.AsQueryable();

            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.Date >= dateFrom.Value.Date);
            }

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < endDate);
            }

            return query;
        }

        // REST OF YOUR EXISTING METHODS (keeping them unchanged)
        public class DailyWellDataDTO
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }

            public DateTime Date { get; set; }

            public int WellID { get; set; }

            [MaxLength(250)]
            public string? WellType { get; set; }

            [MaxLength(255)]
            public string? Horizon { get; set; }

            [MaxLength(250)]
            public string? Flow { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Tbg_Choke_mm { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Csg_Choke_mm { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? THP_barg { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? THT_C { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? CHP_barg { get; set; }

            [MaxLength(215)]
            public string? Flowing_To { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? Sep_press_bar { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? Sep_temp_C { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? Liq_rate_m3d { get; set; }

            public float? BSW_pcnt { get; set; }

            [Column(TypeName = "numeric(18, 1)")]
            public decimal? Fl_Line_Pres_barg { get; set; }

            public float? Liquid_lvl_csg_m { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Pumping_Speed_spm { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Stroke_Length_m { get; set; }

            public float? Pump_Depth_m { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Vol_Inj_m3 { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? Hrs_Inj_hrs { get; set; }

            [Column(TypeName = "numeric(18, 3)")]
            public decimal? Gas_rate_mm_m3d { get; set; }

            [Column(TypeName = "numeric(18, 3)")]
            public decimal? Specific_gravity_oil { get; set; }

            [Column(TypeName = "numeric(18, 3)")]
            public decimal? Specific_gravity_gas { get; set; }

            [MaxLength(255)]
            public string? HOWC { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? GOR { get; set; }

            public string? Field1 { get; set; }

            public float? DownTime { get; set; }

            [MaxLength(50)]
            public string? Remarks { get; set; }
        }

        public async Task<IActionResult> OnPostUpdateDailyWellDataAsync([FromBody] List<DailyWellData> dailyWellDataList)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid data" });
            }

            try
            {
                foreach (var item in dailyWellDataList)
                {
                    if (item.ID > 0)
                    {
                        // Update existing record
                        var existingRecord = await _db.DailyWellData.FindAsync(item.ID);
                        if (existingRecord != null)
                        {
                            existingRecord.Date = item.Date;
                            existingRecord.WellID = item.WellID;
                            existingRecord.WellType = item.WellType;
                            existingRecord.Horizon = item.Horizon;
                            existingRecord.Flow = item.Flow;
                            existingRecord.Tbg_Choke_mm = item.Tbg_Choke_mm;
                            existingRecord.Csg_Choke_mm = item.Csg_Choke_mm;
                            existingRecord.THP_barg = item.THP_barg;
                            existingRecord.THT_C = item.THT_C;
                            existingRecord.CHP_barg = item.CHP_barg;
                            existingRecord.Flowing_To = item.Flowing_To;
                            existingRecord.Sep_press_bar = item.Sep_press_bar;
                            existingRecord.Sep_temp_C = item.Sep_temp_C;
                            existingRecord.Liq_rate_m3d = item.Liq_rate_m3d;
                            existingRecord.BSW_pcnt = item.BSW_pcnt;
                            existingRecord.Fl_Line_Pres_barg = item.Fl_Line_Pres_barg;
                            existingRecord.Liquid_lvl_csg_m = item.Liquid_lvl_csg_m;
                            existingRecord.Pumping_Speed_spm = item.Pumping_Speed_spm;
                            existingRecord.Stroke_Length_m = item.Stroke_Length_m;
                            existingRecord.Pump_Depth_m = item.Pump_Depth_m;
                            existingRecord.Vol_Inj_m3 = item.Vol_Inj_m3;
                            existingRecord.Hrs_Inj_hrs = item.Hrs_Inj_hrs;
                            existingRecord.Gas_rate_mm_m3d = item.Gas_rate_mm_m3d;
                            existingRecord.Specific_gravity_oil = item.Specific_gravity_oil;
                            existingRecord.Specific_gravity_gas = item.Specific_gravity_gas;
                            existingRecord.HOWC = item.HOWC;
                            existingRecord.GOR = item.GOR;
                            existingRecord.Field1 = item.Field1;
                            existingRecord.DownTime = item.DownTime;
                            existingRecord.Remarks = item.Remarks;
                        }
                    }
                    else
                    {
                        // Add new record
                        var newRecord = new DailyWellData
                        {
                            Date = item.Date,
                            WellID = item.WellID,
                            WellType = item.WellType,
                            Horizon = item.Horizon,
                            Flow = item.Flow,
                            Tbg_Choke_mm = item.Tbg_Choke_mm,
                            Csg_Choke_mm = item.Csg_Choke_mm,
                            THP_barg = item.THP_barg,
                            THT_C = item.THT_C,
                            CHP_barg = item.CHP_barg,
                            Flowing_To = item.Flowing_To,
                            Sep_press_bar = item.Sep_press_bar,
                            Sep_temp_C = item.Sep_temp_C,
                            Liq_rate_m3d = item.Liq_rate_m3d,
                            BSW_pcnt = item.BSW_pcnt,
                            Fl_Line_Pres_barg = item.Fl_Line_Pres_barg,
                            Liquid_lvl_csg_m = item.Liquid_lvl_csg_m,
                            Pumping_Speed_spm = item.Pumping_Speed_spm,
                            Stroke_Length_m = item.Stroke_Length_m,
                            Pump_Depth_m = item.Pump_Depth_m,
                            Vol_Inj_m3 = item.Vol_Inj_m3,
                            Hrs_Inj_hrs = item.Hrs_Inj_hrs,
                            Gas_rate_mm_m3d = item.Gas_rate_mm_m3d,
                            Specific_gravity_oil = item.Specific_gravity_oil,
                            Specific_gravity_gas = item.Specific_gravity_gas,
                            HOWC = item.HOWC,
                            GOR = item.GOR,
                            Field1 = item.Field1,
                            DownTime = item.DownTime,
                            Remarks = item.Remarks
                        };

                        await _db.DailyWellData.AddAsync(newRecord);
                    }
                }

                await _db.SaveChangesAsync();

                // Return success
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

        // Optional: Add method to get summary statistics for filtered data
        public async Task<IActionResult> OnGetFilteredSummaryAsync([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                var query = GetFilteredQuery(dateFrom, dateTo);

                var summaryData = await query
                    .GroupBy(x => 1) // Group all records together
                    .Select(g => new
                    {
                        TotalRecords = g.Count(),
                        TotalLiquidRate = g.Sum(x => (double)(x.Liq_rate_m3d ?? 0)),
                        AvgTHP = g.Average(x => (double)(x.THP_barg ?? 0)),
                        AvgTHT = g.Average(x => (double)(x.THT_C ?? 0)),
                        AvgCHP = g.Average(x => (double)(x.CHP_barg ?? 0)),
                        TotalDownTime = g.Sum(x => (double)(x.DownTime ?? 0)),
                        TotalGasRate = g.Sum(x => (double)(x.Gas_rate_mm_m3d ?? 0))
                    })
                    .FirstOrDefaultAsync();

                return new JsonResult(new
                {
                    success = true,
                    summary = summaryData ?? new
                    {
                        TotalRecords = 0,
                        TotalLiquidRate = 0.0,
                        AvgTHP = 0.0,
                        AvgTHT = 0.0,
                        AvgCHP = 0.0,
                        TotalDownTime = 0.0,
                        TotalGasRate = 0.0
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
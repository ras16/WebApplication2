// Update your CompressorInfoModel.cs with these changes

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Production.Data;
using WebApplication2.Pages.Shared.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication2.Pages
{
    public class CompressorInfoModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<CompressorInfo> CompressorInfoDataList { get; set; } = new List<CompressorInfo>();

        public List<Compressors> CompressorsList { get; set; } = new List<Compressors>();//To get Compressor ID
        
        // NEW: Add this property to hold all unique compressor names
        public List<string> AllCompressorNames { get; set; } = new List<string>();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        // Date filter properties
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        // Compressor filter property
        [BindProperty(SupportsGet = true)]
        public string[] SelectedCompressors { get; set; } = new string[0];

        public CompressorInfoModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // Build base query
            var query = _db.CompressorInfoEntries.AsQueryable();

            // Apply date filters if provided
            if (FromDate.HasValue)
            {
                query = query.Where(c => c.CompressDate >= FromDate.Value.Date);
            }

            if (ToDate.HasValue)
            {
                query = query.Where(c => c.CompressDate <= ToDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            // Apply compressor filter if provided
            if (SelectedCompressors != null && SelectedCompressors.Any())
            {
                query = query.Where(c => SelectedCompressors.Contains(c.Compressor));
            }

            // Get total count for pagination (after filtering)
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            // To collect Compressor Master Data (for dropdown)
            CompressorsList = await _db.CompressorsEntries
               .Select(w => new Compressors
               {
                   id = w.id,
                   CompressorName = w.CompressorName
               })
               .ToListAsync();

            // Get ALL unique compressor names from CompressorInfo table (not just current page)
            // This will be used for the Available Compressors pool
            AllCompressorNames = await _db.CompressorInfoEntries
                .Where(c => !string.IsNullOrEmpty(c.Compressor))
                .Select(c => c.Compressor)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Ensure CurrentPage is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get only the records for the current page (with filters applied)
            CompressorInfoDataList = await query
                .OrderByDescending(c => c.CompressDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public class CompressorInfoDTO
        {
            public int ID { get; set; }
            public DateTime? CompressDate { get; set; }
            public string? Compressor { get; set; }
            public int? GasRate { get; set; }
            public int? Downtime { get; set; }
            public string? Comments { get; set; }
        }

        public async Task<IActionResult> OnGetPagedData(
    [FromQuery] int page,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate,
    [FromQuery] string[] selectedCompressors) // Changed from List<string> to string[]
        {
            try
            {
                if (page < 1) page = 1;
                Console.WriteLine($"Received request for page: {page}, FromDate: {fromDate}, ToDate: {toDate}");

                // Convert to list and filter out null/empty values
                var compressorFilter = selectedCompressors?.Where(c => !string.IsNullOrEmpty(c)).ToList() ?? new List<string>();
                Console.WriteLine($"Selected compressors: {string.Join(", ", compressorFilter)}");

                // Build base query
                var query = _db.CompressorInfoEntries.AsQueryable();

                // Apply date filters if provided
                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CompressDate >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CompressDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
                }

                // Apply compressor filter if provided
                if (compressorFilter.Any())
                {
                    query = query.Where(c => !string.IsNullOrEmpty(c.Compressor) && compressorFilter.Contains(c.Compressor));
                    Console.WriteLine($"Applied compressor filter. Compressors: {string.Join(", ", compressorFilter)}");
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                if (page > totalPages && totalPages > 0) page = totalPages;

                var data = await query
                    .OrderByDescending(c => c.CompressDate)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                Console.WriteLine($"Returning filtered data. Current page: {page}, Total pages: {totalPages}, Total records: {totalRecords}");

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    Compressors = CompressorsList,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = PageSize,
                        totalPages = totalPages,
                        totalRecords = totalRecords
                    },
                    filters = new
                    {
                        fromDate = fromDate?.ToString("yyyy-MM-dd"),
                        toDate = toDate?.ToString("yyyy-MM-dd"),
                        selectedCompressors = compressorFilter
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetPagedData: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.StackTrace
                });
            }
        }

        public async Task<IActionResult> OnGetAllData(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string[] selectedCompressors) // Changed from List<string> to string[]
        {
            try
            {
                Console.WriteLine($"Received request for all compressor data. FromDate: {fromDate}, ToDate: {toDate}");

                // Convert to list and filter out null/empty values
                var compressorFilter = selectedCompressors?.Where(c => !string.IsNullOrEmpty(c)).ToList() ?? new List<string>();
                Console.WriteLine($"Selected compressors for export: {string.Join(", ", compressorFilter)}");

                // Build query with all filters (date and compressor)
                var query = _db.CompressorInfoEntries.AsQueryable();

                // Apply date filters if provided
                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CompressDate >= fromDate.Value.Date);
                    Console.WriteLine($"Applied FromDate filter: {fromDate.Value.Date}");
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CompressDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
                    Console.WriteLine($"Applied ToDate filter: {toDate.Value.Date}");
                }

                // Apply compressor filter if provided
                if (compressorFilter.Any())
                {
                    query = query.Where(c => !string.IsNullOrEmpty(c.Compressor) && compressorFilter.Contains(c.Compressor));
                    Console.WriteLine($"Applied compressor filter for export. Compressors: {string.Join(", ", compressorFilter)}");
                }

                // Get ALL records (no pagination) - ordered by date for consistency
                var data = await query
                    .OrderByDescending(c => c.CompressDate)
                    .ToListAsync();

                Console.WriteLine($"Returning all filtered compressor data. Total records: {data.Count}");

                // Get compressors list for reference
                var compressorsList = await _db.CompressorsEntries
                   .Select(w => new Compressors
                   {
                       id = w.id,
                       CompressorName = w.CompressorName
                   })
                   .ToListAsync();

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    compressors = compressorsList,
                    totalRecords = data.Count,
                    filter = new
                    {
                        fromDate = fromDate?.ToString("yyyy-MM-dd"),
                        toDate = toDate?.ToString("yyyy-MM-dd"),
                        selectedCompressors = compressorFilter
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetAllData: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.StackTrace // Add stack trace for debugging
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateDataAsync([FromBody] List<CompressorInfoDTO> compressorData)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid data" });
            }
            try
            {
                foreach (var item in compressorData)
                {
                    if (item.ID > 0)
                    {
                        // Update existing record
                        var existingRecord = await _db.CompressorInfoEntries.FindAsync(item.ID);
                        if (existingRecord != null)
                        {
                            existingRecord.CompressDate = item.CompressDate;
                            existingRecord.Compressor = item.Compressor;
                            existingRecord.GasRate = item.GasRate;
                            existingRecord.Downtime = item.Downtime;
                            existingRecord.Comments = item.Comments;
                        }
                    }
                    else
                    {
                        // Add new record
                        var newRecord = new CompressorInfo
                        {
                            CompressDate = item.CompressDate,
                            Compressor = item.Compressor,
                            GasRate = item.GasRate,
                            Downtime = item.Downtime,
                            Comments = item.Comments
                        };
                        await _db.CompressorInfoEntries.AddAsync(newRecord);
                    }
                }
                await _db.SaveChangesAsync();

                // Return success but not the data (will be loaded via pagination)
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnPostUpdateDataAsync: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
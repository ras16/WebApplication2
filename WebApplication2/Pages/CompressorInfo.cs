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

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public CompressorInfoModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // Get total count for pagination
            TotalRecords = await _db.CompressorInfoEntries.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            //To collect ID
            CompressorsList = await _db.CompressorsEntries
               .Select( w => new Compressors
               {
                   id = w.id,
                   CompressorName = w.CompressorName
               })
               .ToListAsync();


            // Ensure CurrentPage is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get only the records for the current page
            CompressorInfoDataList = await _db.CompressorInfoEntries
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

        public async Task<IActionResult> OnGetPagedData([FromQuery] int page)
        {
            try
            {
                
                if (page < 1) page = 1;
                Console.WriteLine($"Received request for page: {page}");
                var totalRecords = await _db.CompressorInfoEntries.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                if (page > totalPages && totalPages > 0) page = totalPages;

                var data = await _db.CompressorInfoEntries
                    .OrderByDescending(c => c.CompressDate)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
                Console.WriteLine($"Returning data. Current page: {page}, Total pages: {totalPages}");

                return new JsonResult(new
                {
                    success = true,
                    data = data,
                    Compressors = CompressorsList,
                    pagination = new
                    {
                        currentPage = page,  // Make sure this is using the page parameter
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
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
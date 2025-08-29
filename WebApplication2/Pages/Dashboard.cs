using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Production.Data;
using System.Text.Json;
using WebApplication2.Pages.Shared.Models; // Add this using statement

namespace WebApplication2.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IStringLocalizer<DashboardModel> _localizer;

        // Add date filter parameter
        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; } = "monthly"; // Default to monthly

        public int TotalWells { get; set; }
        public int TotalCompressors { get; set; }
        public int TotalHPGas { get; set; }
        public string ProdAreasDataJson { get; set; } = "[]";
        public string WellTypeDataJson { get; set; } = "[]";
        public string ProductionTrendsDataJson { get; set; } = "[]";
        public string FlaredGasByProdAreaJson { get; set; } = "[]";
        public string CompressorDowntimeDataJson { get; set; } = "[]";
        public string FlayerAlocDataJson { get; set; } = "[]";
        public string FlayerAlocTrendsDataJson { get; set; } = "[]";

        // Production rates from WellData for Key Production Metrics
        public string AverageOilProductionRate { get; set; } = "0";
        public string AverageGasProductionRate { get; set; } = "0";
        public string AverageWaterProductionRate { get; set; } = "0";
        public string AverageGORRate { get; set; } = "0";

        // Add properties for filter info
        public string FilterStartDate { get; set; }
        public string FilterEndDate { get; set; }
        public string FilterDescription { get; set; }

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // Calculate date range based on filter
            var dateRange = GetDateRange(DateFilter);
            FilterStartDate = dateRange.StartDate.ToString("yyyy-MM-dd");
            FilterEndDate = dateRange.EndDate.ToString("yyyy-MM-dd");
            FilterDescription = GetFilterDescription(DateFilter);

            // Filter FlayerAloc data by date range
            var flayerAlocDataForWells = await _db.FlayerAloc
                .Where(f => f.date.HasValue && f.wellid > 0 &&
                           f.date.Value >= dateRange.StartDate &&
                           f.date.Value <= dateRange.EndDate)
                .Select(f => new {
                    f.id,
                    f.Name,
                    f.wellid,
                    f.date,
                    f.layer,
                    f.oil,
                    f.gas,
                    f.water
                })
                .ToListAsync();

            // Count total wells (not filtered by date as these are static counts)
            TotalWells = await _db.WellsEntries.CountAsync();
            TotalCompressors = await _db.CompressorsEntries.CountAsync();
            TotalHPGas = await _db.HPGasTBLEntries.CountAsync();

            // Calculate average production rates from WellData filtered by date range
            var wellDataWithRates = await _db.WellDataEntries
                .Where(w => (w.avg_oil_m3_per_day.HasValue || w.avg_gas_10_6_m3_per_day.HasValue ||
                            w.avg_water_m3_per_day.HasValue || w.gor.HasValue) &&
                           w.test_date.HasValue &&
                           w.test_date.Value >= dateRange.StartDate &&
                           w.test_date.Value <= dateRange.EndDate)
                .ToListAsync();

            if (wellDataWithRates.Any())
            {
                var avgOil = wellDataWithRates
                    .Where(w => w.avg_oil_m3_per_day.HasValue)
                    .Average(w => w.avg_oil_m3_per_day.Value);

                var avgGas = wellDataWithRates
                    .Where(w => w.avg_gas_10_6_m3_per_day.HasValue)
                    .Average(w => w.avg_gas_10_6_m3_per_day.Value);

                var avgWater = wellDataWithRates
                    .Where(w => w.avg_water_m3_per_day.HasValue)
                    .Average(w => w.avg_water_m3_per_day.Value);

                var avgGOR = wellDataWithRates
                    .Where(w => w.gor.HasValue)
                    .Average(w => w.gor.Value);

                AverageOilProductionRate = avgOil.ToString("N1");
                AverageGasProductionRate = avgGas.ToString("N1");
                AverageWaterProductionRate = avgWater.ToString("N1");
                AverageGORRate = avgGOR.ToString("N0");
            }

            // Get production areas data (not filtered by date as this appears to be configuration data)
            var prodAreasData = await _db.Prod_Areas_TBL
                .Select(p => new {
                    Label = p.Prod_Area,
                    Value = p.vishka_tank ?? 0
                })
                .ToListAsync();

            // Get well type data filtered by date range
            var wellTypeData = await _db.DailyWellData
                .Where(w => !string.IsNullOrEmpty(w.WellType) &&
                           w.Date >= dateRange.StartDate &&
                           w.Date <= dateRange.EndDate)
                .GroupBy(w => w.WellType)
                .Select(g => new {
                    Label = g.Key,
                    Value = g.Select(x => x.WellID).Distinct().Count()
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            // Get Compressor Downtime data filtered by date range
            var compressorDowntimeData = await _db.CompressorInfoEntries
                .Where(c => !string.IsNullOrEmpty(c.Compressor) && c.Downtime.HasValue &&
                           c.CompressDate.HasValue &&
                           c.CompressDate.Value >= dateRange.StartDate &&
                           c.CompressDate.Value <= dateRange.EndDate)
                .GroupBy(c => c.Compressor)
                .Select(g => new {
                    Label = g.Key,
                    Value = g.Sum(x => x.Downtime ?? 0)
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            if (!compressorDowntimeData.Any())
            {
                compressorDowntimeData.Add(new { Label = "No Data", Value = 0 });
            }

            // Get FlaredGas by ProdArea data filtered by date range
            var prodAreaNames = await _db.Prod_Areas_TBL
                .Select(p => new { p.prod_area_id, p.Prod_Area })
                .ToListAsync();

            var flaredGasData = await _db.DailyFieldProductionEntries
                .Where(d => d.FlaredGas.HasValue &&
                           d.Date >= dateRange.StartDate &&
                           d.Date <= dateRange.EndDate)
                .GroupBy(d => d.prod_area_id)
                .Select(g => new {
                    ProdAreaId = g.Key,
                    TotalFlaredGas = g.Sum(x => x.FlaredGas.Value),
                    RecordCount = g.Count()
                })
                .ToListAsync();

            var flaredGasByProdArea = prodAreaNames
                .GroupJoin(flaredGasData,
                    area => area.prod_area_id,
                    flared => flared.ProdAreaId,
                    (area, flaredGroup) => new {
                        area.Prod_Area,
                        FlaredData = flaredGroup.FirstOrDefault()
                    })
                .Select(joined => new {
                    Label = joined.Prod_Area,
                    Value = Math.Round(joined.FlaredData?.TotalFlaredGas ?? 0.0, 2)
                })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToList();

            if (!flaredGasByProdArea.Any())
            {
                flaredGasByProdArea.Add(new { Label = "No Data", Value = 0.0 });
            }

            // Get production trends data filtered by date range
            var allWellData = await _db.WellDataEntries
                .Where(w => w.test_date.HasValue &&
                           w.test_date.Value >= dateRange.StartDate &&
                           w.test_date.Value <= dateRange.EndDate)
                .ToListAsync();

            var productionTrendsData = GetProductionTrendsData(allWellData, DateFilter);

            // Get FlayerAloc production trends data filtered by date range
            var flayerAlocData = await _db.FlayerAloc
                .Where(f => f.date.HasValue && !string.IsNullOrEmpty(f.layer) &&
                           f.date.Value >= dateRange.StartDate &&
                           f.date.Value <= dateRange.EndDate)
                .ToListAsync();

            var flayerTrendsData = GetFlayerAlocTrendsData(flayerAlocData, DateFilter);

            // Serialize to JSON for use in JavaScript
            ProdAreasDataJson = JsonSerializer.Serialize(prodAreasData);
            WellTypeDataJson = JsonSerializer.Serialize(wellTypeData);
            ProductionTrendsDataJson = JsonSerializer.Serialize(productionTrendsData);
            FlaredGasByProdAreaJson = JsonSerializer.Serialize(flaredGasByProdArea);
            CompressorDowntimeDataJson = JsonSerializer.Serialize(compressorDowntimeData);
            FlayerAlocTrendsDataJson = JsonSerializer.Serialize(flayerTrendsData);
            FlayerAlocDataJson = JsonSerializer.Serialize(flayerAlocDataForWells);
        }

        private (DateTime StartDate, DateTime EndDate) GetDateRange(string filter)
        {
            var now = DateTime.Now;
            var today = now.Date;

            return filter.ToLower() switch
            {
                "lastweek" => (today.AddDays(-7), today),
                "last30days" => (today.AddDays(-30), today),
                "monthly" => (today.AddMonths(-12), today), // Show last 12 months
                "quarterly" => (today.AddMonths(-36), today), // Show last 3 years of quarters
                "yearly" => (today.AddYears(-5), today), // Show last 5 years
                _ => (today.AddMonths(-12), today) // Default to last 12 months
            };
        }

        private (DateTime StartDate, DateTime EndDate) GetQuarterRange(DateTime date)
        {
            // For quarterly view, show last 3 years
            return (date.AddYears(-3), date.Date);
        }

        private string GetFilterDescription(string filter)
        {
            return filter.ToLower() switch
            {
                "lastweek" => "Last 7 Days",
                "last30days" => "Last 30 Days",
                "monthly" => "Last 12 Months",
                "quarterly" => "Last 3 Years (Quarterly)",
                "yearly" => "Last 5 Years",
                _ => "Last 12 Months"
            };
        }

        private List<object> GetProductionTrendsData(List<WellData> allData, string filter)
        {
            var groupedData = filter.ToLower() switch
            {
                "lastweek" or "last30days" => GroupByDay(allData),
                "monthly" => GroupByMonth(allData),
                "quarterly" => GroupByMonth(allData), // Show monthly data within quarter
                "yearly" => GroupByMonth(allData),
                _ => GroupByMonth(allData)
            };

            return groupedData.Cast<object>().ToList();
        }

        private List<object> GroupByDay(List<WellData> data)
        {
            return data
                .Where(w => w.test_date.HasValue)
                .GroupBy(w => w.test_date.Value.Date)
                .Select(g => new {
                    Period = g.Key.Day,
                    PeriodName = g.Key.ToString("MMM dd"),
                    TotalOil = g.Sum(x => (double)(x.avg_oil_m3_per_day ?? 0)),
                    TotalWater = g.Sum(x => (double)(x.avg_water_m3_per_day ?? 0)),
                    TotalGas = g.Sum(x => (double)(x.avg_gas_10_6_m3_per_day ?? 0))
                })
                .OrderBy(x => x.Period)
                .Cast<object>()
                .ToList();
        }

        private List<object> GroupByMonth(List<WellData> data)
        {
            return data
                .Where(w => w.test_date.HasValue)
                .GroupBy(w => new { Year = w.test_date.Value.Year, Month = w.test_date.Value.Month })
                .Select(g => new {
                    Period = g.Key.Month,
                    PeriodName = $"{GetMonthName(g.Key.Month)} {g.Key.Year}", // Include year
                    TotalOil = g.Sum(x => (double)(x.avg_oil_m3_per_day ?? 0)),
                    TotalWater = g.Sum(x => (double)(x.avg_water_m3_per_day ?? 0)),
                    TotalGas = g.Sum(x => (double)(x.avg_gas_10_6_m3_per_day ?? 0))
                })
                .OrderBy(x => x.Period)
                .Cast<object>()
                .ToList();
        }

        private List<object> GetFlayerAlocTrendsData(List<FlayerAloc> flayerAlocData, string filter)
        {
            // Group FlayerAloc data by layer and time period
            var groupedData = filter.ToLower() switch
            {
                "lastweek" or "last30days" => GroupFlayerByDay(flayerAlocData),
                "monthly" => GroupFlayerByMonth(flayerAlocData),
                "quarterly" => GroupFlayerByMonth(flayerAlocData),
                "yearly" => GroupFlayerByMonth(flayerAlocData),
                _ => GroupFlayerByMonth(flayerAlocData)
            };

            return groupedData;
        }

        private List<object> GroupFlayerByDay(List<FlayerAloc> data)
        {
            var grouped = data
                .Where(f => f.date.HasValue && !string.IsNullOrEmpty(f.layer))
                .GroupBy(f => new { Date = f.date.Value.Date, Layer = f.layer })
                .GroupBy(g => g.Key.Date)
                .Select(dayGroup => new {
                    Period = dayGroup.Key.Day,
                    PeriodName = dayGroup.Key.ToString("MMM dd"),
                    Layers = dayGroup
                        .GroupBy(g => g.Key.Layer)
                        .ToDictionary(
                            layerGroup => layerGroup.Key,
                            layerGroup => new {
                                Oil = layerGroup.SelectMany(g => g).Sum(x => x.oil),
                                Water = layerGroup.SelectMany(g => g).Sum(x => x.water),
                                Gas = layerGroup.SelectMany(g => g).Sum(x => x.gas)
                            }
                        )
                })
                .OrderBy(x => x.Period)
                .Cast<object>()
                .ToList();

            return grouped;
        }

        private List<object> GroupFlayerByMonth(List<FlayerAloc> data)
        {
            var grouped = data
                .Where(f => f.date.HasValue && !string.IsNullOrEmpty(f.layer))
                .GroupBy(f => new {
                    Year = f.date.Value.Year,
                    Month = f.date.Value.Month,
                    Layer = f.layer
                })
                .GroupBy(g => new { g.Key.Year, g.Key.Month })
                .Select(monthGroup => new {
                    Period = monthGroup.Key.Month,
                    PeriodName = $"{GetMonthName(monthGroup.Key.Month)} {monthGroup.Key.Year}", // Include year
                    Layers = monthGroup
                        .GroupBy(g => g.Key.Layer)
                        .ToDictionary(
                            layerGroup => layerGroup.Key,
                            layerGroup => new {
                                Oil = layerGroup.SelectMany(g => g).Sum(x => x.oil),
                                Water = layerGroup.SelectMany(g => g).Sum(x => x.water),
                                Gas = layerGroup.SelectMany(g => g).Sum(x => x.gas)
                            }
                        )
                })
                .OrderBy(x => x.Period)
                .Cast<object>()
                .ToList();

            return grouped;
        }

        private static string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Jan",
                2 => "Feb",
                3 => "Mar",
                4 => "Apr",
                5 => "May",
                6 => "Jun",
                7 => "Jul",
                8 => "Aug",
                9 => "Sep",
                10 => "Oct",
                11 => "Nov",
                12 => "Dec",
                _ => "Unknown"
            };
        }
    }
}
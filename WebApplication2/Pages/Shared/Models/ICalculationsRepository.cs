using Microsoft.EntityFrameworkCore;
using Production.Data;

namespace WebApplication2.Pages.Shared.Models
{
    /// <summary>
    /// Repository interface for handling calculations data access
    /// </summary>
    public interface ICalculationsRepository
    {
        /// <summary>
        /// Gets calculations for a specific date range
        /// </summary>
        Task<List<Calculations1>> GetCalculationsForDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets accepted parameters for calculations
        /// </summary>
        Task<List<dynamic>> GetAcceptedParametersAsync();

        /// <summary>
        /// Refreshes calculations data for a specific date range
        /// </summary>
        Task<RefreshDataResult> RefreshCalculationsDataAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Result of refreshing calculations data
    /// </summary>
    public class RefreshDataResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ProcessedCount { get; set; }
    }

    /// <summary>
    /// Implementation of calculations repository
    /// </summary>
    public class CalculationsRepository : ICalculationsRepository
    {
        private readonly ApplicationDbContext _context;

        public CalculationsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets calculations for a specific date range
        /// </summary>
        public async Task<List<Calculations1>> GetCalculationsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Calculations1
                .Where(c => c.CalcDate >= startDate && c.CalcDate <= endDate)
                .OrderBy(c => c.CalcDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets accepted parameters for calculations
        /// </summary>
        public async Task<List<dynamic>> GetAcceptedParametersAsync()
        {
            var acceptedParametersList = await _context.AcceptedParametersEntries
                .Join(_context.WellsEntries,
                        av => av.WellID,
                        w => w.WellID,
                        (av, w) => new {
                            AcceptedParameter = av,
                            WellName = w.WellName ?? "Unknown"
                        })
                .Select(result => new {
                    id = result.AcceptedParameter.ID,
                    paid = result.AcceptedParameter.PAID,
                    well_id = result.AcceptedParameter.WellID,
                    well_name = result.WellName,
                    daysOn = result.AcceptedParameter.DaysOn,
                    well_type = result.AcceptedParameter.WellType,
                    flow_type = result.AcceptedParameter.FlowingTo,
                    choke = (double?)result.AcceptedParameter.Choke ?? 0,
                    csg_chk = (double?)result.AcceptedParameter.CsgChk ?? 0,
                    thp = (double?)result.AcceptedParameter.THP ?? 0,
                    chp = (double?)result.AcceptedParameter.CHP ?? 0,
                    avg_oil_m3_per_day = 0,
                    avg_water_m3_per_day = 0,
                    avg_gas_10_6_m3_per_day = 0,
                    gor = (double?)result.AcceptedParameter.GOR ?? 0,
                    avg_qg_inj = (double?)result.AcceptedParameter.AvgQgInj ?? 0,
                    sg = (double?)result.AcceptedParameter.SG ?? 0,
                    qo = (double?)result.AcceptedParameter.Qo ?? 0,
                    bsw = (double?)result.AcceptedParameter.BSW ?? 0,
                    qg_tot = (double?)result.AcceptedParameter.Qg ?? 0,
                    test_date = result.AcceptedParameter.TestDate,
                })
                .ToListAsync();

            return acceptedParametersList.Cast<dynamic>().ToList();
        }

        /// <summary>
        /// Refreshes calculations data for a specific date range
        /// </summary>
        public async Task<RefreshDataResult> RefreshCalculationsDataAsync(DateTime startDate, DateTime endDate)
        {
            var result = new RefreshDataResult();

            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    result.Success = false;
                    result.Message = "Start date cannot be after end date";
                    return result;
                }

                // Remove existing records for the date range
                var existingData = await _context.Calculations1
                    .Where(c => c.CalcDate >= startDate && c.CalcDate <= endDate)
                    .ToListAsync();

                if (existingData.Any())
                {
                    _context.Calculations1.RemoveRange(existingData);
                    await _context.SaveChangesAsync();
                }

                // Dictionary to store consolidated data by date
                var consolidatedData = new Dictionary<DateTime, Calculations1>();
                var prodAreaId = 1; // Default prod area ID

                // Get data from DailyFieldProduction
                var dailyFieldDataSql = $@"
                    SELECT Date, ExportGas, FlaredGas 
                    FROM DailyFieldProduction
                    WHERE (Date BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}') 
                    AND prod_area_id={prodAreaId} 
                    GROUP BY Date, ExportGas, FlaredGas 
                    ORDER BY Date";

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = dailyFieldDataSql;

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var date = reader.GetDateTime(0); // Date
                            double? exportGas = reader.IsDBNull(1) ? null : (double?)reader.GetDouble(1); // ExportGas
                            double? flaredGas = reader.IsDBNull(2) ? null : (double?)reader.GetDouble(2); // FlaredGas

                            consolidatedData[date] = new Calculations1
                            {
                                CalcDate = date,
                                ExportGas = exportGas,
                                FlaredGas = flaredGas
                            };
                        }
                    }
                }

                // Get data from HPGasTbl
                await LoadHPGasData(startDate, endDate, consolidatedData);

                // Get data from CompressorInfo
                await LoadCompressorData(startDate, endDate, consolidatedData);

                // Get data from ProdAllocationMain
                await LoadCompFuelGasData(startDate, endDate, consolidatedData, prodAreaId);

                // Add all data to Calculations1 table
                if (consolidatedData.Count > 0)
                {
                    await _context.Calculations1.AddRangeAsync(consolidatedData.Values);
                    await _context.SaveChangesAsync();

                    result.Success = true;
                    result.ProcessedCount = consolidatedData.Count;
                    result.Message = $"Successfully processed {consolidatedData.Count} days of data.";
                }
                else
                {
                    result.Success = false;
                    result.Message = "No data found for the selected date range.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Loads HP Gas data from database
        /// </summary>
        private async Task LoadHPGasData(DateTime startDate, DateTime endDate, Dictionary<DateTime, Calculations1> consolidatedData)
        {
            var hpGasSql = $@"
                SELECT date, SUM(flow_rate_m3_per_day) as HPGasFlow 
                FROM HPGasTBL 
                WHERE date BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' 
                Group by date";

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = hpGasSql;

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var date = reader.GetDateTime(0); // Date
                        double? hpGasFlow = reader.IsDBNull(1) ? null : (double?)reader.GetDouble(1); // HPGasFlow

                        if (consolidatedData.ContainsKey(date))
                        {
                            consolidatedData[date].HPGasFlow = hpGasFlow;
                        }
                        else
                        {
                            consolidatedData[date] = new Calculations1
                            {
                                CalcDate = date,
                                HPGasFlow = hpGasFlow
                            };
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Loads Compressor data from database
        /// </summary>
        private async Task LoadCompressorData(DateTime startDate, DateTime endDate, Dictionary<DateTime, Calculations1> consolidatedData)
        {
            var compressorSql = $@"
                SELECT CompressDate, SUM(GasRate) as GasRate 
                FROM CompressorInfo 
                WHERE CompressDate BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' 
                Group by CompressDate";

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = compressorSql;

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var date = reader.GetDateTime(0); // Date
                        double? gasRate = reader.IsDBNull(1) ? null : (double?)reader.GetDouble(1); // GasRate

                        if (consolidatedData.ContainsKey(date))
                        {
                            consolidatedData[date].CompressorGasRate = gasRate;
                        }
                        else
                        {
                            consolidatedData[date] = new Calculations1
                            {
                                CalcDate = date,
                                CompressorGasRate = gasRate
                            };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads Compressor Fuel Gas data from database
        /// </summary>
        private async Task LoadCompFuelGasData(DateTime startDate, DateTime endDate, Dictionary<DateTime, Calculations1> consolidatedData, int prodAreaId)
        {
            // If the date range crosses multiple months, try to get comp_fuel_gas for each month
            var monthlyCompFuelGas = new Dictionary<DateTime, double?>();
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var lastMonth = new DateTime(endDate.Year, endDate.Month, 1);

            // Loop through each month in the range
            do
            {
                var monthlyProdAllocationSql = $@"
                    SELECT Comp_Fuel_Gas 
                    FROM ProdAllocationMain 
                    WHERE Month='{currentDate:yyyy-MM-dd}' AND prod_area_id={prodAreaId} 
                    ORDER BY PAID DESC";

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = monthlyProdAllocationSql;

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            double? compFuelGas = reader.IsDBNull(0) ? null : (double?)reader.GetDouble(0);
                            monthlyCompFuelGas[currentDate] = compFuelGas;
                        }
                    }
                }

                currentDate = currentDate.AddMonths(1); // Move to next month
            }
            while (currentDate <= lastMonth);

            // Set CompFuelGas based on the month of each CalcDate
            double? firstMonthValue = null;
            if (monthlyCompFuelGas.Count > 0)
            {
                firstMonthValue = monthlyCompFuelGas.First().Value;
            }

            foreach (var date in consolidatedData.Keys.ToList())
            {
                var monthStart = new DateTime(date.Year, date.Month, 1);
                if (monthlyCompFuelGas.ContainsKey(monthStart))
                {
                    consolidatedData[date].CompFuelGas = monthlyCompFuelGas[monthStart];
                }
                else
                {
                    // If no specific value for this month, use the first month's value
                    consolidatedData[date].CompFuelGas = firstMonthValue;
                }
            }
        }
    }
}
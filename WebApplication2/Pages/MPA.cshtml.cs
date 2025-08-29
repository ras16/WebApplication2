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
        public async Task<JsonResult> OnGetGetWellData(int wellID, int month, int year)
        {
            // Get records that match the selected well_id and the specified month and year
            var wellDataList = await _db.WellDataEntries
                .Where(w => w.well_id == wellID &&
                           w.test_date.HasValue &&
                           w.test_date.Value.Month == month &&
                           w.test_date.Value.Year == year)
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

            // Return the filtered list of matching records
            return new JsonResult(wellDataList);
        }

        public async Task<JsonResult> OnGetSaveAcceptedValue(int recordId)
        {
            try
            {
                // Get the record details from WellDataEntries
                var wellDataRecord = await _db.WellDataEntries
                    .FirstOrDefaultAsync(wd => wd.id == recordId);

                if (wellDataRecord == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Record not found in Well Data."
                    });
                }

                // Check if a record already exists for this well and month/year
                if (wellDataRecord.test_date.HasValue)
                {
                    var month = wellDataRecord.test_date.Value.Month;
                    var year = wellDataRecord.test_date.Value.Year;
                    var wellId = wellDataRecord.well_id;

                    var existingMonthlyRecord = await _db.AcceptedParametersEntries
                        .FirstOrDefaultAsync(ap =>
                            ap.WellID == wellId &&
                            ap.TestDate.HasValue &&
                            ap.TestDate.Value.Month == month &&
                            ap.TestDate.Value.Year == year);

                    if (existingMonthlyRecord != null)
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = $"A record for this well for {month}/{year} already exists in Accepted Values."
                        });
                    }
                }

                // First check if this specific record already exists in AcceptedValues
                //var existingRecord = await _db.AcceptedValuesEntries
                //    .FirstOrDefaultAsync(av => av.id == recordId);

                //if (existingRecord != null)
                //{
                //    return new JsonResult(new
                //    {
                //        success = false,
                //        message = "This record is already in the Accepted Values table."
                //    });
                //}

                // Generate a new PAID
                int newPaid = await GenerateNewPaidForMonthAndYear(wellDataRecord.test_date);

                // Calculate DaysOn value for this well
                int daysOn = await GetWellTypeDaysCount(wellDataRecord.well_id);

                // Get the WellType from the most recent DailyWellData entry
                var wellType = await GetWellType(wellDataRecord.well_id);

                // Calculate Qg value (DaysOn * avg_gas_10_6_m3_per_day)
                float qg = 0;
                if (wellDataRecord.avg_gas_10_6_m3_per_day.HasValue)
                {
                    qg = (float)(daysOn * wellDataRecord.avg_gas_10_6_m3_per_day.Value);
                }

                // Create a new AcceptedValues entry
                var acceptedParameter = new AcceptedParameters
                {
                    PAID = newPaid,
                    WellID = wellDataRecord.well_id,
                    WellType = wellType,
                    DaysOn = daysOn,
                    FlowingTo = wellDataRecord.flow_type,
                    Choke = (float?)wellDataRecord.avg_tbg_choke_64,
                    THP = (float?)wellDataRecord.avg_thp_barg,
                    Qo = (float?)wellDataRecord.oil_mt_per_day,
                    GOR = (float?)wellDataRecord.gor,
                    AvgQgInj = (float?)wellDataRecord.avg_inj_gas_rate,
                    BSW = (float?)wellDataRecord.bsw,
                    SG = (float?)wellDataRecord.oil_sg,
                    Qg = qg,
                    CHP = (float?)wellDataRecord.avg_chp_barg,
                    CsgChk = (float?)wellDataRecord.avg_csg_choke_64,
                    TestDate = wellDataRecord.test_date,
                    // Other fields remain null for now
                };

                // Add to database and save changes
                _db.AcceptedParametersEntries.Add(acceptedParameter);
                await _db.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = "Record successfully added to Accepted Values."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error saving record: {ex.Message}"
                });
            }
        }

        // Helper method to get the WellType for a specific well
        private async Task<string> GetWellType(int wellId)
        {
            try
            {
                // Get the WellType from the most recent DailyWellData entry
                var latestWellData = await _db.DailyWellData
                    .Where(d => d.WellID == wellId)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefaultAsync();

                return latestWellData?.WellType ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Helper method to generate a new PAID based on month and year
        private async Task<int> GenerateNewPaidForMonthAndYear(DateTime? testDate)
        {
            if (!testDate.HasValue)
            {
                // Fallback if no test date
                return await _db.AcceptedParametersEntries.MaxAsync(ap => ap.PAID) + 1;
            }

            // Get the current highest PAID
            int maxPaid = 0;
            if (await _db.AcceptedParametersEntries.AnyAsync())
            {
                maxPaid = await _db.AcceptedParametersEntries.MaxAsync(av => av.PAID);
            }

            return maxPaid + 1;
        }

        // Helper method to get the count of equal WellType for a given WellID
        private async Task<int> GetWellTypeDaysCount(int wellId)
        {
            try
            {
                // First get the WellType for this specific well from the most recent DailyWellData entry
                var latestWellData = await _db.DailyWellData
                    .Where(d => d.WellID == wellId)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefaultAsync();

                if (latestWellData == null || string.IsNullOrEmpty(latestWellData.WellType))
                {
                    return 0; // No data or no well type found
                }

                // Count all entries with the same WellType for this WellID
                var count = await _db.DailyWellData
                    .Where(d => d.WellID == wellId &&
                           d.WellType == latestWellData.WellType &&
                           d.Flow == latestWellData.Flow)
                    .CountAsync();

                return count;
            }
            catch (Exception)
            {
                return 0; // Return 0 if there's an error
            }
        }

        public async Task<JsonResult> OnGetCheckExistingMonthlyRecord(int wellId, int month, int year)
        {
            try
            {
                // Check if a record exists for this well and month/year
                var existingRecord = await _db.AcceptedParametersEntries
                    .AnyAsync(av =>
                        av.WellID == wellId &&
                        av.TestDate.HasValue &&
                        av.TestDate.Value.Month == month &&
                        av.TestDate.Value.Year == year);

                return new JsonResult(new
                {
                    exists = existingRecord
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    error = true,
                    message = $"Error checking existing record: {ex.Message}"
                });
            }
        }

        public async Task<JsonResult> OnGetGetAcceptedValues([FromQuery] int wellID)
        {
            try
            {
                // Get all accepted values and join with Wells table for names
                var acceptedParametersList = await _db.AcceptedParametersEntries
                    .Where(av => av.WellID == wellID)
                    .Join(_db.WellsEntries,
                        av => av.WellID,
                        w => w.WellID,
                        (av, w) => new {
                            AcceptedParameter = av,
                            WellName = w.WellName ?? "Unknown" // Add null check for well name
                        })
                    .Select(result => new {
                        id = result.AcceptedParameter.ID,
                        paid = result.AcceptedParameter.PAID,
                        well_id = result.AcceptedParameter.WellID,
                        well_name = result.WellName,
                        daysOn = result.AcceptedParameter.DaysOn,
                        well_type = result.AcceptedParameter.WellType,
                        flow_type = result.AcceptedParameter.FlowingTo,
                        avg_tbg_choke_64 = (double?)result.AcceptedParameter.Choke,
                        avg_csg_choke_64 = (double?)result.AcceptedParameter.CsgChk,
                        avg_thp_barg = (double?)result.AcceptedParameter.THP,
                        avg_chp_barg = (double?)result.AcceptedParameter.CHP,
                        avg_oil_m3_per_day = 0, // Not directly mapped
                        avg_water_m3_per_day = 0, // Not directly mapped
                        avg_gas_10_6_m3_per_day = 0, // Calculated from Qg/DaysOn if needed
                        gor = (double?)result.AcceptedParameter.GOR,
                        avg_inj_gas_rate = (double?)result.AcceptedParameter.AvgQgInj,
                        oil_sg = (double?)result.AcceptedParameter.SG,
                        oil_mt_per_day = (double?)result.AcceptedParameter.Qo,
                        bsw = (double?)result.AcceptedParameter.BSW,
                        qg = (double?)result.AcceptedParameter.Qg,
                        test_date = result.AcceptedParameter.TestDate,
                    })
                    .ToListAsync();

                // Check if the list is null or empty
                //if (acceptedValuesList == null || !acceptedValuesList.Any())
                //{
                //    return new JsonResult(new List<object>()); // Return empty list
                //}

                return new JsonResult(acceptedParametersList);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    error = true,
                    message = $"Error retrieving accepted values: {ex.Message}"
                });
            }
        }

        public async Task<JsonResult> OnGetRemoveAcceptedValue(int recordId)
        {
            try
            {
                // Find the record in AcceptedValues
                var record = await _db.AcceptedParametersEntries
                    .FirstOrDefaultAsync(av => av.ID == recordId);

                if (record == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Record not found in Accepted Values."
                    });
                }

                // Remove the record
                _db.AcceptedParametersEntries.Remove(record);
                await _db.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = "Record successfully removed from Accepted Values."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error removing record: {ex.Message}"
                });
            }
        }

        public async Task<JsonResult> OnGetUpdateAllDaysOn()
        {
            try
            {
                // Get all records from AcceptedValues
                var acceptedValues = await _db.AcceptedParametersEntries.ToListAsync();
                int updatedCount = 0;

                foreach (var record in acceptedValues)
                {
                    // Calculate DaysOn for each record based on both WellType and Flow
                    int daysOn = await GetWellTypeDaysCount(record.WellID);

                    // Update the record
                    record.DaysOn = daysOn;
                    updatedCount++;
                }

                // Save all changes
                await _db.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = $"Successfully updated DaysOn for {updatedCount} records."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error updating DaysOn values: {ex.Message}"
                });
            }
        }
    }
}
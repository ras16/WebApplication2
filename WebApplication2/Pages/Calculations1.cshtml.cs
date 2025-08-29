using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Pages.Shared.Models;

namespace WebApplication2.Pages
{
    public class Calculation1 : PageModel
    {
        private readonly ICalculationsRepository _calculationsRepository;
        private readonly ICalculationService _calculationService;

        public Calculation1(ICalculationsRepository calculationsRepository, ICalculationService calculationService)
        {
            _calculationsRepository = calculationsRepository;
            _calculationService = calculationService;
        }

        // Keep all original properties with the same names
        public List<Calculations1> CalculationsData { get; set; }

        public double exportGasSum, flaredGasSum, hpGasFlowSum, compGasRateSum, compFuelGasSum;
        public double exportGasK, flaredGasK, hpGasFlowK, compGasRateK, compFuelGasK;
        public double QgInj, QgTotal, TotalBurun, Reservoir;
        public double fuel = 2210.379;
        public double ResGasProductionGSSum = 0.0;

        // Volumes Before Allocation
        public double TotalInjectionVolume { get; private set; }
        public double TotalGasProductionVolume { get; private set; }
        public double ReservoirGasProductionVolume { get; private set; }
        public double TOTGasProdLPForTOTGasProdVolume { get; private set; }
        public double TOTGasProdLPForResGasProdVolume { get; private set; }
        public double TOTGasProdGLWForTOTGasProdVolume { get; private set; }
        public double TOTGasProdGLWForResGasProdVolume { get; private set; }

        public List<double> injVolumeList = new List<double>();
        public List<double> totGasProdVolumeList = new List<double>();
        public List<double> resGasProdVolumeList = new List<double>();

        // First Difference
        public double GasInjFirstDiff = 0.0, TOTGasProdLPFirstDiff = 0.0, ResGasProdFirstDiff = 0.0, GSWFirstDiff = 0.0;
        public double AssociatedGas = 0.0, gasInj = 0.0;
        public double AssociatedGasTOTGasDifference = 0.0, AssociatedGasTOTGasDiv = 0.0;

        // Volumes After Allocation
        public double RGPVAASum { get; private set; } // Res Gas Prod Volume After Allocation
        public List<double> RGPVAAList = new List<double>();

        public double TGPVAASum { get; private set; } // TOT Gas Prod Volume After Allocation
        public List<double> TGPVAAList = new List<double>();

        public double IVAASum { get; private set; } // Inj Volume After Allocation
        public List<double> IVAAList = new List<double>();

        public double ResGasNF { get; private set; }

        public double TOTGasProdGLW { get; private set; }
        public double TOTGasProdGLWDiff { get; private set; } // TOTGasProdGLW - TOTGasProdGLWForTOTGasProdVolume
        public double TOTGasProdGLWDiv { get; private set; } // TOTGasProdGLW / TOTGasProdGLWForTOTGasProdVolume

        public double RGPVAASumIfGS { get; private set; }
        public double TGPVAASumIfGS { get; private set; }
        public double TGPVAASumIfGLW { get; private set; }
        public double TOTGasProdGLWDiffIfGLW { get; private set; } // TOTGasProdGLW - TGPVAASumIfGLW

        public double RGPVAAPlainSumDiff { get; private set; } // RGPVAASum - RGPVAASumIfGS
        public double TGPVAAPlainSumDiff { get; private set; } // TGPVAASum - TGPVAAPlainSumDiff

        // Final Check
        public double GasInjFinalCheck { get; private set; }
        public double TotalBurunFinalCheck { get; private set; }
        public double ReservoirFinalCheck { get; private set; }
        public double hpGasFlowKFinalCheck { get; private set; }

        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public async Task OnGetAsync(string startDate = null, string endDate = null)
        {
            try
            {
                // Set default dates if not provided
                (StartDate, EndDate) = _calculationService.GetDateRange(startDate, endDate);

                // Query the database with the date range
                CalculationsData = await _calculationsRepository.GetCalculationsForDateRangeAsync(StartDate, EndDate);

                if (CalculationsData == null || !CalculationsData.Any())
                {
                    Message = "No data available for the selected date range. Click 'Refresh Data' to populate from the database.";
                    IsSuccess = false;
                    return;
                }

                // Perform calculations
                CalculationResult result = _calculationService.PerformCalculations(CalculationsData);
                UpdateModelWithResults(result);

                // Load accepted parameters
                await LoadAcceptedParametersAsync();
            }
            catch (Exception ex)
            {
                Message = $"Error loading data: {ex.Message}";
                IsSuccess = false;
            }
        }

        public class RefreshDataModel
        {
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }

        public async Task<IActionResult> OnPostRefreshDataAsync([FromBody] RefreshDataModel model)
        {
            try
            {
                // Parse and validate dates
                if (!_calculationService.TryParseDates(model.StartDate, model.EndDate, out DateTime startDate, out DateTime endDate))
                {
                    return new JsonResult(new { success = false, message = "Invalid date format" });
                }

                // Refresh data
                var refreshResult = await _calculationsRepository.RefreshCalculationsDataAsync(startDate, endDate);

                //await OnGetGetAcceptedParametersAsync();

                if (refreshResult.Success)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = $"Successfully processed {refreshResult.ProcessedCount} days of data."
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = refreshResult.Message });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<JsonResult> OnGetGetAcceptedParametersAsync()
        {
            try
            {
                var acceptedParametersList = await _calculationsRepository.GetAcceptedParametersAsync();

                if (acceptedParametersList == null || !acceptedParametersList.Any())
                {
                    return new JsonResult(null);
                }

                // Calculate all necessary values using the service
                var allocationResult = _calculationService.CalculateAllocations(
                    acceptedParametersList,
                    new AllocationInputData
                    {
                        TotalBurun = TotalBurun,
                        Reservoir = Reservoir,
                        hpGasFlowK = hpGasFlowK,
                        hpGasFlowSum = hpGasFlowSum,
                        compGasRateSum = compGasRateSum
                    });

                // Update the model with calculated values
                UpdateModelWithAllocations(allocationResult);

                // Return the filtered list of matching records
                return new JsonResult(acceptedParametersList);
            }
            catch (Exception ex)
            {
                // Log the error but don't expose details to client
                return new JsonResult(new { error = "An error occurred while retrieving parameters" });
            }
        }

        private async Task LoadAcceptedParametersAsync()
        {
            try
            {
                var acceptedParametersList = await _calculationsRepository.GetAcceptedParametersAsync();
                if (acceptedParametersList?.Any() == true)
                {
                    // Calculate all allocation values
                    var allocationResult = _calculationService.CalculateAllocations(
                        acceptedParametersList,
                        new AllocationInputData
                        {
                            TotalBurun = TotalBurun,
                            Reservoir = Reservoir,
                            hpGasFlowK = hpGasFlowK,
                            hpGasFlowSum = hpGasFlowSum,
                            compGasRateSum = compGasRateSum
                        });

                    // Update the model with calculated values
                    UpdateModelWithAllocations(allocationResult);
                }
            }
            catch (Exception ex)
            {
                // Log the exception but continue without throwing
                Message = $"Warning: Could not load allocation data. {ex.Message}";
            }
        }

        private void UpdateModelWithResults(CalculationResult result)
        {
            // Update all the sum and derived calculation fields
            exportGasSum = result.ExportGasSum;
            flaredGasSum = result.FlaredGasSum;
            hpGasFlowSum = result.HpGasFlowSum;
            compGasRateSum = result.CompGasRateSum;
            compFuelGasSum = result.CompFuelGasSum;

            exportGasK = result.ExportGasK;
            flaredGasK = result.FlaredGasK;
            hpGasFlowK = result.HpGasFlowK;
            compGasRateK = result.CompGasRateK;
            compFuelGasK = result.CompFuelGasK;

            QgInj = result.QgInj;
            QgTotal = result.QgTotal;
            TotalBurun = result.TotalBurun;
            Reservoir = result.Reservoir;
        }

        private void UpdateModelWithAllocations(AllocationResult result)
        {
            // Volumes Before Allocation
            TotalInjectionVolume = result.TotalInjectionVolume;
            TotalGasProductionVolume = result.TotalGasProductionVolume;
            ReservoirGasProductionVolume = result.ReservoirGasProductionVolume;
            TOTGasProdLPForTOTGasProdVolume = result.TOTGasProdLPForTOTGasProdVolume;
            TOTGasProdLPForResGasProdVolume = result.TOTGasProdLPForResGasProdVolume;
            TOTGasProdGLWForTOTGasProdVolume = result.TOTGasProdGLWForTOTGasProdVolume;
            TOTGasProdGLWForResGasProdVolume = result.TOTGasProdGLWForResGasProdVolume;
            ResGasProductionGSSum = result.ResGasProductionGSSum;

            // Volume Lists
            injVolumeList = result.InjVolumeList;
            totGasProdVolumeList = result.TotGasProdVolumeList;
            resGasProdVolumeList = result.ResGasProdVolumeList;

            // First Difference
            gasInj = result.GasInj;
            GasInjFirstDiff = result.GasInjFirstDiff;
            TOTGasProdLPFirstDiff = result.TOTGasProdLPFirstDiff;
            ResGasProdFirstDiff = result.ResGasProdFirstDiff;
            GSWFirstDiff = result.GSWFirstDiff;
            AssociatedGas = result.AssociatedGas;
            AssociatedGasTOTGasDifference = result.AssociatedGasTOTGasDifference;
            AssociatedGasTOTGasDiv = result.AssociatedGasTOTGasDiv;

            // Volumes After Allocation
            RGPVAASum = result.RGPVAASum;
            RGPVAAList = result.RGPVAAList;
            TGPVAASum = result.TGPVAASum;
            TGPVAAList = result.TGPVAAList;
            IVAASum = result.IVAASum;
            IVAAList = result.IVAAList;
            ResGasNF = result.ResGasNF;
            TOTGasProdGLW = result.TOTGasProdGLW;
            TOTGasProdGLWDiff = result.TOTGasProdGLWDiff;
            TOTGasProdGLWDiv = result.TOTGasProdGLWDiv;
            RGPVAASumIfGS = result.RGPVAASumIfGS;
            TGPVAASumIfGS = result.TGPVAASumIfGS;
            TGPVAASumIfGLW = result.TGPVAASumIfGLW;
            TOTGasProdGLWDiffIfGLW = result.TOTGasProdGLWDiffIfGLW;
            RGPVAAPlainSumDiff = result.RGPVAAPlainSumDiff;
            TGPVAAPlainSumDiff = result.TGPVAAPlainSumDiff;

            // Final Check
            GasInjFinalCheck = result.GasInjFinalCheck;
            TotalBurunFinalCheck = result.TotalBurunFinalCheck;
            ReservoirFinalCheck = result.ReservoirFinalCheck;
            hpGasFlowKFinalCheck = result.hpGasFlowKFinalCheck;
        }
    }
}
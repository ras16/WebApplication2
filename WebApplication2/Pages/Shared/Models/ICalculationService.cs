namespace WebApplication2.Pages.Shared.Models
{
    /// <summary>
    /// Service interface for gas production calculations
    /// </summary>
    public interface ICalculationService
    {
        /// <summary>
        /// Determines the date range for calculations
        /// </summary>
        (DateTime startDate, DateTime endDate) GetDateRange(string startDateStr, string endDateStr);

        /// <summary>
        /// Tries to parse start and end dates from string inputs
        /// </summary>
        bool TryParseDates(string startDateStr, string endDateStr, out DateTime startDate, out DateTime endDate);

        /// <summary>
        /// Performs the main calculation operations on the data
        /// </summary>
        CalculationResult PerformCalculations(List<Calculations1> calculationsData);

        /// <summary>
        /// Calculates all allocation-related values
        /// </summary>
        AllocationResult CalculateAllocations(IEnumerable<dynamic> acceptedParametersList, AllocationInputData inputData);
    }

     /// <summary>
    /// Class to hold all calculation results
    /// </summary>
    public class CalculationResult
    {
        // Sum calculations
        public double ExportGasSum { get; set; }
        public double FlaredGasSum { get; set; }
        public double HpGasFlowSum { get; set; }
        public double CompGasRateSum { get; set; }
        public double CompFuelGasSum { get; set; }

        // Derived calculations
        public double ExportGasK { get; set; }
        public double FlaredGasK { get; set; }
        public double HpGasFlowK { get; set; }
        public double CompGasRateK { get; set; }
        public double CompFuelGasK { get; set; }

        // Other calculations
        public double QgInj { get; set; }
        public double QgTotal { get; set; }
        public double TotalBurun { get; set; }
        public double Reservoir { get; set; }
    }

    /// <summary>
    /// Input data required for allocation calculations
    /// </summary>
    public class AllocationInputData
    {
        public double TotalBurun { get; set; }
        public double Reservoir { get; set; }
        public double hpGasFlowK { get; set; }
        public double hpGasFlowSum { get; set; }
        public double compGasRateSum { get; set; }
    }

    /// <summary>
    /// Class to hold all allocation calculation results
    /// </summary>
    public class AllocationResult
    {
        // Volumes Before Allocation
        public double TotalInjectionVolume { get; set; }
        public double TotalGasProductionVolume { get; set; }
        public double ReservoirGasProductionVolume { get; set; }
        public double TOTGasProdLPForTOTGasProdVolume { get; set; }
        public double TOTGasProdLPForResGasProdVolume { get; set; }
        public double TOTGasProdGLWForTOTGasProdVolume { get; set; }
        public double TOTGasProdGLWForResGasProdVolume { get; set; }
        public double ResGasProductionGSSum { get; set; }

        // Volume Lists
        public List<double> InjVolumeList { get; set; } = new List<double>();
        public List<double> TotGasProdVolumeList { get; set; } = new List<double>();
        public List<double> ResGasProdVolumeList { get; set; } = new List<double>();

        // First Difference
        public double GasInj { get; set; }
        public double GasInjFirstDiff { get; set; }
        public double TOTGasProdLPFirstDiff { get; set; }
        public double ResGasProdFirstDiff { get; set; }
        public double GSWFirstDiff { get; set; }
        public double AssociatedGas { get; set; }
        public double AssociatedGasTOTGasDifference { get; set; }
        public double AssociatedGasTOTGasDiv { get; set; }

        // Volumes After Allocation
        public double RGPVAASum { get; set; }
        public List<double> RGPVAAList { get; set; } = new List<double>();
        public double TGPVAASum { get; set; }
        public List<double> TGPVAAList { get; set; } = new List<double>();
        public double IVAASum { get; set; }
        public List<double> IVAAList { get; set; } = new List<double>();
        public double ResGasNF { get; set; }
        public double TOTGasProdGLW { get; set; }
        public double TOTGasProdGLWDiff { get; set; }
        public double TOTGasProdGLWDiv { get; set; }
        public double RGPVAASumIfGS { get; set; }
        public double TGPVAASumIfGS { get; set; }
        public double TGPVAASumIfGLW { get; set; }
        public double TOTGasProdGLWDiffIfGLW { get; set; }
        public double RGPVAAPlainSumDiff { get; set; }
        public double TGPVAAPlainSumDiff { get; set; }

        // Final Check
        public double GasInjFinalCheck { get; set; }
        public double TotalBurunFinalCheck { get; set; }
        public double ReservoirFinalCheck { get; set; }
        public double hpGasFlowKFinalCheck { get; set; }
    }

    /// <summary>
    /// Implementation of calculation service
    /// </summary>
    public class CalculationService : ICalculationService
    {
        private const double DefaultFuel = 2210.379;

        public (DateTime startDate, DateTime endDate) GetDateRange(string startDateStr, string endDateStr)
        {
            DateTime startDate;
            // Set default dates if not provided
            if (string.IsNullOrEmpty(startDateStr) || !DateTime.TryParse(startDateStr, out startDate))
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            DateTime endDate;
            if (string.IsNullOrEmpty(endDateStr) || !DateTime.TryParse(endDateStr, out endDate))
            {
                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            }

            return (startDate, endDate);
        }

        public bool TryParseDates(string startDateStr, string endDateStr, out DateTime startDate, out DateTime endDate)
        {
            bool startDateValid = DateTime.TryParse(startDateStr, out startDate);
            bool endDateValid = DateTime.TryParse(endDateStr, out endDate);

            if (!startDateValid || !endDateValid)
            {
                return false;
            }

            // Validate date range
            if (startDate > endDate)
            {
                return false;
            }

            return true;
        }

        public CalculationResult PerformCalculations(List<Calculations1> calculationsData)
        {
            var result = new CalculationResult();

            // Sum calculations
            result.ExportGasSum = calculationsData.Sum(x => x.ExportGas ?? 0);
            result.FlaredGasSum = calculationsData.Sum(x => x.FlaredGas ?? 0);
            result.HpGasFlowSum = calculationsData.Sum(x => x.HPGasFlow ?? 0);
            result.CompGasRateSum = calculationsData.Sum(x => x.CompressorGasRate ?? 0);
            result.CompFuelGasSum = calculationsData.Sum(x => x.CompFuelGas ?? 0);

            // Derived calculations
            result.ExportGasK = result.ExportGasSum / 1000;
            result.FlaredGasK = result.FlaredGasSum / 1000;
            result.HpGasFlowK = result.HpGasFlowSum / 1000;
            result.CompGasRateK = result.CompGasRateSum / 1000;
            result.CompFuelGasK = result.CompFuelGasSum / 1000;

            result.QgInj = result.HpGasFlowK + result.CompGasRateK;
            result.QgTotal = result.ExportGasK + result.FlaredGasK + result.CompGasRateK + DefaultFuel;
            result.TotalBurun = result.QgTotal;
            result.Reservoir = result.TotalBurun - result.CompGasRateK;

            return result;
        }

        public AllocationResult CalculateAllocations(IEnumerable<dynamic> acceptedParametersList, AllocationInputData inputData)
        {
            var result = new AllocationResult();

            CalculateVolumesBeforeAllocation(acceptedParametersList, result);
            CalculateFirstDifference(inputData, result);
            CalculateResGasProdVolumesAfterAllocation(acceptedParametersList, result, inputData);
            CalculateVolumesAfterAllocation(acceptedParametersList, result);
            CalculateFinalCheck(inputData, result);

            return result;
        }

        private void CalculateVolumesBeforeAllocation(IEnumerable<dynamic> acceptedParametersList, AllocationResult result)
        {
            // Initialize values
            result.TotalInjectionVolume = 0;
            result.TotalGasProductionVolume = 0;
            double TOTGasProductionGSSum = 0; // Track gas production sum for "GS" well types

            // Calculate injection and production volumes
            foreach (var param in acceptedParametersList)
            {
                double injVolume = 0.0;
                double gasProdVolume = 0.0;
                double gasProdInjVolDiff = 0.0;

                if (param.daysOn > 0)
                {
                    double injectionVolume = param.daysOn * param.avg_qg_inj;
                    injVolume = injectionVolume;
                    result.InjVolumeList.Add(injectionVolume);
                    result.TotalInjectionVolume += injectionVolume;
                }

                // For all wells with gas production
                if (param.daysOn > 0)
                {
                    double gasProductionVolume = param.daysOn * param.qg_tot;
                    gasProdVolume = gasProductionVolume;
                    result.TotGasProdVolumeList.Add(gasProductionVolume);
                    result.TotalGasProductionVolume += gasProductionVolume;

                    // Track gas production for "GS" well types separately
                    if (param.well_type?.ToString().ToUpper() == "GS")
                    {
                        TOTGasProductionGSSum += gasProdVolume;
                    }
                    if (param.well_type?.ToString().ToUpper() == "GLW")
                    {
                        result.TOTGasProdGLWForTOTGasProdVolume += gasProdVolume;
                    }
                }

                // Calculate reservoir gas production (Net gas production)
                gasProdInjVolDiff = gasProdVolume - injVolume;
                result.ResGasProdVolumeList.Add(gasProdInjVolDiff);
                result.ReservoirGasProductionVolume += gasProdInjVolDiff;

                if (param.well_type?.ToString().ToUpper() == "GS")
                {
                    result.ResGasProductionGSSum += gasProdInjVolDiff;
                }
                if (param.well_type?.ToString().ToUpper() == "GLW")
                {
                    result.TOTGasProdGLWForResGasProdVolume += gasProdInjVolDiff;
                }
            }

            // Calculate TOTGasProdLP as the difference between total gas production and GS well gas production
            result.TOTGasProdLPForTOTGasProdVolume = result.TotalGasProductionVolume - TOTGasProductionGSSum;
            result.TOTGasProdLPForResGasProdVolume = result.ReservoirGasProductionVolume - result.ResGasProductionGSSum;
        }

        private void CalculateFirstDifference(AllocationInputData inputData, AllocationResult result)
        {
            // Volumes to be matched (Mm3)
            /// TOT Gas Prod LP = TotalBurun
            /// Res Gas Prod = Reservoir
            /// GSW = hpGasFlowK
            result.GasInj = (inputData.hpGasFlowSum + inputData.compGasRateSum) / 1000;

            // Calculate First Difference
            result.GasInjFirstDiff = result.GasInj - result.TotalInjectionVolume;
            result.TOTGasProdLPFirstDiff = inputData.TotalBurun - result.TOTGasProdLPForTOTGasProdVolume;
            result.ResGasProdFirstDiff = inputData.Reservoir - result.ReservoirGasProductionVolume;
            result.GSWFirstDiff = inputData.hpGasFlowK - result.ResGasProductionGSSum;

            // Calculate Associated Gas
            result.AssociatedGas = inputData.Reservoir - inputData.hpGasFlowK;

            // Calculate AssociatedGasTOTGasDifference
            result.AssociatedGasTOTGasDifference = result.AssociatedGas - result.TOTGasProdLPForResGasProdVolume;

            // Calculate Matching Coefficients
            result.AssociatedGasTOTGasDiv = result.AssociatedGas / result.TOTGasProdLPForResGasProdVolume;
        }

        private void CalculateResGasProdVolumesAfterAllocation(IEnumerable<dynamic> parametersList, AllocationResult result, AllocationInputData inputData)
        {
            int i = 0;
            foreach (var param in parametersList)
            {
                if (param.well_type?.ToString().ToUpper() == "GS")
                {
                    result.RGPVAAList.Add(result.ResGasProdVolumeList[i]);
                    result.RGPVAASumIfGS += result.ResGasProdVolumeList[i];
                }
                else
                {
                    result.RGPVAAList.Add(result.ResGasProdVolumeList[i] * result.AssociatedGasTOTGasDiv);
                    if (param.well_type?.ToString().ToUpper() == "NF")
                    {
                        result.ResGasNF += result.RGPVAAList[i];
                    }
                }
                result.RGPVAASum += result.RGPVAAList[i];
                i++;
            }

            result.TOTGasProdGLW = inputData.TotalBurun - result.ResGasNF;
            result.TOTGasProdGLWDiff = result.TOTGasProdGLW - result.TOTGasProdGLWForTOTGasProdVolume;
            result.TOTGasProdGLWDiv = result.TOTGasProdGLW / result.TOTGasProdGLWForTOTGasProdVolume;
        }

        private void CalculateVolumesAfterAllocation(IEnumerable<dynamic> parametersList, AllocationResult result)
        {
            int i = 0;
            foreach (var param in parametersList)
            {
                if (param.well_type?.ToString().ToUpper() == "GS")
                {
                    result.TGPVAAList.Add(result.RGPVAAList[i]);
                    result.TGPVAASumIfGS += result.RGPVAAList[i];
                }
                else
                {
                    if (param.well_type?.ToString().ToUpper() == "NF")
                    {
                        result.TGPVAAList.Add(result.RGPVAAList[i]);
                    }
                    else
                    {
                        result.TGPVAAList.Add(result.TotGasProdVolumeList[i] * result.TOTGasProdGLWDiv);
                    }
                }

                if (param.well_type?.ToString().ToUpper() == "GLW")
                {
                    result.TGPVAASumIfGLW += result.TotGasProdVolumeList[i] * result.TOTGasProdGLWDiv;
                }

                result.TGPVAASum += result.TGPVAAList[i];

                result.IVAAList.Add(result.TGPVAAList[i] - result.RGPVAAList[i]);
                result.IVAASum += result.IVAAList[i];
                i++;
            }

            result.RGPVAAPlainSumDiff = result.RGPVAASum - result.RGPVAASumIfGS;
            result.TGPVAAPlainSumDiff = result.TGPVAASum - result.TGPVAASumIfGS;
            result.TOTGasProdGLWDiffIfGLW = result.TGPVAASumIfGLW - result.TOTGasProdGLW;
        }

        private void CalculateFinalCheck(AllocationInputData inputData, AllocationResult result)
        {
            result.GasInjFinalCheck = result.IVAASum - result.GasInj;
            result.TotalBurunFinalCheck = result.TGPVAAPlainSumDiff - inputData.TotalBurun;
            result.ReservoirFinalCheck = result.RGPVAASum - inputData.Reservoir;
            result.hpGasFlowKFinalCheck = result.TGPVAASumIfGS - inputData.hpGasFlowK;
        }
    }
}

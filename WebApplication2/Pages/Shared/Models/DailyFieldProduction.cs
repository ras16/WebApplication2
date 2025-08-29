namespace WebApplication2.Pages.Shared.Models
{
    //Production Datasets
    public class DailyFieldProduction
    {
        public required int Id { get; set; }
        public required int prod_area_id { get; set; }
        public DateTime Date { get; set; }
        public int? DayInMonth { get; set; }
        public double? SPPFluid { get; set; }
        public double? SPPOil { get; set; }
        public double? CorrFact { get; set; }
        public double? AvgBSW { get; set; }
        public double? AvgBSWSPP { get; set; }
        public double? VolRedFact { get; set; }
        public double? SPPFlTemp { get; set; }
        public double? OilDens { get; set; }
        public double? OilDelTNT { get; set; }
        public double? MthOilIPR { get; set; }
        public double? DailyGasIPR { get; set; }
        public double? AvailForLifting { get; set; }
        public double? LiftedToday { get; set; }
        public string? LiftPort { get; set; }
        public double? FlaredGas { get; set; }
        public double? ExportGas { get; set; }
        public double? SPPTotGas { get; set; }
        public double? NewOil { get; set; }
        public string? Comments { get; set; }
        public double? AvgVishkaOil { get; set; }
        public double? VishkaInletPress { get; set; }
        public double? LPWaterInj { get; set; }
        public double? HPWaterInj { get; set; }
        public double? WaterDisposed { get; set; }
    }
}
namespace WebApplication2.Pages.Shared.Models
{
    //Production Datasets
    public class FormData
    {
        public required int ID { get; set; }
        public required int prod_area_id { get; set; }
        public DateTime Date { get; set; }
        public int? DayInMonth { get; set; }
        public decimal? SPPFluid { get; set; }
        public decimal? SPPOil { get; set; }
        public decimal? CorrFact { get; set; }
        public decimal? AvgBSW { get; set; }
        public decimal? AvgBSWSPP { get; set; }
        public decimal? VolRedFact { get; set; }
        public decimal? SPPFlTemp { get; set; }
        public decimal? OilDens { get; set; }
        public decimal? OilDelTNT { get; set; }
        public decimal? MthOilIPR { get; set; }
        public decimal? DailyGasIPR { get; set; }
        public decimal? AvailForLifting { get; set; }
        public decimal? LiftedToday { get; set; }
        public string? LiftPort { get; set; }
        public decimal? FlaredGas { get; set; }
        public decimal? ExportGas { get; set; }
        public decimal? SPPTotGas { get; set; }
        public decimal? NewOil { get; set; }
        public string? Comments { get; set; }
        public decimal? AvgVishkaOil { get; set; }
        public decimal? VishkaInletPress { get; set; }
        public decimal? LPWaterInj { get; set; }
        public decimal? HPWaterInj { get; set; }
        public decimal? WaterDisposed { get; set; }
    }
}
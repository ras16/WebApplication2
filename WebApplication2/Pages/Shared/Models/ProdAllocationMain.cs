namespace WebApplication2.Pages.Shared.Models
{
    public class ProdAllocationMain
    {
        public required int PAID { get; set; }
        public required int prod_area_id { get; set; }
        public DateTime? Month { get; set; }
        public double? Days { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? MidMonth { get; set; }
        public double? OilMT { get; set; }
        public double? WaterMT { get; set; }
        public double? OilM3 { get; set; }
        public double? WaterM3 { get; set; }
        public double? Gas { get; set; }
        public double?   ACTGas { get; set; }
        public double? FlaredGas { get; set; }
        public double? VentedGas { get; set; }
        public double? burun_gas { get; set; }
        public double? OilSG { get; set; }
        public double? WaterSG { get; set; }
        public bool? Wells_Ind { get; set; }
        public double? Comp_Fuel_Gas { get; set; }
        public double? Oil_Mult { get; set; }
        public double? Water_Mult { get; set; }
        public double? LP_Gas_Mult { get; set; }
        public double? Re_Adjust_Gas_Mult { get; set; }
        public double? GLW_Adjust_Gas { get; set; }

        public AcceptedParameters AcceptedParameters { get; set; }
    }
}

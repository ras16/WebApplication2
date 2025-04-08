namespace WebApplication2.Pages.Shared.Models
{
    public class ProdAllocationMain
    {
        public required int PAID { get; set; }
        public required int ProdAreaId { get; set; }
        public DateTime Month { get; set; }
        public float Days { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime MidMonth { get; set; }
        public float OilMT { get; set; }
        public float WaterMT { get; set; }
        public float OilM3 { get; set; }
        public float WaterM3 { get; set; }
        public float Gas { get; set; }
        public float ACTGas { get; set; }
        public float FlaredGas { get; set; }
        public float VentedGas { get; set; }
        public float BurunGas { get; set; }
        public float OilSG { get; set; }
        public float WaterSG { get; set; }
        public bool WellsInd { get; set; }
        public float CompFuelGas { get; set; }
        public float OilMult { get; set; }
        public float WaterMult { get; set; }
        public float LPGasMult { get; set; }
        public float ReAdjustGasMult { get; set; }
        public float GLWAdjustGas { get; set; }
    }
}

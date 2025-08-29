namespace WebApplication2.Pages.Shared.Models
{
    public class ProductionReportViewModel
    {
        public string WellName { get; set; }
        public int PAID { get; set; }
        public string WellType { get; set; }
        public int DaysOn { get; set; }
        public string FlowingTo { get; set; }
        public string Choke { get; set; }
        public decimal THP { get; set; }
        public decimal Qo { get; set; }
        public decimal Qw { get; set; }
        public decimal OilMT { get; set; }
        public decimal WaterMT { get; set; }
        public decimal GasMm3 { get; set; }
    }
}

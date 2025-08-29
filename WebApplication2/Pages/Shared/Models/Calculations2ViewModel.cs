using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Shared.Models
{
    public class Calculations2ViewModel
    {
        [Key]
        public string? WellName { get; set; }
        public int? PAID { get; set; }
        public string? WellType { get; set; }
        public int? DaysOn { get; set; }
        public string? FlowingTo { get; set; }
        public double? Choke { get; set; }
        public double? THP { get; set; }
        public double? Qo { get; set; }
        public double? Qw { get; set; }
        public double? OilMT { get; set; }
        public double? WaterMT { get; set; }
        public double? GasMm3 { get; set; }
    }
}

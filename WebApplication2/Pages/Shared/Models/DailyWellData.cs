using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Shared.Models
{
    public class DailyWellData
    {

        [Key]
        public int ID { get; set; }
        public DateTime? Date { get; set; }
        public int? WellID { get; set; }
        public string? WellType { get; set; }
        public string? Horizon { get; set; }
        public string? Flow { get; set; }
        public decimal? Tbg_Choke_mm { get; set; }
        public decimal? Csg_Choke_mm { get; set; }
        public decimal? THP_barg { get; set; }
        public decimal? THT_C { get; set; }
        public decimal? CHP_barg { get; set; }
        public string? Flowing_To { get; set; }
        public decimal? Sep_press_bar { get; set; }
        public decimal? Sep_temp_C { get; set; }
        public decimal? Liq_rate_m3d { get; set; }
        public double? BSW_pcnt { get; set; }
        public decimal? Fl_Line_Pres_barg { get; set; }
        public double? Liquid_lvl_csg_m { get; set; }
        public decimal? Pumping_Speed_spm { get; set; }
        public decimal? Stroke_Length_m { get; set; }
        public double? Pump_Depth_m { get; set; }
        public decimal? Vol_Inj_m3 { get; set; }
        public decimal? Hrs_Inj_hrs { get; set; }
        public decimal? Gas_rate_mm_m3d { get; set; }
        public decimal? Specific_gravity_oil { get; set; }
        public decimal? Specific_gravity_gas { get; set; }
        public string? HOWC { get; set; }
        public decimal? GOR { get; set; }
        public string? Field1 { get; set; }
        public double? DownTime { get; set; }
        public string? Remarks { get; set; }
    }
}

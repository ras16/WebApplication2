namespace WebApplication2.Pages.Shared.Models
{
    public class HP_Gas
    {
        public int ID { get; set; }
        public int WellID { get; set; }
        public DateTime Date { get; set; }
        public double? Flow_rate_m3_per_day { get; set; } //Qo m3/d another name for it 
        public double? Tbg_chk_mm { get; set; }
        public double? Fthp_bar { get; set; }
        public double? Pipe_id_mm { get; set;}
        public double? Orifice_mm { get; set; }
        public double? Diff_p_in_H2O { get; set; }
        public double? Ftht_f { get; set; }
        public double? Down_time_hrs { get; set; }
        public double? Sep_liq_rate_m3_per_day { get; set; }
        public string? Comments { get; set; }

    }
}

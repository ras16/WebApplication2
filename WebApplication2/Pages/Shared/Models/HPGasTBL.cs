namespace WebApplication2.Pages.Shared.Models
{
    public class HPGasTBL
    {
        public int ID { get; set; }
        public int WellID { get; set; }
        public DateTime date { get; set; }
        public double? flow_rate_m3_per_day { get; set; } //Qo m3/d another name for it 
        public double? tbg_chk_mm { get; set; }
        public double? fthp_bar { get; set; }
        public double? pipe_id_mm { get; set;}
        public double? orifice_mm { get; set; }
        public double? diff_p_in_H2O { get; set; }
        public double? ftht_f { get; set; }
        public double? down_time_hrs { get; set; }
        public double? Sep_liq_rate_m3_per_day { get; set; }
        public string? Comments { get; set; }

    }
}

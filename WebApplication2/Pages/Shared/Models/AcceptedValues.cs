namespace WebApplication2.Pages.Shared.Models
{
    public class AcceptedValues
    {
            //Accepted Datasets
            public required int id { get; set; }
            public required int PAID { get; set; }
            public DateTime? test_date { get; set; }
            public int well_id { get; set; }
            public string? prod_int { get; set; }
            public string? test_type { get; set; }
            public string? flow_type { get; set; }
            public double? hours_tested { get; set; }
            public double? avg_tbg_choke_64 { get; set; }
            public double? avg_csg_choke_64 { get; set; }
            public double? avg_thp_barg { get; set; }
            public double? avg_tht_f { get; set; }
            public double? avg_chp_barg { get; set; }
            public double? avg_oil_m3_per_day { get; set; }
            public required double? avg_water_m3_per_day { get; set; }
            public required double? avg_gas_10_6_m3_per_day { get; set; }
            public required double? gor { get; set; }
            public required double? avg_inj_gas_rate { get; set; }
            public required double? oil_sg { get; set; }
            public required double? gas_sg { get; set; }
            public required double? oil_mt_per_day { get; set; }
            public required double? nacl_ppm { get; set; }
            public required double? bsw { get; set; }
            public required string? test_company { get; set; }
            public required string? rig_kit { get; set; }
            public required string? entered_by { get; set; }
            public required int? no_prod { get; set; }
            public required string? comments { get; set; }
            public required double? interval_top { get; set; }
            public required double? interval_bottom { get; set; }
            public required int? representative_ind { get; set; }
    }
}

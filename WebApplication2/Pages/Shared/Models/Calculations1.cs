    using System.ComponentModel.DataAnnotations;

    namespace WebApplication2.Pages.Shared.Models
    {
        public class Calculations1
        {
            [Key]
            public int Id { get; set; }
            public DateTime CalcDate { get; set; }
            public double? ExportGas { get; set; }
            public double? FlaredGas { get; set; }
            public double? HPGasFlow { get; set; }
            public double? CompressorGasRate { get; set; }
            public double? CompFuelGas { get; set; }
        }
    }

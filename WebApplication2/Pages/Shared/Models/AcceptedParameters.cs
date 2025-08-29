using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Shared.Models
{
    public class AcceptedParameters
    {
        [Key]
        public int ID { get; set; }

        public int PAID { get; set; }

        public int WellID { get; set; }

        [StringLength(50)]
        public string? WellType { get; set; }

        [Column(TypeName = "numeric(18, 0)")]
        public int? DaysOn { get; set; }

        [StringLength(50)]
        public string? FlowingTo { get; set; }

        public double? Choke { get; set; }

        public double? THP { get; set; }

        public double? Qo { get; set; }

        public double? GOR { get; set; }

        public double? AvgQgInj { get; set; }

        public double? BSW { get; set; }

        public double? SG { get; set; }

        public double? TGOR { get; set; }

        public double? AddQo { get; set; }

        public double? AddBSW { get; set; }

        public double? AddGOR { get; set; }

        public double? Qg { get; set; }

        public double? CHP { get; set; }

        public double? CsgChk { get; set; }

        public DateTime? TestDate { get; set; }

        public Wells Wells { get; set; }
        public ProdAllocationMain ProdAllocationMain { get; set; }
    }
}

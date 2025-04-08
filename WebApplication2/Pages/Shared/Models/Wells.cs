namespace WebApplication2.Pages.Shared.Models
{
    public class Wells
    {
        //Wells datasets 
        public  int WellID { get; set; }
        public  string? WellName { get; set; }
        public  DateTime? DateDrilled { get; set; }
        public  double? TotalDepth { get; set; }
        public  int? Prod_Area_ID { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public  DateTime? spud_date { get; set; }
        public  DateTime? completion_date { get; set; }
        public  double? kb_lev { get; set; }
        public  double? ground_lev { get; set; }

    }
}

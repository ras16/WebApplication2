using Microsoft.AspNetCore.Routing.Constraints;

namespace WebApplication2.Pages.Shared.Models
{
    public class FlayerAloc
    {
        //FlayerAloc datasets 
        public int id { get; set; }
        public string? Name { get; set; }
        public int wellid { get; set; }
        public DateTime? date { get; set; }
        public string layer { get; set; }
        public double oil { get; set; }
        public double water { get; set; }
        public double gas { get; set; }
    }
}

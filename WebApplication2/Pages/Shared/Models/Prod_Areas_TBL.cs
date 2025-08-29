using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Shared.Models
{
    public class Prod_Areas_TBL
    {
        //ProdAreasTBL datasets
        [Key]   
        public int prod_area_id { get; set; }
        public string Prod_Area { get; set; }
        public Boolean Exploration { get; set; }
        public int? vishka_tank { get; set; }
        public int? Prod_Image { get; set; }

    }
}

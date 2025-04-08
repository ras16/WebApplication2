using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Shared.Models
{
    public class Compressors
    {

        //Compressor Datasets 
        [Key] //Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //Auto-generated ID
        public int id { get; set; }
        public string CompressorName { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateStop{ get; set; }
      
    }
}

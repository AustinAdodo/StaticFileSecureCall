using System.ComponentModel.DataAnnotations;

namespace StaticFileSecureCall.Models
{
    public class FileRepository
    {
        public int id { get; set; }

        [Required]
        public string Filename { get; set; }

        [Required]
        public string Address { get; set; }    
    }
}

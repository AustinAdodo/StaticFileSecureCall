using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace StaticFileSecureCall.Models
{
    public class FileRepository
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string Filename { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string internalId { get; set; }
    }
}

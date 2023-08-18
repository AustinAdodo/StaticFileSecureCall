using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using StaticFileSecureCall.Validation;

namespace StaticFileSecureCall.Models
{
    /// <summary>
    ///http://go.microsoft.com/fwlink/?LinkId=287068
    /// </summary>
    public class FileRepository
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string Filename { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string InternalId { get; set; }

        [Required]
        public string Reference { get; set; }
    }
}

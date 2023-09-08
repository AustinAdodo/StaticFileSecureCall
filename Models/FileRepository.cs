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
        public string Filename { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; }= string.Empty;
        [Required] public string InternalId { get; set; } = string.Empty;

        [Required] public string Reference { get; set; } = string.Empty;
    }
}

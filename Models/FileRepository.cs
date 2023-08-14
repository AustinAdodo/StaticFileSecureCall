using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using StaticFileSecureCall.Validation;

namespace StaticFileSecureCall.Models
{
    [UniqueFields(fieldNames: FieldNames.Names)]
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

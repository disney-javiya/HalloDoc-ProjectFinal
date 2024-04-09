using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class businessCreateRequest
    {
        [Required]
        [StringLength(100)]
        public string? BusinessFirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string? BusinessLastName { get; set; }

        [StringLength(23)]
        public string? BusinessPhoneNumber { get; set; } = null;
        [Required]
        [StringLength(50)]
        public string? BusinessEmail { get; set; }
        [Required]
        [StringLength(100)]
        public string? BusinessPropertyName { get; set; }


        [StringLength(500)]
        public string? Notes { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string? LastName { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        [StringLength(50)]
        public string? Email { get; set; }

        [StringLength(23)]
        public string? PhoneNumber { get; set; } = null;

        [Required]
        [StringLength(100)]
        public string? Street { get; set; }
        [Required]
        [StringLength(100)]
        public string? City { get; set; }
        [Required]
        [StringLength(100)]
        public string? State { get; set; }
        [Required]
        [StringLength(10)]
        public string? ZipCode { get; set; }
    }
}

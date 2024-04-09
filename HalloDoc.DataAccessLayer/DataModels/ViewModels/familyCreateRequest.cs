using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class familyCreateRequest
    {
        [Required]
        [StringLength(100)]
        public string? FamilyFirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string? FamilyLastName { get; set; }

        [StringLength(23)]
        public string? FamilyPhoneNumber { get; set; } = null;
        [Required]
        [StringLength(50)]
        public string? FamilyEmail { get; set; }
        [Required]
        [StringLength(100)]
        public string? RelationName { get; set; }

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
        public IFormFile[]? MultipleFiles { get; set; }
    }
}

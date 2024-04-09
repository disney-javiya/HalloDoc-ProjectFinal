using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class patientInfo
    {
        [StringLength(250)]
        public string Symptoms { get; set; } = null!;
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Column(TypeName = "character varying")]
        [Required]
        public string? PasswordHash { get; set; }
        [Required]
        [StringLength(50)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? Mobile { get; set; }

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

        public int RequestTypeId { get; set; }
        public IFormFile[]? MultipleFiles { get; set; }


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
        public string? ConciergeFirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string? ConciergeLastName { get; set; }

        [StringLength(23)]
        public string? ConciergePhoneNumber { get; set; } = null;
        [Required]
        [StringLength(50)]
        public string? ConciergeEmail { get; set; }
        [Required]
        [StringLength(100)]
        public string? ConciergePropertyName { get; set; }
        [Required]
        [StringLength(100)]
        public string? ConciergeStreet { get; set; }
        [Required]
        [StringLength(100)]
        public string? ConciergeCity { get; set; }
        [Required]
        [StringLength(100)]
        public string? ConciergeState { get; set; }
        [Required]
        [StringLength(10)]
        public string? ConciergeZipCode { get; set; }


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

    }
}

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
        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly DateOfBirth { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Column(TypeName = "character varying")]
        [Required(ErrorMessage = "Password is required")]
        public string? PasswordHash { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [StringLength(50)]
        public string Email { get; set; } = null!;
        [Required]
        [StringLength(20)]
        public string? Mobile { get; set; }
        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(23)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted.")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Street is required")]
        [StringLength(100)]
        public string? Street { get; set; }
        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string? City { get; set; }
        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string? State { get; set; }
        [Required(ErrorMessage = "Zipcode is required")]
        [StringLength(10)]
        public string? ZipCode { get; set; }

        public int RequestTypeId { get; set; }
        public IFormFile[]? MultipleFiles { get; set; }


        [Required(ErrorMessage = "Family First Name is required")]
        [StringLength(100)]
        public string? FamilyFirstName { get; set; }
        [Required(ErrorMessage = "Family Last Name is required")]
        [StringLength(100)]
        public string? FamilyLastName { get; set; }
        [Required(ErrorMessage = "Family Phone Number is required")]
        [StringLength(23)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted.")]
        public string? FamilyPhoneNumber { get; set; }
        [Required(ErrorMessage = "Family Email is required")]
        [StringLength(50)]
        public string? FamilyEmail { get; set; }
        [Required]
        [StringLength(100)]
        public string? RelationName { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }



        [Required(ErrorMessage = "Concierge First Name is required")]
        [StringLength(100)]
        public string? ConciergeFirstName { get; set; }
        [Required(ErrorMessage = "Concierge Last Name is required")]
        [StringLength(100)]
        public string? ConciergeLastName { get; set; }
        [Required(ErrorMessage = "Concierge Phone Number is required")]
        [StringLength(23)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted.")]
        public string? ConciergePhoneNumber { get; set; }
        [Required(ErrorMessage = "Concierge Email is required")]
        [StringLength(50)]
        public string? ConciergeEmail { get; set; }
        [Required(ErrorMessage = "Concierge Property Name is required")]
        [StringLength(100)]
        public string? ConciergePropertyName { get; set; }
        [Required(ErrorMessage = "Concierge Street is required")]
        [StringLength(100)]
        public string? ConciergeStreet { get; set; }
        [Required(ErrorMessage = "Concierge City is required")]
        [StringLength(100)]
        public string? ConciergeCity { get; set; }
        [Required(ErrorMessage = "Concierge State is required")]
        [StringLength(100)]
        public string? ConciergeState { get; set; }
        [Required(ErrorMessage = "Concierge Zipcode is required")]
        [StringLength(10)]
        public string? ConciergeZipCode { get; set; }


        [Required(ErrorMessage = "Business First Name is required")]
        [StringLength(100)]
        public string? BusinessFirstName { get; set; }
        [Required(ErrorMessage = "Business last Name is required")]
        [StringLength(100)]
        public string? BusinessLastName { get; set; }

        [Required(ErrorMessage = "Business Phone number is required")]
        [StringLength(23)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted.")]
        public string? BusinessPhoneNumber { get; set; }
        [Required(ErrorMessage = "Business Email is required")]
        [StringLength(50)]
        public string? BusinessEmail { get; set; }
        [Required(ErrorMessage = "Business Property Name is required")]
        [StringLength(100)]
        public string? BusinessPropertyName { get; set; }

    }
}

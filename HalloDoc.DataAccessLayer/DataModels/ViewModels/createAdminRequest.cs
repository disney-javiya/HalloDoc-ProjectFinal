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
    public class createAdminRequest
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
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&-+=()])(?=\\S+$).{8,20}$", ErrorMessage = "Password must be Strong")]
        public string? PasswordHash { get; set; }
        [Required]
        [StringLength(50)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        [RegularExpression("^([0]|\\+91)?[6789]\\d{9}$", ErrorMessage = "Enter Valid Mobile Number")]
        public string? Mobile { get; set; }
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

        [StringLength(100)]
        public string? AdditionalNotes { get; set; }
    }
}

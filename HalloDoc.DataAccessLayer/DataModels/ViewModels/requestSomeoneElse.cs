using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class requestSomeoneElse
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
        [Required]
        [StringLength(50)]
        public string Email { get; set; } = null!;
        [Required]
        [StringLength(20)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted.")]
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
       

        [StringLength(100)]
        public string? Relation { get; set; }

        public IFormFile[]? MultipleFiles { get; set; }
    }
}

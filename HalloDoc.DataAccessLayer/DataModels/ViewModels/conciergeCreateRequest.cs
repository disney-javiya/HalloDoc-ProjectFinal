using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class conciergeCreateRequest
    {
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
    }
}

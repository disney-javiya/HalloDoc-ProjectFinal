using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class sendOrder
    {
       
        public int Id { get; set; }

        public int? VendorId { get; set; }

        public int? RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string? FaxNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string? Email { get; set; }
        [Required]
        [StringLength(100)]
        public string? BusinessContact { get; set; }
        [Required]
        [Column(TypeName = "character varying")]
        public string? Prescription { get; set; }
 
        public int? NoOfRefill { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? CreatedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public int? hname { get; set; }
     
        public int? type { get; set; }
        public IEnumerable<HealthProfessionalType> HealthProfessionalType { get; set; }
        public IEnumerable<HealthProfessional> HealthProfessional { get; set; }
    }
}

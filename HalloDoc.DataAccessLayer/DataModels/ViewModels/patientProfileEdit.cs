using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class patientProfileEdit
    {

        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [StringLength(100)]
        public string? LastName { get; set; }

        [Column("strMonth")]
        [StringLength(20)]
        public string? StrMonth { get; set; }

        [Column("intYear")]
        public int? IntYear { get; set; }

        [Column("intDate")]
        public int? IntDate { get; set; }

        [StringLength(23)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Street { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class createRole
    {
        [Key]
        public int RoleId { get; set; }

        [StringLength(50)]
        public string Name { get; set; } = null!;

        public short AccountType { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; } = null!;




        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedDate { get; set; }

        [StringLength(128)]
        public string? ModifiedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }

        [Column(TypeName = "bit(1)")]
        public BitArray IsDeleted { get; set; } = null!;

        [Column("IP")]
        [StringLength(20)]
        public string? Ip { get; set; }
    }
}

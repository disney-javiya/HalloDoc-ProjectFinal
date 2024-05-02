using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class TimesheetModel
    {
        public int PhysicianId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Startdate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Enddate { get; set; }

        public string? Status { get; set; }

        [Column("isFinalized", TypeName = "bit(1)")]
        public BitArray? IsFinalized { get; set; }
    }
}

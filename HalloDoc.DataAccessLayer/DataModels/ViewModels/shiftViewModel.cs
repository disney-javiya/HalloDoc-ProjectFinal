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
    public class shiftViewModel
    {
        [Key]
        public int ShiftId { get; set; }

        public int PhysicianId { get; set; }

        public DateOnly StartDate { get; set; }

        [Column(TypeName = "bit(1)")]
        public BitArray IsRepeat { get; set; } = null!;

        [StringLength(7)]
        public string? WeekDays { get; set; }

        public int? RepeatUpto { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; } = null!;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedDate { get; set; }

        [Column("IP")]
        [StringLength(20)]
        public string? Ip { get; set; }
        [Key]
        public int ShiftDetailId { get; set; }


        [Column(TypeName = "timestamp without time zone")]
        public DateTime ShiftDate { get; set; }

        public int? RegionId { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public short Status { get; set; }

        [Column(TypeName = "bit(1)")]
        public BitArray IsDeleted { get; set; } = null!;

        [StringLength(128)]
        public string? ModifiedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? LastRunningDate { get; set; }

        [StringLength(100)]
        public string? EventId { get; set; }

        [Column(TypeName = "bit(1)")]
        public BitArray? IsSync { get; set; }
    }
}

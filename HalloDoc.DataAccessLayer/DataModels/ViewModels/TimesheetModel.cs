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
        //public DateOnly? Date { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Startdate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Enddate { get; set; }

        public string? Status { get; set; }

        [Column("isFinalized", TypeName = "bit(1)")]
        public BitArray? IsFinalized { get; set; }
        //[Column(TypeName = "timestamp without time zone")]
        //public DateTime? Shiftdate { get; set; }

        //public int? ShiftHours { get; set; }
        //public int? TotalHours { get; set; }

        //public int? Housecall { get; set; }

        //public int? PhoneConsult { get; set; }

        //[Column("isWeekend", TypeName = "bit(1)")]
        //public BitArray? IsWeekend { get; set; }

        public List<TimesheetDetail> Timesheets { get; set; }
        public List<ShiftDetail> ShiftDetail { get; set; }
    }
}

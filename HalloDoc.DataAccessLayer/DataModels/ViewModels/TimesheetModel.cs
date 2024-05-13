using Microsoft.AspNetCore.Http;
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
        public int InvoiceId { get; set; }
        //public DateOnly? Date { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Startdate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Enddate { get; set; }

        public string? Status { get; set; }

        [Column("isFinalized", TypeName = "bit(1)")]
        public BitArray? IsFinalized { get; set; }
        
        public List<TimesheetDetail> Timesheets { get; set; }
        public List<TimesheetDetail> physicians { get; set; }
        public List<TimesheetReimbursement> timesheetReimbursements { get; set; }
        public List<ShiftDetail> ShiftDetail { get; set; }

        public string Item { get; set; }
        public string FileName { get; set; }
        public IFormFile ReceiptFile { get; set; }
        public int Amount { get; set; }
        public int Gap { get; set; }


        public int? TotalHours { get; set; }

        //public bool? WeekendHoliday { get; set; }

        //public int? NoHousecalls { get; set; }

        //public int? NoHousecallsNight { get; set; }

        //public int? NoPhoneConsult { get; set; }

        //public int? NoPhoneConsultNight { get; set; }


        public int? NightshiftWeekend { get; set; }
        public int? TotalNightshiftWeekend { get; set; }
        public int? Shift { get; set; }
        public int? TotalShift { get; set; }
        public int? PhoneConsults { get; set; }
        public int? TotalPhoneConsults { get; set; }
        public int? Housecall { get; set; }
        public int? TotalHousecall { get; set; }
        public int? TotalInvoice { get; set; }
    }
}

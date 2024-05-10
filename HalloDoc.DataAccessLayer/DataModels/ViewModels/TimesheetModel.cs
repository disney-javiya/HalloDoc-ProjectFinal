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
    }
}

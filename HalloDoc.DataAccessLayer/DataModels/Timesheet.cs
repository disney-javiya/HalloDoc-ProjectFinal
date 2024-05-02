using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("Timesheet")]
public partial class Timesheet
{
    public int PhysicianId { get; set; }

    [Key]
    public int TimesheetId { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Startdate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Enddate { get; set; }

    public string? Status { get; set; }

    [Column("isFinalized", TypeName = "bit(1)")]
    public BitArray? IsFinalized { get; set; }

    [ForeignKey("PhysicianId")]
    [InverseProperty("Timesheets")]
    public virtual Physician Physician { get; set; } = null!;

    [InverseProperty("Timesheet")]
    public virtual ICollection<TimesheetDetail> TimesheetDetails { get; set; } = new List<TimesheetDetail>();

    [InverseProperty("Timesheet")]
    public virtual ICollection<TimesheetReimbursement> TimesheetReimbursements { get; set; } = new List<TimesheetReimbursement>();
}

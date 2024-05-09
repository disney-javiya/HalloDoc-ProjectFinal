using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("TimesheetReimbursement")]
public partial class TimesheetReimbursement
{
    [Key]
    public int TimesheetReimbursementId { get; set; }

    public int TimesheetId { get; set; }

    public string Item { get; set; } = null!;

    public int Amount { get; set; }

    public string? Filename { get; set; }

    public int? PhysicianId { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ReimbursementDate { get; set; }

    [Column(TypeName = "character varying")]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "character varying")]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("TimesheetId")]
    [InverseProperty("TimesheetReimbursements")]
    public virtual Timesheet Timesheet { get; set; } = null!;
}

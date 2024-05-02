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

    public string? Bill { get; set; }

    [ForeignKey("TimesheetId")]
    [InverseProperty("TimesheetReimbursements")]
    public virtual Timesheet Timesheet { get; set; } = null!;
}

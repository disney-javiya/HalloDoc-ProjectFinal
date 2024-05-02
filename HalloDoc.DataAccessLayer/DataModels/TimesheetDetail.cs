using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

public partial class TimesheetDetail
{
    [Key]
    public int TimesheetDetailId { get; set; }

    public int TimesheetId { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Shiftdate { get; set; }

    public int? ShiftHours { get; set; }

    public int? Housecall { get; set; }

    public int? PhoneConsult { get; set; }

    [Column("isWeekend", TypeName = "bit(1)")]
    public BitArray? IsWeekend { get; set; }

    [ForeignKey("TimesheetId")]
    [InverseProperty("TimesheetDetails")]
    public virtual Timesheet Timesheet { get; set; } = null!;
}

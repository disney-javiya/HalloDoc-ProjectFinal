using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("Payrate")]
public partial class Payrate
{
    public int PhysicianId { get; set; }

    [Key]
    public int PayrateId { get; set; }

    public int? NightShiftWeekend { get; set; }

    public int? Shift { get; set; }

    public int? HousecallNightWeekend { get; set; }

    public int? Phoneconsult { get; set; }

    public int? PhoneconsultNightWeekend { get; set; }

    public int? BatchTesting { get; set; }

    public int? Housecall { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("PhysicianId")]
    [InverseProperty("Payrates")]
    public virtual Physician Physician { get; set; } = null!;
}

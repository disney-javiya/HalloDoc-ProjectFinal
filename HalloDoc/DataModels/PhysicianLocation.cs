using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataModels;

[Keyless]
[Table("PhysicianLocation")]
public partial class PhysicianLocation
{
    public int LocationId { get; set; }

    public int PhysicianId { get; set; }

    [Precision(11, 8)]
    public decimal? Latitude { get; set; }

    [Precision(11, 8)]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [StringLength(50)]
    public string? PhysicianName { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [ForeignKey("PhysicianId")]
    public virtual Physician Physician { get; set; } = null!;
}

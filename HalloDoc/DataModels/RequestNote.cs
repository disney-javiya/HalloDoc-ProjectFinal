using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataModels;

public partial class RequestNote
{
    [Key]
    public int RequestNotesId { get; set; }

    public int RequestId { get; set; }

    [Column("strMonth")]
    [StringLength(20)]
    public string? StrMonth { get; set; }

    [Column("intYear")]
    public int? IntYear { get; set; }

    [Column("intDate")]
    public int? IntDate { get; set; }

    [StringLength(500)]
    public string? PhysicianNotes { get; set; }

    [StringLength(500)]
    public string? AdminNotes { get; set; }

    [StringLength(128)]
    public string CreatedBy { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [StringLength(128)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedDate { get; set; }

    [Column("IP")]
    [StringLength(20)]
    public string? Ip { get; set; }

    [StringLength(500)]
    public string? AdministrativeNotes { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("RequestNotes")]
    public virtual Request Request { get; set; } = null!;
}

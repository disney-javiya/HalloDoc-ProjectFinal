using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataModels;

[Table("Admin")]
public partial class Admin
{
    [Key]
    public int AdminId { get; set; }

    [StringLength(128)]
    public string AspNetUserId { get; set; } = null!;

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string? Mobile { get; set; }

    [StringLength(500)]
    public string? Address1 { get; set; }

    [StringLength(500)]
    public string? Address2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    public int? RegionId { get; set; }

    [StringLength(10)]
    public string? Zip { get; set; }

    [StringLength(20)]
    public string? AltPhone { get; set; }

    [StringLength(128)]
    public string CreatedBy { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [StringLength(128)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedDate { get; set; }

    public short? Status { get; set; }

    [Column(TypeName = "bit(1)")]
    public BitArray? IsDeleted { get; set; }

    public int? RoleId { get; set; }

    [InverseProperty("Admin")]
    public virtual ICollection<AdminRegion> AdminRegions { get; set; } = new List<AdminRegion>();

    [ForeignKey("AspNetUserId")]
    [InverseProperty("AdminAspNetUsers")]
    public virtual AspNetUser AspNetUser { get; set; } = null!;

    [ForeignKey("ModifiedBy")]
    [InverseProperty("AdminModifiedByNavigations")]
    public virtual AspNetUser? ModifiedByNavigation { get; set; }

    [InverseProperty("Admin")]
    public virtual ICollection<RequestStatusLog> RequestStatusLogs { get; set; } = new List<RequestStatusLog>();

    [InverseProperty("Admin")]
    public virtual ICollection<RequestWiseFile> RequestWiseFiles { get; set; } = new List<RequestWiseFile>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssignmentDataAccessLayer.DataModels;

[Table("Project")]
public partial class Project
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string? ProjectName { get; set; }

    [StringLength(70)]
    public string? Assignee { get; set; }

    [Column("DomainID")]
    public int? DomainId { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }

    [StringLength(150)]
    public string? Domain { get; set; }

    [Column(TypeName = "character varying")]
    public string? City { get; set; }

    [ForeignKey("DomainId")]
    [InverseProperty("Projects")]
    public virtual Domain? DomainNavigation { get; set; }
}

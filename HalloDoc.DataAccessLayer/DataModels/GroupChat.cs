using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

public partial class GroupChat
{
    [Key]
    public int GroupChatsId { get; set; }

    public int? GroupId { get; set; }

    [Column(TypeName = "character varying")]
    public string? SenderId { get; set; }

    [Column(TypeName = "character varying")]
    public string? Message { get; set; }

    public int? RequestId { get; set; }

    public int? PhysicianId { get; set; }

    public int? AdminId { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? SentDate { get; set; }

    public TimeOnly? SentTime { get; set; }

    [ForeignKey("AdminId")]
    [InverseProperty("GroupChats")]
    public virtual Admin? Admin { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("GroupChats")]
    public virtual GroupsMain? Group { get; set; }

    [ForeignKey("PhysicianId")]
    [InverseProperty("GroupChats")]
    public virtual Physician? Physician { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("GroupChats")]
    public virtual Request? Request { get; set; }
}

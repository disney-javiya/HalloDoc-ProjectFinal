using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("GroupsMain")]
public partial class GroupsMain
{
    [Key]
    public int GroupId { get; set; }

    [Column(TypeName = "character varying")]
    public string? GroupName { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();
}

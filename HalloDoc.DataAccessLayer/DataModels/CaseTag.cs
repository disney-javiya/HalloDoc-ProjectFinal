using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Keyless]
[Table("CaseTag")]
public partial class CaseTag
{
    public int CaseTagId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;
}

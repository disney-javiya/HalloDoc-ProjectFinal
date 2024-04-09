using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("passwordreset")]
public partial class Passwordreset
{
    [Key]
    [Column("prid")]
    public int Prid { get; set; }

    [Column("token")]
    [StringLength(500)]
    public string? Token { get; set; }

    [Column("email")]
    [StringLength(50)]
    public string? Email { get; set; }

    [Column("isupdated", TypeName = "bit(1)")]
    public BitArray? Isupdated { get; set; }

    [Column("createddate", TypeName = "timestamp without time zone")]
    public DateTime? Createddate { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("chat")]
public partial class Chat
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("senderid", TypeName = "character varying")]
    public string Senderid { get; set; } = null!;

    [Column("receiverid", TypeName = "character varying")]
    public string Receiverid { get; set; } = null!;

    [Column("senttime")]
    public TimeOnly Senttime { get; set; }

    [Column("sentdate", TypeName = "timestamp without time zone")]
    public DateTime Sentdate { get; set; }

    [Column("message", TypeName = "character varying")]
    public string? Message { get; set; }

    [Column("receiver2id", TypeName = "character varying")]
    public string? Receiver2id { get; set; }

    [Column("isGroup")]
    public bool? IsGroup { get; set; }

    [ForeignKey("Receiverid")]
    [InverseProperty("ChatReceivers")]
    public virtual AspNetUser Receiver { get; set; } = null!;

    [ForeignKey("Senderid")]
    [InverseProperty("ChatSenders")]
    public virtual AspNetUser Sender { get; set; } = null!;
}

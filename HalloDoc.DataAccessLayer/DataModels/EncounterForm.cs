using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.DataAccessLayer.DataModels;

[Table("EncounterForm")]
public partial class EncounterForm
{
    [Key]
    public int Id { get; set; }

    public int RequestId { get; set; }

    [Column("isFinalized", TypeName = "bit(1)")]
    public BitArray? IsFinalized { get; set; }

    [Column("history_illness")]
    public string? HistoryIllness { get; set; }

    [Column("medical_history")]
    public string? MedicalHistory { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Date { get; set; }

    public string? Medications { get; set; }

    public string? Allergies { get; set; }

    public decimal? Temp { get; set; }

    [Column("HR")]
    public decimal? Hr { get; set; }

    [Column("RR")]
    public decimal? Rr { get; set; }

    [Column("BP(S)")]
    public int? BpS { get; set; }

    [Column("BP(D)")]
    public int? BpD { get; set; }

    public decimal? O2 { get; set; }

    public string? Pain { get; set; }

    [Column("HEENT")]
    public string? Heent { get; set; }

    [Column("CV")]
    public string? Cv { get; set; }

    public string? Chest { get; set; }

    [Column("ABD")]
    public string? Abd { get; set; }

    public string? Extr { get; set; }

    public string? Skin { get; set; }

    public string? Neuro { get; set; }

    public string? Other { get; set; }

    public string? Diagnosis { get; set; }

    [Column("Treatment_Plan")]
    public string? TreatmentPlan { get; set; }

    [Column("medication_dispensed")]
    public string? MedicationDispensed { get; set; }

    [Column("procedures")]
    public string? Procedures { get; set; }

    [Column("Follow_up")]
    public string? FollowUp { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("EncounterForms")]
    public virtual Request Request { get; set; } = null!;
}

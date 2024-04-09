using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class viewNotes
    {
        public int RequestId { get; set; }

        [StringLength(500)]
        public string? TransferNote { get; set; }

        [StringLength(500)]
        public string? PhysicianNote { get; set; }

        [StringLength(500)]
        public string? AdminNote { get; set; }

        [StringLength(500)]
        public string? AdminCancellationNotes { get; set; }

        [StringLength(500)]
        public string? PhysicianCancellationNotes { get; set; }

        [StringLength(500)]
        public string? PatientCancellationNotes { get; set; }

        [StringLength(500)]
        public string? CaseTag { get; set; }

        [StringLength(500)]
        public string? AdditionalNote { get; set; }
    }
}

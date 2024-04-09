using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class RequestandRequestClient
    {
       
        public int? count { get; set; }
        public int? requestId { get; set; }

        public int RequestTypeId { get; set; }
        public string? patientName { get; set; }

        public string? patientDOB { get; set; }
        [StringLength(100)]
        public string? requestorName { get; set; }

        public DateTime requestedDate { get; set; }

        [StringLength(23)]
        public string? patientContact { get; set; }
        [StringLength(23)]
        public string? requestorContact { get; set; }
        [StringLength(500)]
        public string? patientAddress { get; set; }
        public short Status { get; set; }
        [StringLength(50)]
        public string? patientEmail { get; set; }

        [StringLength(50)]
        public string? patientCity { get; set; }
        [StringLength(50)]
        public string? physicianName { get; set; }

        public int? PhysicianId { get; set; }
        public IEnumerable<CaseTag> CaseTag { get; set; }

        public IEnumerable<Region> Region { get; set; }
        public IEnumerable<Physician> Physician { get; set; }

    }
}

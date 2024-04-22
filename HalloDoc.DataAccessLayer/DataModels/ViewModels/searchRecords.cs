using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class searchRecords
    {
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

        [StringLength(10)]
        public string? patientZipCode { get; set; }
        [StringLength(50)]
        public string? physicianName { get; set; }


        public string? physicianNote { get; set; }
        public string? cancelByProviderNote { get; set; }

        public string? adminNote { get; set; }

        public string? patientNote { get; set; }

        public int? PhysicianId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateOnly FromDateOfService { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateOnly? ToDateOfService { get; set; }


        [Column(TypeName = "timestamp without time zone")]
        public DateTime DateService { get; set; }

        public RequestNote? reqNotes { get; set; }




        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool PreviousPage { get; set; }
        public bool NextPage { get; set; }
    }
}

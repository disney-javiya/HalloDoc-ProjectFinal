using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class encounterModel
    {
        public int? requestId { get; set; }

        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Location { get; set; }

        public string? DateOfBirth { get; set; }
        [StringLength(23)]
        public string? phone { get; set; }

        public string? Email { get; set; }
       
        public BitArray? IsFinalized { get; set; }

      
        public string? HistoryIllness { get; set; }

       
        public string? MedicalHistory { get; set; }

      
        public DateTime? Date { get; set; }

        public string? Medications { get; set; }

        public string? Allergies { get; set; }

        public decimal? Temp { get; set; }

      
        public decimal? Hr { get; set; }

     
        public decimal? Rr { get; set; }

       
        public int? BpS { get; set; }

       
        public int? BpD { get; set; }

        public decimal? O2 { get; set; }

        public string? Pain { get; set; }

        public string? Heent { get; set; }

        public string? Cv { get; set; }

        public string? Chest { get; set; }

       
        public string? Abd { get; set; }

        public string? Extr { get; set; }

        public string? Skin { get; set; }

        public string? Neuro { get; set; }

        public string? Other { get; set; }

        public string? Diagnosis { get; set; }

       
        public string? TreatmentPlan { get; set; }

        
        public string? MedicationDispensed { get; set; }

      
        public string? Procedures { get; set; }

      
        public string? FollowUp { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentDataAccessLayer.DataModels.ViewModel
{
    public class ProjectDomainModel
    {
        public int DomainId { get; set; }

        [StringLength(150)]
        public string? DomainName { get; set; }
        public int ProjectId { get; set; }
     
        [StringLength(150)]
        public string? ProjectName { get; set; }

        [StringLength(70)]
        public string? Assignee { get; set; }


        [StringLength(200)]
        public string? Description { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateOnly? DueDate { get; set; }

        [Column(TypeName = "character varying")]
        public string? City { get; set; }
    }
}

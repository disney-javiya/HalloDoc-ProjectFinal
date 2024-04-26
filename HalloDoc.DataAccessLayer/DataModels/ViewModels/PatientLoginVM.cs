using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class PatientLoginVM
    {
        
        [StringLength(256)]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

    }
}

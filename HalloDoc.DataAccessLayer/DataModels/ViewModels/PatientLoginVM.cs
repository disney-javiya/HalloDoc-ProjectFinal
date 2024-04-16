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
        [Required(ErrorMessage = "Email is required")]
        [StringLength(256)]
        public string? Email { get; set; }

    }
}

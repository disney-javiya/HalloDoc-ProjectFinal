using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class ResetPasswordVM
    {
        [StringLength(256)]
        [Required(ErrorMessage = "Email is Required")]
        public string? Email { get; set; }

        [StringLength(256)]
        [Required(ErrorMessage = "Password is Required")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Column(TypeName = "character varying")]
        public string? CPasswordHash { get; set; }

        [Column(TypeName = "character varying")]
        public string? Token { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class requestSomeoneElse
    {
        [StringLength(250)]
        public string Symptoms { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(50)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? Mobile { get; set; }

        [StringLength(100)]
        public string? Street { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }


        [StringLength(100)]
        public string? Relation { get; set; }

        public IFormFile[]? MultipleFiles { get; set; }
    }
}

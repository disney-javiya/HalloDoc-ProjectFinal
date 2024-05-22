using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class GroupChatViewModel
    {
        public int GroupId { get; set; }
        public string? GroupName { get; set; }

        public string? SenderId { get; set; }

        [Column(TypeName = "character varying")]
        public string? Message { get; set; }

        public int? RequestId { get; set; }

        public int? PhysicianId { get; set; }

        public int? AdminId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? SentDate { get; set; }

        public TimeOnly? SentTime { get; set; }

    }
}

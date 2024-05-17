using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class ChatViewModel
    {
        public int PhysicianId { get; set; }
        public int AdminId { get; set; }
        public string physicianName { get; set; }
        public string CurrentUserId { get; set; }
        public string SenderType { get; set; }
        public string ReceiverType { get; set; }
        public string? PatientAspId { get; set; }
        public string PatientName { get; set;}
    }
}

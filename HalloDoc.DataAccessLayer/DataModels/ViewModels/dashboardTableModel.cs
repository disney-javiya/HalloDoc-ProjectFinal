using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class dashboardTableModel
    {
        public List<RequestandRequestClient> result {  get; set; }

        public List<searchRecords> records { get; set; }

        public List<EmailLog> emailLogs { get; set; }
        public List<BlockRequest> blockRequests { get; set; }
        public List<Physician> physicians { get; set; }
        public List<HealthProfessional> healthProfessionals { get; set; }

        public List<User> users { get; set; }
        public List<Smslog> SMSLogs { get; set; }
        public List<Role> roles { get; set; }
        public List<userAccessModel> userAccess { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool PreviousPage { get; set; }
        public bool NextPage { get; set; }
    }
}

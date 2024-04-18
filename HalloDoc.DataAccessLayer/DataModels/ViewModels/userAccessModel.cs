using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class userAccessModel
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public int PhysicianId { get; set; }

        public string AccountType { get; set; }

        public string AccountPOC { get; set; }

        public string Phone { get; set; }

        public string email { get; set; }
        public short? Status { get; set; }

        public int OpenRequests { get; set; }

        public int? RegionId { get; set; }

    }
}

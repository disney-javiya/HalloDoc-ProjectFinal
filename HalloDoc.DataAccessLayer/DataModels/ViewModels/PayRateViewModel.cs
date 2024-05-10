using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class PayRateViewModel
    {
        public int? PhysicianId { get; set; }
        public int? NightshiftWeekend { get; set; }
        public int? Shift { get; set; }
        public int? HousecallsNightsWeekend { get; set; }
        public int? PhoneConsults { get; set; }
        public int? PhoneConsultsNightsWeekend { get; set; }
        public int? BatchTesting { get; set; }
        public int? Housecall { get; set; }
    }
}

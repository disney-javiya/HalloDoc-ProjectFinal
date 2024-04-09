using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class ShiftDetailsModel
    {
        public List<Physician> physicians { get; set; }
        public List<Region> regions { get; set; }
        public List<Shift> shifts { get; set; }
        public Shift shiftData { get; set; }
        public ShiftDetail ShiftDetailData { get; set; }
        public List<ShiftDetail> shiftdetail { get; set; }
        public List<ShiftDetailsModel> shiftDetails { get; set; }
        public string PhysicianName { get; set; }
        public int Physicianid { get; set; }
        public string RegionName { get; set; }
        public int RegionId { get; set; }
        public short Status { get; set; }
        public TimeOnly Starttime { get; set; }
        public DateOnly Shiftdate { get; set; }
        public TimeOnly Endtime { get; set; }
        public int Shiftdetailid { get; set; }
        public int? RepeatUpto { get; set; }
    }
}

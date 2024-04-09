using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HalloDoc.DataAccessLayer.DataModels.ViewModels
{
    public class CancellationReason
    {
        public int CaseTagId { get; set; }

        public List<SelectListItem>? CancellationReasons { get; set; }
    }
    
}

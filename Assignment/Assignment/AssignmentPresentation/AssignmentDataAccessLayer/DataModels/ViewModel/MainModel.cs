using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentDataAccessLayer.DataModels.ViewModel
{
    public class MainModel
    {
        //public List<ProjectDomainModel> projects { get; set; }
        public List<Project> projects { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool PreviousPage { get; set; }
        public bool NextPage { get; set; }
    }
}

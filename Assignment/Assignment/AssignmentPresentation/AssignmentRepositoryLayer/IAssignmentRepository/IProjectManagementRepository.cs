using AssignmentDataAccessLayer.DataModels;
using AssignmentDataAccessLayer.DataModels.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentRepositoryLayer.IAssignmentRepository
{
    public interface IProjectManagementRepository
    {
        //public List<ProjectDomainModel> GetAllProjects();
        public List<Project> GetAllProjects();
        public void AddProjectPost(string ProjectName, string Assignee, string Description, DateOnly DueDate, string City, string DomainName);
        //public ProjectDomainModel GetProjectDetail(int projectId);
        public Project GetProjectDetail(int projectId);
        public void EditDetailsPost(int projectId, string ProjectName, string Assignee, string Description, DateOnly DueDate, string City, string DomainName);
        public void DeletePost(int projectId);
        public List<Domain> GetAllDomains();
        public List<Project> filterData(string projectName);
    }
}

using AssignmentDataAccessLayer.DataContext;
using AssignmentDataAccessLayer.DataModels;
using AssignmentDataAccessLayer.DataModels.ViewModel;
using AssignmentRepositoryLayer.IAssignmentRepository;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentRepositoryLayer
{
    public class ProjectManagementRepository: IProjectManagementRepository
    {

        private readonly ApplicationDbContext _context;

        public ProjectManagementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Project> GetAllProjects()
        {

          List<Project> project =  _context.Projects.ToList();
            return project;
        }

        public List<Project> filterData(string projectName)
        {
            List<Project> project = GetAllProjects();
            project = project.Where(u => u.ProjectName != null && u.ProjectName.ToLower().Contains(projectName)).ToList();
            
            return project;
        }
        public List<Domain> GetAllDomains()
        {

            List<Domain> domains = _context.Domains.ToList();
            return domains;
        }

     

        public void AddProjectPost( string ProjectName, string Assignee, string Description, DateOnly DueDate, string City, string DomainName)
        {
            Project project = new Project();
            project.ProjectName = ProjectName;
            project.Assignee = Assignee;
            project.Description = Description;
            project.DueDate = DueDate;
            project.City = City;
            Domain d = _context.Domains.Where(x => x.Name == DomainName).FirstOrDefault();
            if (d != null)
            {
                project.Domain = d.Name;
                project.DomainId = d.Id;

            }
           

            _context.Projects.Add(project);
            _context.SaveChanges();
        }


        public Project GetProjectDetail(int projectId)
        {
            List<Project> model = GetAllProjects();
            Project result = model.Where(x => x.Id == projectId).FirstOrDefault();
            return result;
        }

      
        public void EditDetailsPost(int projectId, string ProjectName, string Assignee, string Description, DateOnly DueDate, string City, string DomainName)
        {
          Project project =  _context.Projects.Where(x => x.Id == projectId).FirstOrDefault();
            if(project != null)
            {
                project.ProjectName = ProjectName;
                project.Assignee = Assignee;
                project.Description = Description;
                project.DueDate = DueDate;
                project.City = City;
                string name = _context.Domains.Where(x=>x.Name == DomainName).Select(x=>x.Name).FirstOrDefault();
                if(name != null)
                {
                    project.Domain = name;
                }
                else
                {
                    project.Domain = DomainName;
                }
                _context.SaveChanges();
            }
        }

        public void DeletePost(int projectId)
        {
           Project p =  _context.Projects.Where(x => x.Id == projectId).FirstOrDefault();
            if(p != null)
            {
                _context.Projects.Remove(p);
                _context.SaveChanges();
            }
        }
    }
}

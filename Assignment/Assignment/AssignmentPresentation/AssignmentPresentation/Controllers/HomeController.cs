using AssignmentDataAccessLayer.DataModels;
using AssignmentDataAccessLayer.DataModels.ViewModel;
using AssignmentPresentation.Models;
using AssignmentRepositoryLayer.IAssignmentRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Numerics;

namespace AssignmentPresentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectManagementRepository _projectRepository;

        public HomeController(ILogger<HomeController> logger, IProjectManagementRepository projectRepository)
        {
            _logger = logger;
            _projectRepository = projectRepository;
        }

        public IActionResult Index(string? projectName, int pagenumber=1)
        {

            MainModel model = new MainModel();
            if (projectName != null )
            {
                model.projects = _projectRepository.filterData(projectName);
            }
            else
            {
                model.projects = _projectRepository.GetAllProjects();
            }

            
            return View(model);
        }


        [HttpPost]
        public IActionResult AddProjectPost(string projectName, string assignee, string description, DateOnly dueDate, string city, string domainName)
        {
            _projectRepository.AddProjectPost(projectName, assignee, description, dueDate, city, domainName);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public Project GetProjectDetail(int projectId)
        {
            Project projectDomainModel = _projectRepository.GetProjectDetail(projectId);
            return projectDomainModel;
        }
        [HttpPost]
        public IActionResult EditDetailsPost(int projectId, string ProjectName, string Assignee, string Description, DateOnly DueDate, string City, string DomainName)
        {
            _projectRepository.EditDetailsPost(projectId, ProjectName, Assignee, Description, DueDate, City, DomainName);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeletePost(int projectId)
        {
            _projectRepository.DeletePost(projectId);
            return RedirectToAction("Index");
        }

        public List<Domain> GetAllDomains()
        {
          List<Domain> domain = _projectRepository.GetAllDomains();
            return domain;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
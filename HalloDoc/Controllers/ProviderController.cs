using Microsoft.AspNetCore.Mvc;
using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.Models;
using System.Diagnostics;
using Repository.IRepository;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using NETCore.MailKit.Core;
using System.Collections;
using System.Net.Mail;
using System.Net;
using HalloDoc.DataAccessLayer.DataContext;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Tokens;
using HalloDoc.AuthMiddleware;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Repository;
namespace HalloDoc.Controllers
{
    public class ProviderController : Controller
    {

        private readonly ILogger<ProviderController> _logger;
        private readonly IProviderRepository _providerRepository;
        private readonly ApplicationDbContext _context;
        private readonly IAuthenticateRepository _authenticate;


        public ProviderController(ILogger<ProviderController> logger, IProviderRepository providerRepository, ApplicationDbContext context, IAuthenticateRepository authenticate)
        {
            _logger = logger;
            _providerRepository = providerRepository;
            _context = context;
            _authenticate = authenticate;


        }



       
        //[CustomeAuthorize("Provider")]
        public IActionResult providerDashboard()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }

        public List<int> getCountNumber()
        {
            List<int> result = new List<int>();
            IEnumerable<RequestandRequestClient> res;
            for (int i = 1; i <= 6; i++)
            {
                res = _providerRepository.getRequestStateData(i, ViewBag.Data);
                result.Add(res.Count());
            }


            return result;
        }

        public IActionResult providerTableData(string type,  string patient_name, string typeid, int pagesize, int pagenumber = 1)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            int t = int.Parse(type);
            int tid = 0;
          
            if (typeid != null && typeid != "all")
            {
                tid = int.Parse(typeid);
            }
            if (typeid == "all")
            {
                typeid = null;
            }
           
           

            IEnumerable<RequestandRequestClient> res = _providerRepository.getRequestStateData(t, ViewBag.Data);
            dashboardTableModel model = new();
            List<RequestandRequestClient> result;
           
            if ( patient_name != null && typeid == null)
            {
                result = _providerRepository.getFilterByName(res, patient_name);
            }
            else if(  patient_name == null && typeid != null)
            {
                result = _providerRepository.getByRequesttypeId(res, tid);
            }
            else if ( patient_name != null && typeid != null)
            {
                result = _providerRepository.getFilterByrequestTypeAndName(res, tid, patient_name);
            }
            else 
            {
                result = res.ToList();
            }
           
            model.result = result;
            var count = result.Count();
            if (count > 0)
            {
                result = result.Skip((pagenumber - 1) * 3).Take(3).ToList();
                model.result = result;
                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }
            switch (t)
            {
                case 1:
                    return PartialView("_newStateProvider", model);

                case 2:
                    return PartialView("_pendingStateProvider", model);

                case 3:
                    return PartialView("_activeStateProvider", model);

                case 4:
                    return PartialView("_concludeStateProvider", model);

                default:
                    return View("providerDashboard");

            }


        }

        public IActionResult providerAccept(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerAccept(requestId, ViewBag.Data);
            return View("providerDashboard");
        }

        
        public IActionResult providerViewCase(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            var requestClient = _providerRepository.getPatientInfo(requestId);
            if (requestClient != null)
            {
                var confirmationNumber = _providerRepository.getConfirmationNumber(requestId);
                ViewBag.ConfirmationNumber = confirmationNumber;
            }

            return View(requestClient);

        }

        [HttpGet]
        public IActionResult providerViewNotes(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            var res = _providerRepository.getNotes(requestId, ViewBag.Data);


            return View(res);

        }
        
        [HttpPost]
        public IActionResult providerViewNotes(int requestId, viewNotes v)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerNotes(requestId, v, ViewBag.Data);
            return RedirectToAction("providerViewNotes", new { requestId = requestId });

        }

        [HttpPost]
        public IActionResult providerTransferCase(string requestId, string additionalNotesTransfer)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerTransferCase(requestId,  additionalNotesTransfer, ViewBag.Data);
            return RedirectToAction("providerDashboard");

        }

        public IActionResult providerViewUploads(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
           
            var document = _providerRepository.GetDocumentsByRequestId(requestId);
            ViewBag.pname = _providerRepository.getName(requestId.ToString());
            ViewBag.num = _providerRepository.getConfirmationNumber(requestId);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);

        }









        public IActionResult UploadFiles(int requestId, List<IFormFile> files)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.UploadFiles(requestId, files, ViewBag.Data);
            return RedirectToAction("providerViewUploads", new { requestId = requestId });
        }


        public IActionResult DownloadFile(int fileId)
        {
            var file = _providerRepository.GetFileById(fileId);
            if (file == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine("wwwroot/Files", file.FileName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", file.FileName);
        }

        public IActionResult DeleteFile(int requestId, int fileId)
        {
            _providerRepository.DeleteFile(fileId);

            return RedirectToAction("providerViewUploads", new { requestId = requestId });
        }


        public IActionResult DownloadFiles(string fileIds, int? requestId)
        {
            IEnumerable<RequestWiseFile> files;

            if (!fileIds.IsNullOrEmpty())
            {
                var ids = fileIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToList();

                files = _providerRepository.GetFilesByIds(ids);
            }
            else if (requestId != null)
            {
                files = _providerRepository.GetFilesByRequestId(requestId.Value);
            }
            else
            {

                return BadRequest("No files selected or invalid request.");
            }
            var zipMemoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var filePath = Path.Combine("wwwroot/Files", file.FileName);
                    var entry = zipArchive.CreateEntry(file.FileName);
                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return File(zipMemoryStream, "application/zip", "DownloadedFiles.zip");
        }


        public IActionResult DeleteFiles(string fileIds, int? requestId)
        {
            IEnumerable<RequestWiseFile> files;

            if (!fileIds.IsNullOrEmpty())
            {
                var ids = fileIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToList();

                _providerRepository.GetFilesByIdsDelete(ids);
            }
            else if (requestId != null)
            {
                _providerRepository.GetFilesByRequestIdDelete(requestId.Value);
            }
            else
            {

                return BadRequest("No files selected or invalid request.");
            }

            return RedirectToAction("providerViewUploads", new { requestId = requestId });

        }


        public async Task<ActionResult> SendEmailDocument(string fileIds, int requestId)
        {
            var patientEmail = _providerRepository.GetPatientEmail(requestId);

            string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";
            string senderPassword = "Disney@20";
            string filesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");

            SmtpClient client = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "HalloDoc-Documents"),
                Subject = "Documents",
                IsBodyHtml = true,
                Body = "Please review the following documents:"
            };

            if (string.IsNullOrWhiteSpace(patientEmail))
            {
                return BadRequest("Patient email not found or invalid.");
            }



            if (!fileIds.IsNullOrEmpty())
            {
                var ids = fileIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToList();

                var selectedfiles = _providerRepository.GetSelectedFiles(ids);
                var filesInFolder = Directory.GetFiles(filesFolder);

                foreach (var fileName in selectedfiles)
                {
                    if (filesInFolder.Contains(Path.Combine(filesFolder, fileName)))
                    {
                        mailMessage.Attachments.Add(new Attachment(Path.Combine(filesFolder, fileName)));
                    }
                }
            }
            else
            {
                var fileNames = _providerRepository.GetAllFiles(requestId);

                var filesInFolder = Directory.GetFiles(filesFolder);

                foreach (var fileName in fileNames)
                {
                    if (filesInFolder.Contains(Path.Combine(filesFolder, fileName)))
                    {
                        mailMessage.Attachments.Add(new Attachment(Path.Combine(filesFolder, fileName)));
                    }
                }

            }





            mailMessage.To.Add(patientEmail);
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), requestId, ViewBag.Data, filesFolder);
            try
            {
                await client.SendMailAsync(mailMessage);
                return RedirectToAction("providerViewUploads", new { requestId = requestId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }

        }
    }
}

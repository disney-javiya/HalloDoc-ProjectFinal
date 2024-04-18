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
using DocumentFormat.OpenXml.Office2010.Excel;

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



       
        [CustomeAuthorize("Physician")]
        public IActionResult providerDashboard()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("ProviderPhysician")]
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
        [CustomeAuthorize("Physician")]
        public IActionResult providerAccept(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerAccept(requestId, ViewBag.Data);
            return View("providerDashboard");
        }

        [CustomeAuthorize("Physician")]
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
        [CustomeAuthorize("Physician")]
        [HttpGet]
        public IActionResult providerViewNotes(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            var res = _providerRepository.getNotes(requestId, ViewBag.Data);


            return View(res);

        }
        [CustomeAuthorize("Physician")]
        [HttpPost]
        public IActionResult providerViewNotes(int requestId, viewNotes v)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerNotes(requestId, v, ViewBag.Data);
            return RedirectToAction("providerViewNotes", new { requestId = requestId });

        }
        [CustomeAuthorize("Physician")]
        [HttpPost]
        public IActionResult providerTransferCase(string requestId, string additionalNotesTransfer)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerTransferCase(requestId,  additionalNotesTransfer, ViewBag.Data);
            return RedirectToAction("providerDashboard");

        }
        [CustomeAuthorize("Physician")]
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








        [CustomeAuthorize("Physician")]
        public IActionResult UploadFiles(int requestId, List<IFormFile> files)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.UploadFiles(requestId, files, ViewBag.Data);
            return RedirectToAction("providerViewUploads", new { requestId = requestId });
        }

        [CustomeAuthorize("Physician")]
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
        [CustomeAuthorize("Physician")]
        public IActionResult DeleteFile(int requestId, int fileId)
        {
            _providerRepository.DeleteFile(fileId);

            return RedirectToAction("providerViewUploads", new { requestId = requestId });
        }

        [CustomeAuthorize("Physician")]
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

        [CustomeAuthorize("Physician")]
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

        [CustomeAuthorize("Physician")]
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

        [CustomeAuthorize("Physician")]
        [HttpGet]
        public List<string> providerSendAgreement(string requestId)
        {
            List<string> res = new List<string>();
            ViewBag.Data = HttpContext.Session.GetString("key");
            res = _providerRepository.adminSendAgreementGet(requestId);
            return res;

        }
        [CustomeAuthorize("Physician")]
        [HttpPost]
        public IActionResult providerSendAgreement(string requestId, string email, string mobile)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            AspNetUser aspNetUser = _providerRepository.GetUserByEmail(email);
            Physician physician = _providerRepository.getProviderInfo(ViewBag.Data);
            if (aspNetUser == null)
            {
                ModelState.AddModelError("Email", "Email does not exist");
                return RedirectToAction("Index");
            }
            else
            {
                string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";

                string senderPassword = "Disney@20";
                int req = int.Parse(requestId);
                string agreementLink = $"{Request.Scheme}://{Request.Host}/Home/reviewAgreement?requestId={req}&physicianId={physician.PhysicianId}";

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
                    From = new MailAddress(senderEmail, "HalloDoc"),
                    Subject = "Review the Agreement",
                    IsBodyHtml = true,
                    Body = $"Please review the agreement by clicking the following link. Further treatment will be carried out only after you agreee to the conditions.: <a href='{agreementLink}'>{agreementLink}</a>"
                };

                //mailMessage.To.Add(email);
                mailMessage.To.Add("pateldisney20@gmail.com");
                client.SendMailAsync(mailMessage);

                TempData["isSentAgreement"] = true;
                //sendSMS(requestId, mobile);
                _providerRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), int.Parse(requestId), ViewBag.Data, null);
                return RedirectToAction("providerDashboard");
            }


        }


        [HttpGet]
        [CustomeAuthorize("Physician")]
        public IActionResult sendOrder()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            sendOrder s = new sendOrder();
            s.HealthProfessionalType = _providerRepository.GetAllHealthProfessionalType();
            s.HealthProfessional = _providerRepository.GetAllHealthProfessional();
            return View(s);
        }
        [CustomeAuthorize("Physician")]
        [HttpGet]
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId)
        {
            var res = _providerRepository.GetHealthProfessional(healthprofessionalId);
            return res;
        }
        public List<HealthProfessional> GetAllHealthProfessional()
        {
            var res = _providerRepository.GetAllHealthProfessional();
            return res;
        }
        public List<HealthProfessionalType> GetAllHealthProfessionalType()
        {
            var res = _providerRepository.GetAllHealthProfessionalType();
            return res;
        }
        [HttpGet]
        public HealthProfessional GetProfessionInfo(int vendorId)
        {
            var res = _providerRepository.GetProfessionInfo(vendorId);
            return res;
        }
        [HttpPost]
        [CustomeAuthorize("Physician")]
        public IActionResult sendOrder(int requestId, sendOrder s)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.sendOrderDetails(requestId, s, ViewBag.Data);
            return RedirectToAction("sendOrder", new { requestId = requestId });
        }
        [CustomeAuthorize("Physician")]
        public IActionResult providerEncounterCase(string calltype, int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerEncounterCase(requestId, calltype, ViewBag.Data);
            return View("providerDashboard");
        }
        [CustomeAuthorize("Physician")]
        public IActionResult providerProfile()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            Physician physician = _providerRepository.getProviderInfo(ViewBag.Data);
            var res = _providerRepository.getPhysicianDetails(physician.PhysicianId);
            return View(res);
           
        }


        [CustomeAuthorize("Physician")]
       
        [HttpGet]
        public List<Region> getPhysicianRegions()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //List<Region> res = new List<Region>();
            var res = _providerRepository.getPhysicianRegions(ViewBag.Data);
            return res;
        }
        [CustomeAuthorize("Physician")]
       
        [HttpPost]
        public IActionResult physicianUpdatePassword(string password)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _providerRepository.physicianUpdatePassword(ViewBag.Data, password);
            return RedirectToAction("providerProfile");

        }
        [CustomeAuthorize("Physician")]
        [HttpGet]
        public IActionResult providerEncounterForm(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            encounterModel em = _providerRepository.providerEncounterForm(requestId);
            return View(em);
        }
        [CustomeAuthorize("Physician")]
        [HttpPost]
        public IActionResult providerEncounterForm(int requestId, encounterModel em)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
           _providerRepository.providerEncounterFormPost(requestId, em);
            return RedirectToAction("providerEncounterForm" , new { requestId = requestId });
        }

        public IActionResult editAccountRequest(string reqmessage)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");


           Physician p = _providerRepository.getProviderInfo(ViewBag.Data);
           AspNetUser a = _providerRepository.GetUserById(p.CreatedBy);
            string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";

            string senderPassword = "Disney@20";


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
                From = new MailAddress(senderEmail, "HalloDoc"),
                Subject = "HalloDoc - Request to edit",
                IsBodyHtml = true,
                Body = $"This message is from physician to edit Account! Additional Message:  {reqmessage}"
            };

            //mailMessage.To.Add(a.Email);
            mailMessage.To.Add("pateldisney20@gmail.com");

            client.SendMailAsync(mailMessage);

            ViewBag.Data = HttpContext.Session.GetString("key");

            _providerRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), null, ViewBag.Data, null);




            return View("providerProfile");

        }
        [CustomeAuthorize("Physician")]
        public IActionResult transferToConcludeState(int requestId)
        {
            _providerRepository.transferToConcludeState(requestId);
            return RedirectToAction("providerDashboard");
        }
        [CustomeAuthorize("Physician")]
        public IActionResult providerIsFinal(int requestId)
        {
            _providerRepository.providerIsFinal(requestId);
            return RedirectToAction("providerDashboard");
        }

        [CustomeAuthorize("Physician")]
        public IActionResult providerConcludeCare(int requestId)
        {
            var document = _providerRepository.GetDocumentsByRequestId(requestId);
            ViewBag.pname = _providerRepository.getName(requestId.ToString());
            ViewBag.num = _providerRepository.getConfirmationNumber(requestId);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }
        [CustomeAuthorize("Physician")]
        public IActionResult providerConcludeCarePost(int requestId, string notes)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.providerConcludeCarePost(requestId, notes, ViewBag.Data);
            return RedirectToAction("providerDashboard");
        }

        public IActionResult providerMySchedule()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ShiftDetailsModel model = getSchedulingData();
            return View(model);
        }

        public ShiftDetailsModel getSchedulingData()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ShiftDetailsModel model = new();
            model.physicians = _providerRepository.GetAllPhysicians();
            model.regions = _providerRepository.getAllRegions();
            model.shiftDetails = _providerRepository.getshiftDetail(ViewBag.Data);

            return model;
        }
        public IActionResult _ViewShiftModalProvider()
        {
            return View();
        }
        [HttpGet]
        public IActionResult _ViewShiftModalProvider(int id)
        {
            ShiftDetailsModel res = _providerRepository.getViewShiftData(id);
            return View(res);
        }

        [HttpPost]
        public IActionResult insertShift(shiftViewModel s, string checktoggle, int[] dayList)
        {
            //string selected = Request.Form["uncheckedCheckboxes"];
            ViewBag.Data = HttpContext.Session.GetString("key");
            _providerRepository.insertShift(s, checktoggle, dayList, ViewBag.Data);

            return RedirectToAction("providerMySchedule");
        }
        public IActionResult logOut()
        {

            Response.Cookies.Delete("jwt");
            HttpContext.Session.Remove("key");
            return RedirectToAction("Index", "Admin");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.Models;
using System.Diagnostics;
using Repository.IRepository;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using HalloDoc.DataAccessLayer.DataContext;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.EntityFrameworkCore;
using System.Security;
using Repository;
using System.IO.Compression;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Net.Mail;
using System.Net;
using Elfie.Serialization;
using HalloDoc.AuthMiddleware;
using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.Http;
using System.Reflection;
using System.Configuration.Provider;
using System.Web.Helpers;
using System.Globalization;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Bibliography;
using System.Drawing;

namespace HalloDoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminRepository _adminRepository;
        private readonly IAuthenticateRepository _authenticate;

      

        public AdminController(ILogger<AdminController> logger, IAdminRepository adminRepository, IAuthenticateRepository authenticate)
        {
            _logger = logger;
            _adminRepository = adminRepository;
            _authenticate = authenticate;

        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AspNetUser user)
        {


            var data = _adminRepository.ValidateUser(user.Email, user.PasswordHash);
            if (data == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                TempData["isNotValidateUser"] = true;
                return View(user);
            }
            AspNetUser loginuser = new()
            {
                Email = data.Email,
                UserName = data.UserName,
                Id = data.Id
            };

            string rolename = _adminRepository.getRoleName(loginuser);

            var jwttoken = _authenticate.GenerateJwtToken(loginuser, rolename);
            Response.Cookies.Append("jwt", jwttoken);
            ViewBag.LoginSuccess = true;
            
            HttpContext.Session.SetString("key", user.Email);
            ViewBag.Data = HttpContext.Session.GetString("key");
            string returnUrl = HttpContext.Request.Query["returnUrl"];
            if (rolename == "Admin")
            {
                
                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return View("adminDashboard");
            }
            if (rolename == "Physician")
            {
                //string returnUrl = HttpContext.Request.Query["returnUrl"];
                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("providerDashboard", "Provider");
            }

            return View();
        }
        public IActionResult adminForgotpassword()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Reset(PatientLoginVM obj)
        {
            AspNetUser aspNetUser = _adminRepository.GetUserByEmail(obj.Email);
            if (aspNetUser == null)
            {

                return RedirectToAction("Forgotpassword");
            }
            else
            {
                string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";

                string senderPassword = "Disney@20";
                string Token = Guid.NewGuid().ToString();
                string resetLink = $"{Request.Scheme}://{Request.Host}/Admin/Resetpassword?token={Token}";

            
                _adminRepository.passwordresetInsert(obj.Email, Token);


                

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
                    Subject = "Set up your Account",
                    IsBodyHtml = true,
                    Body = $"Please click the following link to reset your password: <a href='{resetLink}'>{resetLink}</a>"
                };

                //mailMessage.To.Add(obj.Email);
                mailMessage.To.Add("pateldisney20@gmail.com");

                client.SendMailAsync(mailMessage);

                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpGet]

        public IActionResult ResetPassword(string token)
        {
            var passwordReset = _adminRepository.getPasswordReset(token);

            ResetPasswordVM resetPasswordVM = new ResetPasswordVM
            {
                Email = passwordReset.Email,
                Token = token
            };

            TimeSpan difference = (TimeSpan)(DateTime.Now - passwordReset.Createddate);

            double hours = difference.TotalHours;

            if (hours > 24)
            {
                return NotFound();
            }

            if (passwordReset.Isupdated == new BitArray(1))
            {
                ModelState.AddModelError("Email", "You can only update one time using this link");
                return View(resetPasswordVM);
            }
            TempData["success"] = "Enter Password";
            return View(resetPasswordVM); ;

        }


        [HttpPost]


        public IActionResult ResetPassword(ResetPasswordVM obj)
        {

            _adminRepository.ResetPassword(obj);
          

            return RedirectToAction("Index", "Admin");
        }

        [CustomeAuthorize("Admin")]
        public IActionResult adminDashboard()
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
                res = _adminRepository.getRequestStateData(i);
                result.Add(res.Count());
            }


            return result;
        }
     
        public IActionResult adminTableData(string type, string regionId, string patient_name, string typeid, int pagesize, int pagenumber = 1)
        {
            int t = int.Parse(type);
            int tid = 0;
            int r = 0;
            if (typeid != null && typeid != "all")
            {
                tid = int.Parse(typeid);
            }
            if (typeid == "all")
            {
                typeid = null;
            }
            if (regionId == "Select Region")
            {
                regionId = null;
            }
            if (regionId != null && regionId != "Select Regions")
            {
                r = int.Parse(regionId);
            }

            IEnumerable<RequestandRequestClient> res = _adminRepository.getRequestStateData(t);
            dashboardTableModel model = new();
            List<RequestandRequestClient> result;
            if (regionId != null && patient_name == null && typeid == null)
            {
                result = _adminRepository.getFilterByRegions(res, r);
            }
            else if (regionId == null && patient_name != null && typeid == null)
            {
                result = _adminRepository.getFilterByName(res, patient_name);
            }
            else if (regionId == null && patient_name == null && typeid != null)
            {
                result = _adminRepository.getByRequesttypeId(res, tid);
            }
            else if (regionId != null && patient_name != null && typeid == null)
            {
                result = _adminRepository.getFilterByRegionAndName(res, patient_name, r);
            }
            else if (regionId == null && patient_name == null && typeid == null)
            {
                result = res.ToList();
            }
            else
            {
                result = _adminRepository.getByRequesttypeIdRegionAndName(res, tid, r, patient_name);
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
                    return PartialView("_newState", model);

                case 2:
                    return PartialView("_pendingState", model);

                case 3:
                    return PartialView("_activeState", model);

                case 4:
                    return PartialView("_concludeState", model);

                case 5:
                    return PartialView("_tocloseState", model);

                case 6:
                    return PartialView("_unpaidState", model);

                default:
                    return View("adminDashboard");

            }


        }

        public List<Region> getAllRegions()
        {
            List<Region> r = _adminRepository.getAllRegions();
            return r;
        }
        public List<Physician> GetAllPhysicians()
        {
            List<Physician> r = _adminRepository.GetAllPhysicians();
            return r;
        }


        [HttpPost]
        public FileResult Export(string type, string regionId, string patient_name, string typeid)
        {
            int t = int.Parse(type);
            int tid = 0;
            int r = 0;
            if (typeid != null && typeid != "all")
            {
                tid = int.Parse(typeid);
            }
            if(typeid == "all")
            {
                typeid = null;
            }
            if (regionId != null && regionId != "Select Regions")
            {
                r = int.Parse(regionId);
            }
            if (regionId == "Select Regions")
            {
                regionId = null;
            }
            IEnumerable<RequestandRequestClient> res = _adminRepository.getRequestStateData(t);
            List<RequestandRequestClient> result;
            if (regionId != null && patient_name == null && typeid == null)
            {
                result = _adminRepository.getFilterByRegions(res, r);
            }
            else if (regionId == null && patient_name != null && typeid == null)
            {
                result = _adminRepository.getFilterByName(res, patient_name);
            }
            else if (regionId == null && patient_name == null && typeid != null)
            {
                result = _adminRepository.getByRequesttypeId(res, tid);
            }
            else if (regionId != null && patient_name != null && typeid == null)
            {
                result = _adminRepository.getFilterByRegionAndName(res, patient_name, r);
            }
            else if (regionId == null && patient_name == null && typeid == null)
            {
                result = res.ToList();
            }
            else
            {
                result = _adminRepository.getByRequesttypeIdRegionAndName(res, tid, r, patient_name);
            }

            return GenerateExcel(result);

        }

        public FileResult GenerateExcel(List<RequestandRequestClient> data)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Patient Name";
            worksheet.Cell(currentRow, 2).Value = "Patient DOB";
            worksheet.Cell(currentRow, 3).Value = "Patient Email";
            worksheet.Cell(currentRow, 4).Value = "Requestor Name";
            worksheet.Cell(currentRow, 5).Value = "Physician Name";
            worksheet.Cell(currentRow, 6).Value = "Patient Contact";
            worksheet.Cell(currentRow, 7).Value = "Requestor Contact";
            worksheet.Cell(currentRow, 8).Value = "Patient Address";
            worksheet.Cell(currentRow, 9).Value = "Requested Date";

            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 30;
            worksheet.Column(4).Width = 20;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 35;
            worksheet.Column(9).Width = 15;
            foreach (var user in data)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = user.patientName;
                worksheet.Cell(currentRow, 2).Value = user.patientDOB;
                worksheet.Cell(currentRow, 3).Value = user.patientEmail;
                worksheet.Cell(currentRow, 4).Value = user.requestorName;
                worksheet.Cell(currentRow, 5).Value = user.physicianName;
                worksheet.Cell(currentRow, 6).Value = user.patientContact;
                worksheet.Cell(currentRow, 7).Value = user.requestorContact;
                worksheet.Cell(currentRow, 8).Value = user.patientAddress;
                worksheet.Cell(currentRow, 9).Value = user.requestedDate;

            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "users.xlsx");
            }
        }
        [HttpPost]
        public FileResult ExportAll(int type)
        {
            List<RequestandRequestClient> res = _adminRepository.getRequestStateData(type).ToList();
            return GenerateExcel(res);


        }


        [CustomeAuthorize("Admin")]
        public IActionResult adminViewCase(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            var requestClient = _adminRepository.getPatientInfo(requestId);
            if (requestClient != null)
            {
                var confirmationNumber = _adminRepository.getConfirmationNumber(requestId);
                ViewBag.ConfirmationNumber = confirmationNumber;
            }

            return View(requestClient);

        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult adminViewNotes(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            var res = _adminRepository.getNotes(requestId, ViewBag.Data);


            return View(res);

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminViewNotes(int requestId, viewNotes v)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminNotes(requestId, v, ViewBag.Data);
            return RedirectToAction("adminViewNotes", new { requestId = requestId });

        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public string adminCancelNote(string requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            string pname = _adminRepository.getName(requestId);
            return pname;

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminCancelNote(string requestId, string reason, string additionalNotes)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ViewBag.CancelNote = true;
            _adminRepository.adminCancelNote(requestId, reason, additionalNotes, ViewBag.Data);
            TempData["CancelNote"] = true;
            return RedirectToAction("adminDashboard");

        }

        [HttpGet]
        public List<Physician> GetPhysicians(int regionId)
        {
            var res = _adminRepository.GetPhysicians(regionId);
            return res;
        }

        [HttpPost]
        public IActionResult adminAssignNote(string requestId, string region, string physician, string additionalNotesAssign)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminAssignNote(requestId, region, physician, additionalNotesAssign, ViewBag.Data);

            TempData["AssignNote"] = true;
            return RedirectToAction("adminDashboard");

        }

        [CustomeAuthorize("Admin")]
        [HttpGet]
        public string adminBlockNote(string requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            string pname = _adminRepository.getName(requestId);
            return pname;

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminBlockNote(string requestId, string additionalNotesBlock)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminBlockNote(requestId, additionalNotesBlock, ViewBag.Data);
            TempData["BlockNote"] = true;
            return RedirectToAction("adminDashboard");

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminTransferCase(string requestId, string physician, string additionalNotesTransfer)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminTransferCase(requestId, physician, additionalNotesTransfer, ViewBag.Data);
            return RedirectToAction("adminDashboard");

        }


        [CustomeAuthorize("Admin")]
        public IActionResult adminViewUploads(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            
            var document = _adminRepository.GetDocumentsByRequestId(requestId);
            ViewBag.pname = _adminRepository.getName(requestId.ToString());
            ViewBag.num = _adminRepository.getConfirmationNumber(requestId.ToString());
            if (document == null)
            {
                return NotFound();
            }

            return View(document);

        }
        public List<string> GetNameConfirmation(string requestId)
        {
            int r = int.Parse(requestId);
            var res = _adminRepository.GetNameConfirmation(r);
            return res;
        }
        /*-----------------------------------Upload Files--------------------------------------------------*/

        public IActionResult UploadFiles(int requestId, List<IFormFile> files)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.UploadFiles(requestId, files, ViewBag.Data);
            return RedirectToAction("adminViewUploads", new { requestId = requestId });
        }


        public IActionResult DownloadFile(int fileId)
        {
            var file = _adminRepository.GetFileById(fileId);
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
            _adminRepository.DeleteFile(fileId);

            return RedirectToAction("adminViewUploads", new { requestId = requestId });
        }


        public IActionResult DownloadFiles(string fileIds, int? requestId)
        {
            IEnumerable<RequestWiseFile> files;

            if (!fileIds.IsNullOrEmpty())
            {
                var ids = fileIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToList();

                files = _adminRepository.GetFilesByIds(ids);
            }
            else if (requestId != null)
            {
                files = _adminRepository.GetFilesByRequestId(requestId.Value);
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

        [CustomeAuthorize("Admin")]
        public IActionResult DeleteFiles(string fileIds, int? requestId)
        {
            IEnumerable<RequestWiseFile> files;

            if (!fileIds.IsNullOrEmpty())
            {
                var ids = fileIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToList();

                _adminRepository.GetFilesByIdsDelete(ids);
            }
            else if (requestId != null)
            {
                _adminRepository.GetFilesByRequestIdDelete(requestId.Value);
            }
            else
            {

                return BadRequest("No files selected or invalid request.");
            }

            return RedirectToAction("adminViewUploads", new { requestId = requestId });

        }


        public async Task<ActionResult> SendEmailDocument(string fileIds, int requestId)
        {
            var patientEmail = _adminRepository.GetPatientEmail(requestId);

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

                var selectedfiles = _adminRepository.GetSelectedFiles(ids);
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
                var fileNames = _adminRepository.GetAllFiles(requestId);

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
            _adminRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), requestId, ViewBag.Data, filesFolder);
            try
            {
                await client.SendMailAsync(mailMessage);
                return RedirectToAction("adminViewUploads", new { requestId = requestId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
           
        }

        [HttpGet]
        [CustomeAuthorize("Admin")]
        public IActionResult sendOrder()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            sendOrder s = new sendOrder();
            s.HealthProfessionalType = _adminRepository.GetAllHealthProfessionalType();
            s.HealthProfessional = _adminRepository.GetAllHealthProfessional();
            return View(s);
        }
        [HttpGet]
        [CustomeAuthorize("Admin")]
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId)
        {
            var res = _adminRepository.GetHealthProfessional(healthprofessionalId);
            return res;
        }
        public List<HealthProfessional> GetAllHealthProfessional()
        {
            var res = _adminRepository.GetAllHealthProfessional();
            return res;
        }
        public List<HealthProfessionalType> GetAllHealthProfessionalType()
        {
            var res = _adminRepository.GetAllHealthProfessionalType();
            return res;
        }
        [HttpGet]
        public HealthProfessional GetProfessionInfo(int vendorId)
        {
            var res = _adminRepository.GetProfessionInfo(vendorId);
            return res;
        }
        [HttpPost]
        [CustomeAuthorize("Admin")]
        public IActionResult sendOrder(int requestId, sendOrder s)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.sendOrderDetails(requestId, s, ViewBag.Data);
            return RedirectToAction("sendOrder", new { requestId = requestId });
        }
        [HttpPost]
        [CustomeAuthorize("Admin")]
        public IActionResult adminClearCase(string requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminClearCase(requestId, ViewBag.Data);
            TempData["ClearCase"] = true;
            return RedirectToAction("adminDashboard");

        }

        [HttpGet]
        [CustomeAuthorize("Admin")]
        public List<string> adminSendAgreement(string requestId)
        {
            List<string> res = new List<string>();
            ViewBag.Data = HttpContext.Session.GetString("key");
            res = _adminRepository.adminSendAgreementGet(requestId);
            return res;

        }

        [HttpPost]
        [CustomeAuthorize("Admin")]
        public IActionResult adminSendAgreement(string requestId, string email, string mobile)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            AspNetUser aspNetUser = _adminRepository.GetUserByEmail(email);
            Admin admin = _adminRepository.getAdminInfo(ViewBag.Data);
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
                string agreementLink = $"{Request.Scheme}://{Request.Host}/Home/reviewAgreement?requestId={req}&adminId={admin.AdminId}?return=1";

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
                _adminRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), int.Parse(requestId), ViewBag.Data, null);
                return RedirectToAction("adminDashboard");
            }


        }
        public void sendSMS(string requestId, string mobile)
        {
            string req = requestId;
            string agreementLink = $"{Request.Scheme}://{Request.Host}/Home/reviewAgreement?requestId={req}";
            string accountSid = "AC5c509d7da59a0b33e1b8e928c6a1b6b9";
            string authToken = "b1193502b178351e8f7709e095524ca9";

            TwilioClient.Init(accountSid, authToken);
            var messageOptions = new CreateMessageOptions(
             new PhoneNumber("+916353121783"));
            messageOptions.From = new PhoneNumber("+15642161066");
            messageOptions.Body = "Please review the agreement by clicking the following link. Further treatment will be carried out only after you agreee to the conditions.: <a href='{agreementLink}'>{agreementLink}</a>";

            var message = MessageResource.Create(messageOptions);
            Console.WriteLine(message.Body);
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.insertSMSLog(messageOptions.Body, mobile, int.Parse(requestId), ViewBag.Data);


        }

        [CustomeAuthorize("Admin")]
        public IActionResult closeCase(int requestId)
        {
            var document = _adminRepository.GetDocumentsByRequestId(requestId);
            ViewBag.pname = _adminRepository.getName(requestId.ToString());
            ViewBag.num = _adminRepository.getConfirmationNumber(requestId.ToString());

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        [CustomeAuthorize("Admin")]
        public IActionResult closeCaseAdmin(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.closeCaseAdmin(requestId, ViewBag.Data);
            return RedirectToAction("adminDashboard");
        }

        public RequestClient getPatientInfo(int requestId)
        {
            RequestClient r = new RequestClient();
            r = _adminRepository.getPatientInfo(requestId);
            return r;
        }

        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult patientCancelNote(string requestId, string additionalNotesPatient)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.patientCancelNote(requestId, additionalNotesPatient);
            return RedirectToAction("adminDashboard");

        }

        [CustomeAuthorize("Admin")]
        public string adminTransferNotes(string requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            string res = _adminRepository.adminTransferNotes(requestId, ViewBag.Data);
            return res;

        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult adminProfile()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            Admin a = new Admin();
            a = _adminRepository.getAdminInfo(ViewBag.Data);
            return View(a);

        }

       
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminProfileUpdatePassword(string password)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminProfileUpdatePassword(ViewBag.Data, password);
            return RedirectToAction("adminProfile");

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminProfileUpdateStatus(Admin a)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminProfileUpdateStatus(ViewBag.Data, a);
            return RedirectToAction("adminProfile");

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminProfile(Admin a, string? uncheckedCheckboxes)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminUpdateProfile(ViewBag.Data, a, uncheckedCheckboxes);
            return RedirectToAction("adminProfile");

        }

        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminProfileBilling(Admin a)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminUpdateProfileBilling(ViewBag.Data, a);
            return RedirectToAction("adminProfile");

        }

        [CustomeAuthorize("Admin")]
        public IActionResult adminCreateRequest()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

           
            return View();

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminCreateRequest(createAdminRequest RequestData)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            string email = RequestData.Email;
            var row = _adminRepository.GetUserByEmail(email);
            var res = _adminRepository.adminCreateRequest(RequestData, ViewBag.Data);

            if (row == null)
            {
                SendEmailUser(RequestData.Email, res);
            }
            return RedirectToAction("adminDashboard");

        }

        public Action SendEmailUser(System.String Email, string id)
        {

            AspNetUser aspNetUser = _adminRepository.GetUserByEmail(Email);


            string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";
            string senderPassword = "Disney@20";
            string resetLink = $"{Request.Scheme}://{Request.Host}/Home/createPatientAccount?id={id}";

            _adminRepository.passwordresetInsert(Email, id);

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
                Subject = "Set up your Account",
                IsBodyHtml = true,
                Body = $"Please create password for your account: <a href='{resetLink}'>{resetLink}</a>"
            };

            mailMessage.To.Add(Email)
;

            client.SendMailAsync(mailMessage);
            ViewBag.Data = HttpContext.Session.GetString("key");
           int requestId = _adminRepository.GetUserByRequestId(aspNetUser.Id);
            _adminRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), requestId, ViewBag.Data, null);
            return null;
        }

        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult encounterForm(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            encounterModel em = _adminRepository.adminEncounterForm(requestId);
            return View(em);
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminEncounterForm(int requestId, encounterModel em)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminEncounterFormPost(requestId, em);
            return RedirectToAction("encounterForm", new { requestId = requestId });
        }


        public IActionResult downloadEncounterForm(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            encounterModel en = _adminRepository.getEncounterDetails(requestId);

            if(en == null)
            {

                return null; 
            }

            byte[] pdfdata = _adminRepository.GeneratePDF(en);
            return File(pdfdata, "application/pdf", "MedicalReport.pdf");
        }

        [HttpGet]
        public List<Region> getAdminRegions()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
         
            var res = _adminRepository.getAdminRegions(ViewBag.Data);
            return res;
        }

        [HttpGet]
        public List<Region> getAdminRegionsbyEmail(string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            var res = _adminRepository.getAdminRegions(email);
            return res;
        }

        [CustomeAuthorize("Admin")]
        public IActionResult providerMenu()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");        
            return View();
        }

        [CustomeAuthorize("Admin")]


        public IActionResult providerMenuTable(int? regionId,  int pagenumber = 1)
        {
            
            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (regionId != null )
            {
                model.physicians = _adminRepository.GetPhysicians(regionId);
            }
            else
            {
                model.physicians = _adminRepository.GetAllPhysicians();
            }

            model.roles = _adminRepository.GetAllRoles();
            var count = model.physicians.Count();
            if (count > 0)
            {
                model.physicians = model.physicians.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_providerMenu", model);
        }



        [CustomeAuthorize("Admin")]
        public IActionResult createPhysicianAccount()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            return View();
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult createPhysicianAccount(Physician p, IFormFile photo, string password, string role, List<int> region, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.createPhysicianAccount(p, photo, password, role, region, ViewBag.Data, agreementDoc, backgroundDoc, hippaDoc, disclosureDoc, licenseDoc);
            return RedirectToAction("providerMenu");
        }

        [CustomeAuthorize("Admin")]
        public IActionResult editPhysicianAccount(int physicianId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            var res = _adminRepository.getPhysicianDetails(physicianId);
            return View(res);
        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public List<Region> getPhysicianRegions(int physicianId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            var res = _adminRepository.getPhysicianRegions(physicianId);
            return res;
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult physicianUpdateStatus(int physicianId, Physician p)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.physicianUpdateStatus(ViewBag.Data, physicianId, p);
            return RedirectToAction("editPhysicianAccount", new { physicianId = physicianId });

        }


        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult physicianUpdatePassword(int physicianId, string password)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.physicianUpdatePassword(ViewBag.Data, physicianId, password);
            return RedirectToAction("adminProfile", new { physicianId = physicianId });

        }
        [CustomeAuthorize("Admin")]

        [HttpPost]
        public IActionResult physicianUpdateAccount(int physicianId, Physician p, string uncheckedCheckboxes)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.physicianUpdateAccount(ViewBag.Data, physicianId, p, uncheckedCheckboxes);
            return RedirectToAction("editPhysicianAccount", new { physicianId = physicianId });

        }

        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult physicianUpdateBilling(int physicianId, Physician p)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.physicianUpdateBilling(ViewBag.Data, physicianId, p);
            return RedirectToAction("editPhysicianAccount", new { physicianId = physicianId });

        }


        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult physicianUpdateBusiness(int physicianId, Physician p, IFormFile[] files, IFormFile? photo, IFormFile? signature)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.physicianUpdateBusiness(ViewBag.Data, physicianId, p, files, photo, signature);
            return RedirectToAction("editPhysicianAccount", new { physicianId = physicianId });


        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult physicianUpdateUpload(int physicianId, Physician p, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.physicianUpdateUpload(ViewBag.Data, physicianId, agreementDoc, backgroundDoc, hippaDoc, disclosureDoc, licenseDoc);
            return RedirectToAction("editPhysicianAccount", new { physicianId = physicianId });

        }

        [HttpGet]
        public List<Role> GetPhysiciansRoles()
        {
            List<Role> res = _adminRepository.GetPhysiciansRoles();
            return res;
        }

        [CustomeAuthorize("Admin")]
        public IActionResult deletePhysicianAccount(int physicianId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.deletePhysicianAccount(ViewBag.Data, physicianId);
            return RedirectToAction("providerMenu");
        }

       
        public IActionResult contactProvider(string physicianId, string ctype, string messagebody)
        {
            int pid = int.Parse(physicianId);
            var physician = _adminRepository.getPhysicianDetails(pid);
            if(ctype != null && messagebody !=null)
            {
                if (ctype == "1" || ctype == "3")
                {


                    string accountSid = "AC5c509d7da59a0b33e1b8e928c6a1b6b9";
                    string authToken = "b1193502b178351e8f7709e095524ca9";

                    TwilioClient.Init(accountSid, authToken);
                    var messageOptions = new CreateMessageOptions(
                     new PhoneNumber("+916353121783"));
                    messageOptions.From = new PhoneNumber("+15642161066");
                    messageOptions.Body = messagebody;

                    var message = MessageResource.Create(messageOptions);
                    Console.WriteLine(message.Body);
                    _adminRepository.insertSMSLog(messageOptions.Body, physician.Mobile, null, ViewBag.Data);
                }
                if (ctype == "2" || ctype == "3")
                {
                    string email = physician.Email;



                    if (physician == null)
                    {
                        ModelState.AddModelError("Email", "Email does not exist");
                        return RedirectToAction("Index");
                    }
                    else
                    {
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
                            Subject = "HalloDoc Admin",
                            IsBodyHtml = true,
                            Body = $"This message is for {physician.FirstName} from admin of HalloDoc! {messagebody}"
                        };

                        //mailMessage.To.Add(email);
                        mailMessage.To.Add("pateldisney20@gmail.com");

                        client.SendMailAsync(mailMessage);

                        ViewBag.Data = HttpContext.Session.GetString("key");

                        _adminRepository.insertEmailLog(mailMessage.Body, mailMessage.Subject, mailMessage.To.ToString(), null, ViewBag.Data, null);

                    }


                }
            }
           
            return RedirectToAction("providerMenu");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult adminAccess()
        {           
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }

        [CustomeAuthorize("Admin")]


        public IActionResult adminAccessTable( int pagenumber = 1)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            model.roles = _adminRepository.GetAllRoles();
           

            var count = model.roles.Count();
            if (count > 0)
            {
                model.roles = model.roles.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_adminAccess", model);
        }


        [CustomeAuthorize("Admin")]
        public IActionResult createRole()  
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult createRole(createRole r, List<string> menu)
        {
           
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.createRole(r, menu, ViewBag.Data);
            return RedirectToAction("adminAccess");
        }

        public List<Menu> menuByAccountType(int accountType)
        {
            List<Menu> menu = new List<Menu>();
            menu = _adminRepository.menuByAccountType(accountType);
            return menu;
        }
        public List<int> menuByAccountTypeRoleId(int accountType, int roleId)
        {
            List<int> menu = new List<int>();
            menu = _adminRepository.menuByAccountTypeRoleId(accountType, roleId);
            return menu;
        }
        [CustomeAuthorize("Admin")]
        public ActionResult adminDeleteRole(int roleId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminDeleteRole(roleId, ViewBag.Data);
            return RedirectToAction("adminAccess");
        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult editRole(int roleId)
        {
            Role r = _adminRepository.getRoleData(roleId);
            return View(r);
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult editRole(Role r, List<int> menu, int roleId)
        {
           
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.updateRole(r, menu, roleId, ViewBag.Data);

            return RedirectToAction("adminAccess");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult createAdmin()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public List<Role> GetAdminsRoles()
        {
            List<Role> res = _adminRepository.GetAdminsRoles();
            return res;
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult createAdmin(Admin a, string password, List<int> region, string role)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.createAdmin(a, password, region, role, ViewBag.Data);
            return RedirectToAction("adminDashboard");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult adminScheduling()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult insertShift(shiftViewModel s, string checktoggle, int[] dayList)
        {
            
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.insertShift(s, checktoggle, dayList, ViewBag.Data);

            return RedirectToAction("providerSchedulingDayWise");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult providerLocation()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            List<PhysicianLocation> res = _adminRepository.getAllPhysicianLocation();
            return View(res);
        }
        [CustomeAuthorize("Admin")]
      
        public IActionResult partnersPage()
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }


        [CustomeAuthorize("Admin")]
        public IActionResult partnersPageTable(int? healthprofessionId, string? vendor_name, int pagenumber = 1)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (healthprofessionId != null || vendor_name != null)
            {
                model.healthProfessionals = _adminRepository.filterPartnersPage(healthprofessionId, vendor_name);
            }
            else
            {
                model.healthProfessionals = _adminRepository.GetHealthProfessionals();
            }

            model.healthProfessionaltype = _adminRepository.GetAllHealthProfessionalType();
            var count = model.healthProfessionals.Count();
            if (count > 0)
            {
                model.healthProfessionals = model.healthProfessionals.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_partnersPage", model);
        }


  

        [CustomeAuthorize("Admin")]
        public IActionResult addBusiness()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult addBusiness(HealthProfessional h , string Profession)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.addHealthProfessional(h , Profession);
            return RedirectToAction("addBusiness");
        }
        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult editBusiness(int VendorId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
           HealthProfessional h = _adminRepository.GetProfessionInfo(VendorId);
            return View(h);
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult editBusinessPost(int VendorId, HealthProfessional h)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
             _adminRepository.editBusinessPost(VendorId,h);
            return RedirectToAction("partnersPage");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult adminDeletePartner(int VendorId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.adminDeletePartner(VendorId, ViewBag.Data);
            return RedirectToAction("partnersPage");
        }
        [CustomeAuthorize("Admin")]
      
        public IActionResult blockedHistory()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");         
            return View();
        }

        public IActionResult blockedHistoryTable(string? patientName, DateOnly? date, string? email, string? phone, int pagenumber = 1)
        {
            
            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            model.requestClients = _adminRepository.GetAllRequestClient();
            if (patientName != null || date != null || email != null || phone != null)
            {
                model.blockRequests = _adminRepository.filterBlockedHistory(patientName, date, email, phone);
            }
            else
            {
                model.blockRequests = _adminRepository.GetAllBlockRequests();
            }


            var count = model.blockRequests.Count();
            if (count > 0)
            {
                model.blockRequests = model.blockRequests.Skip((pagenumber - 1) * 3).Take(3).ToList();
                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_blockedHistory", model);
        }


        [CustomeAuthorize("Admin")]
        [HttpPost]
    
        public IActionResult blockedHistory(string patientName, DateOnly date, string email, string phone)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            List<BlockRequest> res = _adminRepository.filterBlockedHistory(patientName, date, email, phone);
            return View(res);
        }
        [CustomeAuthorize("Admin")]
        public IActionResult unblockPatient(int RequestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.unblockPatient(RequestId, ViewBag.Data);
            return RedirectToAction("blockedHistory");
        }
       

        public IActionResult clearBtnSearch(string id)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            if(id == "clearBtnPatientHistory")
            {
                return RedirectToAction("patientHistory");
            }
            else if(id == "clearBtnSearchRecords")
            {
                return RedirectToAction("searchRecords");
               
            }
            else if (id == "clearBtnUserAccess")
            {
                return RedirectToAction("userAccess");

            }
            else if(id == "clearBtnSMSLogs")
            {
                return RedirectToAction("SMSLogs");
             
            }
            else if(id == "clearBtnEmailLogs")
            {
                return RedirectToAction("emailLogs");

            }
            else
            {
                return RedirectToAction("blockedHistory");

            }

        }
        [CustomeAuthorize("Admin")]
    
        public IActionResult patientHistory()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        public IActionResult patientHistoryTable(string? patientFirstName, string? patientLastName, string? email, string? phone, int pagenumber = 1)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (patientFirstName != null || patientLastName != null || email != null || phone != null)
            {
                model.users = _adminRepository.filterPatientHistory(patientFirstName, patientLastName, email, phone);
            }
            else
            {
                model.users = _adminRepository.patientHistory();
            }


            var count = model.users.Count();
            if (count > 0)
            {
                model.users = model.users.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_patientHistory", model);
        }

        [CustomeAuthorize("Admin")]
        public IActionResult explorePatientHistory(int UserId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            var res = _adminRepository.explorePatientHistory(UserId);
            return View(res);
        }
        [CustomeAuthorize("Admin")]
        public IActionResult searchRecords()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }



        [CustomeAuthorize("Admin")]
        public IActionResult searchRecordsTable(int? requestStatus, string? patientName, int? requestType, DateOnly? fromDate, DateOnly? toDate, string? providerName, string? email, string? phone, int pagenumber=1)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (requestStatus != 0 || patientName != null || requestType != 0 || fromDate != null || toDate !=null || providerName !=null || email!=null || phone !=null)
            {
                model.records = _adminRepository.filterSearchRecords(ViewBag.Data, requestStatus, patientName, requestType, fromDate, toDate, providerName, email, phone);
            }
            else
            {
                model.records = _adminRepository.searchRecords(ViewBag.Data);
            }
           

            var count = model.records.Count();
            if (count > 0)
            {
                model.records = model.records.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }            
            return PartialView("_searchRecords", model);
        }



        

        public IActionResult deleteRequest(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.deleteRequest(requestId);
            return View("searchRecords");
        }
        [CustomeAuthorize("Admin")]
       
   
        public IActionResult emailLogs()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
           
            return View();
        }
        [CustomeAuthorize("Admin")]

        public IActionResult emailLogsTable(int? role, string? recieverName, string? email,  DateOnly? createdDate, DateOnly? sentDate, int pagenumber = 1)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (role!=null || recieverName != null || email != null  || createdDate != null || sentDate != null)
            {
                model.emailLogs = _adminRepository.filterEmailLogs(role, recieverName, email, createdDate, sentDate);
            }
            else
            {
                model.emailLogs = _adminRepository.emailLogs();
            }

            model.requestClients = _adminRepository.GetAllRequestClient();
            var count = model.emailLogs.Count();
            if (count > 0)
            {
                model.emailLogs = model.emailLogs.Skip((pagenumber - 1) * 3).Take(3).ToList();
                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }         
            return PartialView("_emailLogs", model);
        }


      
        [CustomeAuthorize("Admin")]
        public IActionResult SMSLogs()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View();
        }
        [CustomeAuthorize("Admin")]
        public IActionResult SMSLogsTable(int? role, string? recieverName, string? mobile, DateOnly? createdDate, DateOnly? sentDate, int pagenumber=1)
        {
           
            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (role!= null || recieverName != null || mobile != null || createdDate != null || sentDate != null)
            {
                model.SMSLogs = _adminRepository.filterSMSLogs(role, recieverName, mobile, createdDate, sentDate);
            }
            else
            {
                model.SMSLogs = _adminRepository.SMSLogs();
            }

            model.requestClients = _adminRepository.GetAllRequestClient();
            var count = model.SMSLogs.Count();
            if (count > 0)
            {
                model.SMSLogs = model.SMSLogs.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }
            return PartialView("_SMSLogs", model);
        }
        [CustomeAuthorize("Admin")]
        public IActionResult ProviderSchedulingDayWise(int? regionId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ShiftDetailsModel model = getSchedulingData(regionId);
           
            return View(model);
        }

        [CustomeAuthorize("Admin")]
        public IActionResult ProviderSchedulingWeekWise(int? regionId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ShiftDetailsModel model = getSchedulingData(regionId);

            return View(model);
        }
        [CustomeAuthorize("Admin")]
        public IActionResult ProviderSchedulingMonthWise(int? regionId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            ShiftDetailsModel model = getSchedulingData(regionId);
            return View(model);
        }

        public ShiftDetailsModel getSchedulingData(int? regionId)
        {
            ShiftDetailsModel model = new();
            model.physicians = _adminRepository.GetAllPhysicians();
            model.regions = _adminRepository.getAllRegions();
            model.shiftDetails = _adminRepository.getshiftDetail(regionId);
           
            return model;
        }
        public IActionResult _ViewShiftModal()
        {
            return View();
        }
        public IActionResult _AddShiftModal()
        {
            return View();
        }
        [HttpGet]
        public IActionResult _ViewShiftModal(int id)
        {
            ShiftDetailsModel res =_adminRepository.getViewShiftData(id);
            return View(res);
        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult UpdateShiftDetailData(ShiftDetailsModel model)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.UpdateShiftDetailData(model,ViewBag.Data);
            return RedirectToAction(nameof(ProviderSchedulingDayWise));
        }
        [CustomeAuthorize("Admin")]
        public IActionResult DeleteShiftDetails(int id)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            _adminRepository.DeleteShiftDetails(id, ViewBag.Data);
            return RedirectToAction(nameof(ProviderSchedulingDayWise));
        }
        [CustomeAuthorize("Admin")]
        public IActionResult ApproveShift(string[] selectedShifts)
        {

            _adminRepository.ApproveShift(selectedShifts);
            return RedirectToAction("ShiftForReview");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult DeleteShift(string[] selectedShifts)
        {
            _adminRepository.DeleteShift(selectedShifts);
            return RedirectToAction("ShiftForReview");
        }
        [CustomeAuthorize("Admin")]
        public IActionResult UpdateShiftStatus(int id)
        {
            _adminRepository.UpdateShiftDetailsStatus(id)
;
            return RedirectToAction(nameof(ProviderSchedulingDayWise));
        }
        [CustomeAuthorize("Admin")]
        public IActionResult ShiftForReview(int reg = 0)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            return View(_adminRepository.getReviewShiftData(reg));
        }
        [CustomeAuthorize("Admin")]
        public void SchedulingMonth(string month)
        {
            int monthNum = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;

            _adminRepository.SchedulingMonth(monthNum);

        }


        [CustomeAuthorize("Admin")]
        public IActionResult ProviderOnCall(int reg = 0)
        {
            ShiftDetailsModel s = _adminRepository.getProviderOnCall(reg);
            return View(s);
        }
        [CustomeAuthorize("Admin")]
        public IActionResult userAccess()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");       
            return View();
        }

        [CustomeAuthorize("Admin")]
        [HttpGet]
        public IActionResult adminEditAccount(string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            Admin a = new Admin();
            a = _adminRepository.getAdminInfo(email);
            return View(a);

        }



        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminEditProfileUpdatePassword(string password, string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminProfileUpdatePassword(email, password);
            return RedirectToAction("adminEditAccount");

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminEditProfileUpdateStatus(Admin a, string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminProfileUpdateStatus(email, a);
            return RedirectToAction("adminEditAccount", new { email = a.Email });

        }
        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminEditProfile(Admin a, string? uncheckedCheckboxes, string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminUpdateProfile(email, a, uncheckedCheckboxes);
            return RedirectToAction("adminEditAccount", new { email = a.Email });

        }

        [CustomeAuthorize("Admin")]
        [HttpPost]
        public IActionResult adminEditProfileBilling(Admin a, string email)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");

            _adminRepository.adminUpdateProfileBilling(email, a);
            return RedirectToAction("adminEditAccount", new { email = a.Email });

        }



        [CustomeAuthorize("Admin")]
        public IActionResult userAccessTable(string? accounttype, int pagenumber = 1)
        {

            ViewBag.Data = HttpContext.Session.GetString("key");
            dashboardTableModel model = new dashboardTableModel();
            if (accounttype != null && accounttype != "Select Account Type")
            {
                model.userAccess = _adminRepository.userAccessSearch(accounttype);
            }
            else
            {
                model.userAccess = _adminRepository.userAccess();
            }


            var count = model.userAccess.Count();
            if (count > 0)
            {
                model.userAccess = model.userAccess.Skip((pagenumber - 1) * 3).Take(3).ToList();

                model.TotalPages = (int)Math.Ceiling((double)count / 3);
                model.CurrentPage = pagenumber;
                model.PreviousPage = pagenumber > 1;
                model.NextPage = pagenumber < model.TotalPages;
            }


            return PartialView("_userAccess", model);
        }


        [CustomeAuthorize("Admin")]
        public List<int> getPhysicianNotification()
        {
            List<int> phy_ids = new List<int>();
            phy_ids = _adminRepository.getPhysicianNotification();
            return phy_ids;
        }
       
        public bool IsUserExists(string email)
        {
          bool isExists =  _adminRepository.IsUserExists(email);
            return isExists;
        }
        public string GetRequestName(int RequestId)
        {
          string name =  _adminRepository.GetRequestName(RequestId);
            return name;
        }
        public IActionResult logOut()
        {

            Response.Cookies.Delete("jwt");
            HttpContext.Session.Remove("key");
            return RedirectToAction("Index");
        }
    }
}













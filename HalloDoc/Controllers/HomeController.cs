using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
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
using Twilio.TwiML.Messaging;

namespace HalloDoc.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPatientRepository _patientRepository;
        private readonly ApplicationDbContext _context;
        private readonly IAuthenticateRepository _authenticate;


        public HomeController(ILogger<HomeController> logger, IPatientRepository patientRepository, ApplicationDbContext context, IAuthenticateRepository authenticate)
        {
            _logger = logger;
            _patientRepository = patientRepository;
            _context = context;
            _authenticate = authenticate;


        }


        /*-----------------------------------Index--------------------------------------------------*/
      
        public IActionResult Index()
        {
           

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AspNetUser user)
        {
          

            var data = _patientRepository.ValidateUser(user.Email, user.PasswordHash);

            if (data == null)
            {

                TempData["Error"] = true;



                return View("Index", user);


            }
            AspNetUser loginuser = new()
            {
                Email = data.Email,
                UserName = data.UserName,
                Id = data.Id
            };

            var jwttoken = _authenticate.GenerateJwtToken(loginuser,"Patient");
            Response.Cookies.Append("jwt", jwttoken);
            HttpContext.Session.SetString("key", user.Email);
           
            return RedirectToAction("patientDashboard");
        }


        /*-----------------------------------Forgot Password--------------------------------------------------*/
       
        public IActionResult Forgotpassword()
        {
            return View();
        }
       
        [HttpPost]
        public IActionResult Reset(PatientLoginVM obj)
        {
            AspNetUser aspNetUser = _patientRepository.GetUserByEmail(obj.Email);
            if (aspNetUser == null)
            {
               
                return RedirectToAction("Forgotpassword");
            }
            else
            {
                string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";

                string senderPassword = "Disney@20";
                string Token = Guid.NewGuid().ToString();
                string resetLink = $"{Request.Scheme}://{Request.Host}/Home/Resetpassword?token={Token}";

                Passwordreset temp = _context.Passwordresets.Where(x => x.Email == obj.Email).FirstOrDefault();



                if (temp != null)
                {
                    temp.Token = Token;
                    temp.Createddate = DateTime.Now;
                    temp.Isupdated = new BitArray(1);
                }
                else
                {
                   
                    Passwordreset passwordReset = new Passwordreset();
                    passwordReset.Token = Token;
                    passwordReset.Email = obj.Email;
                    passwordReset.Isupdated = new BitArray(1);
                    passwordReset.Createddate = DateTime.Now;
                    _context.Passwordresets.Add(passwordReset);
                }
                _context.SaveChanges();


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

                mailMessage.To.Add(obj.Email);

                client.SendMailAsync(mailMessage);

                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        //[Route("Patient/ResetPassword/{token}")]
        public IActionResult ResetPassword(string token)
        {
            var passwordReset = _context.Passwordresets.Where(u => u.Token == token).FirstOrDefault();

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
        //[Route("/ResetPassword/{token}")]

        public IActionResult ResetPassword(ResetPasswordVM obj)
        {
            var passwordReset = _context.Passwordresets.Where(u => u.Token == obj.Token).FirstOrDefault();

            AspNetUser aspNetUser = _patientRepository.GetUserByEmail(obj.Email);

            if (aspNetUser != null)
            {
                aspNetUser.PasswordHash = obj.Password;
                passwordReset.Isupdated = new BitArray(1, true);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }


        /*-----------------------------------Create Patient Account--------------------------------------------------*/
        public IActionResult createPatientAccount()
        {
            return View();
        }

     


        [HttpPost]
        public IActionResult CheckEmail(string email)
        {
            bool emailExists;
            var data = _patientRepository.GetUserByEmail(email);
            if (data == null)
            {
                emailExists = false;
            }
            else
            {
                emailExists = true;
            }


            return new OkObjectResult(new { exists = emailExists });
        }

        /*-----------------------------------Patient Dashboard--------------------------------------------------*/
        [CustomeAuthorize("Patient")]
        public IActionResult patientDashboard()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //if (ViewBag.Data == null)
            //{
            //    return RedirectToAction(nameof(Index));
            //}
            var res = _patientRepository.GetbyEmail(ViewBag.Data);
            return View(res);
        }
        /*-----------------------------------Request Me Form --------------------------------------------------*/
        [CustomeAuthorize("Patient")]
        public IActionResult requestMe()
        {
            return View();
        }

        [HttpPost]
        [CustomeAuthorize("Patient")]
        public IActionResult requestMe(createPatientRequest RequestData)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
           
            _patientRepository.createPatientRequestMe(RequestData);
            return RedirectToAction(nameof(requestMe));
        }


        /*-----------------------------------request Someone Else Form--------------------------------------------------*/
        [CustomeAuthorize("Patient")]
        public IActionResult requestSomeoneElse()
        {

            return View();
        }

        [HttpPost]
        [CustomeAuthorize("Patient")]
        public IActionResult requestSomeoneElse(requestSomeoneElse r)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //if (ViewBag.Data == null)
            //{
            //    return RedirectToAction(nameof(Index));
            //}
            ViewBag.Data = HttpContext.Session.GetString("key");
            _patientRepository.createPatientRequestSomeoneElse(ViewBag.Data ,r);
            return RedirectToAction(nameof(requestSomeoneElse));
        }


        /*-----------------------------------patient site--------------------------------------------------*/
        public IActionResult patientSite()
        {
            return View();
        }
        /*-----------------------------------Patient Submit Screen--------------------------------------------------*/
        public IActionResult patientSubmitRequestScreen()
        {
            return View();
        }
        /*-----------------------------------create patient request--------------------------------------------------*/
        public IActionResult createPatientRequest()
        {
            return View();
        }



        [HttpPost]
        public IActionResult createPatientRequest(patientInfo RequestData)
        {
            

            TempData["ShowSuccessAlert"] = true;
            _patientRepository.CreateRequest(RequestData);
            return RedirectToAction(nameof(createPatientRequest));
        }

        /*-----------------------------------create family request--------------------------------------------------*/
        public IActionResult familyCreateRequest()
        {
           
            return View();
        }
        [HttpPost]
        public IActionResult familyCreateRequest(patientInfo RequestData)
        {
            

            var row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            var res = _patientRepository.CreateFamilyRequest(RequestData);
            if(row==null)
            {
                SendEmailUser(RequestData.Email, res);
            }
            TempData["ShowSuccessAlert"] = true;
            return RedirectToAction(nameof(familyCreateRequest));
        }
        /*-----------------------------------create concierge request--------------------------------------------------*/
        public IActionResult conciergePatientRequest()
        {
            return View();
        }

        [HttpPost]
        public IActionResult conciergePatientRequest(patientInfo RequestData)
        {
           
            var row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            var res = _patientRepository.CreateConciergeRequest(RequestData);
            if (row == null)
            {
                SendEmailUser(RequestData.Email, res);
            }
            TempData["ShowSuccessAlert"] = true;
            return RedirectToAction(nameof(conciergePatientRequest));
        }
        /*-----------------------------------create business request--------------------------------------------------*/
        public IActionResult businessPatientRequest()
        {
            return View();
        }

        [HttpPost]
        public IActionResult businessPatientRequest(patientInfo RequestData)
        {
           
            var row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            var res = _patientRepository.CreateBusinessRequest(RequestData);
            if (row == null)
            {
                SendEmailUser(RequestData.Email, res);
            }
            TempData["ShowSuccessAlert"] = true;
            return RedirectToAction(nameof(businessPatientRequest));
        }



        public Action SendEmailUser(System.String Email, string id)
        {
            
            AspNetUser aspNetUser = _patientRepository.GetUserByEmail(Email);


            string senderEmail = "tatva.dotnet.disneyjaviya@outlook.com";
            string senderPassword = "Disney@20";
            string resetLink = $"{Request.Scheme}://{Request.Host}/Home/createPatientAccount?id={id}";

           
            Passwordreset temp = _context.Passwordresets.Where(x => x.Email == Email).FirstOrDefault();



            if (temp != null)
            {
                temp.Token = id.ToString();
                temp.Createddate = DateTime.Now;
                temp.Isupdated = new BitArray(1);
            }
            else
            {
                Passwordreset passwordReset = new Passwordreset();
                passwordReset.Token = id.ToString();
                passwordReset.Email = Email;
                passwordReset.Isupdated = new BitArray(1);
                passwordReset.Createddate = DateTime.Now;
                _context.Passwordresets.Add(passwordReset);
            }
            _context.SaveChanges();


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
            return null;
        }


        [HttpGet]
        public IActionResult createPatientAccount(string id)
        {
            var passwordReset = _context.Passwordresets.Where(u => u.Token == id).FirstOrDefault();

            if (passwordReset == null)
            {
                return NotFound(); 
            }
            ResetPasswordVM resetPasswordVM = new ResetPasswordVM
            {
                Email = passwordReset.Email,
                Token = id
            };

            TimeSpan difference = (TimeSpan)(DateTime.Now - passwordReset.Createddate);

            if (difference.TotalHours > 24)
            {
                return NotFound(); 
            }

            if (passwordReset.Isupdated.Get(0))
            {
                ModelState.AddModelError("Email", "You can only update once using this link");
                return View(new ResetPasswordVM());
            }

            TempData["success"] = "Enter Password";
            return View(new ResetPasswordVM { Token = id }); 
        }

        [HttpPost]
        public IActionResult createPatientAccount(ResetPasswordVM obj)
        {
            var passwordReset = _context.Passwordresets.Where(u => u.Token == obj.Token).FirstOrDefault();

            AspNetUser aspNetUser = _context.AspNetUsers.Where(x => x.Id == obj.Token).FirstOrDefault();

            if (aspNetUser != null)
            {
                var plainText = Encoding.UTF8.GetBytes(obj.Password);
               var passwordhash = Convert.ToBase64String(plainText);
                aspNetUser.PasswordHash = passwordhash;
                passwordReset.Isupdated = new BitArray(1, true);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }



        /*-----------------------------------View Documents--------------------------------------------------*/
        [CustomeAuthorize("Patient")]
        public IActionResult ViewDocuments(int requestId)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //if (ViewBag.Data == null)
            //{
            //    return RedirectToAction(nameof(Index));
            //}
            var document = _patientRepository.GetDocumentsByRequestId(requestId);
            ViewBag.pname = _patientRepository.getName(requestId.ToString());
            ViewBag.num = _patientRepository.getConfirmationNumber(requestId.ToString());

            if (document == null)
            {
                return NotFound(); 
            }

            return View(document);
        }

        public IActionResult DownloadFile(int fileId)
        {
            var file = _patientRepository.GetFileById(fileId);
            if (file == null)
            {
                return NotFound(); 
            }

            var filePath = Path.Combine("wwwroot/Files", file.FileName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", file.FileName);
        }

        /*-----------------------------------Patient Profile--------------------------------------------------*/
        [CustomeAuthorize("Patient")]
        public IActionResult patientProfile()
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //if (ViewBag.Data == null)
            //{
            //    return RedirectToAction(nameof(Index));
            //}
            
            var res = _patientRepository.GetPatientData(ViewBag.Data);
            return View(res);
        }
        [HttpPost]
        [CustomeAuthorize("Patient")]
        public IActionResult patientProfile(User u)
        {
            ViewBag.Data = HttpContext.Session.GetString("key");
            //if (ViewBag.Data == null)
            //{
            //    return RedirectToAction(nameof(Index));
            //}

            
            _patientRepository.updateProfile(ViewBag.Data, u);
            return View();
        }

        /*-----------------------------------Upload Files--------------------------------------------------*/

        public IActionResult UploadFiles(int requestId, List<IFormFile> files)
        {
          
            _patientRepository.UploadFiles(requestId, files);
            return RedirectToAction("ViewDocuments", new { requestId = requestId });
        }






        public IActionResult DownloadFiles(List<int> fileIds, int? requestId)
        {
            IEnumerable<RequestWiseFile> files;

            if (!fileIds.IsNullOrEmpty())
            {
                files = _patientRepository.GetFilesByIds(fileIds);
            }
            else if (requestId != null)
            {
                files = _patientRepository.GetFilesByRequestId(requestId.Value);
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


        /*-----------------------------------Review Agreement--------------------------------------------------*/
       
        public IActionResult reviewAgreement()
        {
            return View();
        }

      
        public IActionResult reviewAgreementPost(string requestId, string adminId, string physicianId)
        {
            int req = int.Parse(requestId);
            int aid =0;
            int pid=0;
            if (adminId != null)
            {
                 aid = int.Parse(adminId);
                pid = 0;
            }
            if(physicianId != null)
            {
                 pid = int.Parse(physicianId);
                aid = 0;
            }
            _patientRepository.agreementApproved(req, aid, pid);
            return View("Index");
        }

        /*-----------------------------------Logout--------------------------------------------------*/
        public IActionResult logOut()
        {
            Response.Cookies.Delete("jwt");
            HttpContext.Session.Remove("key");
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new DataAccessLayer.DataModels.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
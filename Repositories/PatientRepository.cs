
using Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalloDoc.DataAccessLayer.DataContext;
using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Helpers;

namespace Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
       
        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public AspNetUser ValidateUser(string email, string password)
        {
            var passwordhash = "";
            if (password != null)
            {
                var plainText = Encoding.UTF8.GetBytes(password);
                 passwordhash = Convert.ToBase64String(plainText);
            }
           
            return _context.AspNetUsers.Where(x => x.Email == email && x.PasswordHash == passwordhash).FirstOrDefault();
        }

        public AspNetUser GetUserByEmail(string email)
        {
            return _context.AspNetUsers.Where(x => x.Email == email).FirstOrDefault();
        }

        public void agreementApproved(int requestId)
        {

            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            var res =  _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
           if(res != null)
           {
                res.Status = 4;
               
                _context.SaveChanges();
                rs.RequestId = requestId;
                rs.Status = 4;  
                rs.CreatedDate = DateTime.Now;
            }       
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }





        public void InsertPatientInfo(patientInfo RequestData, AspNetUser asp, User data, Request req, RequestClient rc)
        {
            //AspNetUser asp = new AspNetUser();
            if (asp.UserName == null)
            {
                asp.UserName = RequestData.FirstName + RequestData.LastName;
                asp.Email = RequestData.Email;
                asp.PhoneNumber = RequestData.PhoneNumber;
                asp.CreatedDate = DateTime.Now;
                _context.AspNetUsers.Add(asp);
                _context.SaveChanges();

            }



            //User data = new User();
            if (req.RequestTypeId == 4)
            {
                var c_regionid = _context.Regions.Where(x => x.Name == RequestData.ConciergeCity).Select(u => u.RegionId).FirstOrDefault();
                data.RegionId = c_regionid;
                rc.RegionId = c_regionid;
                data.Street = RequestData.ConciergeStreet;
                data.City = RequestData.ConciergeCity;
                data.State = RequestData.ConciergeState;
                data.ZipCode = RequestData.ConciergeZipCode;

            }
            else
            {
                var regionid = _context.Regions.Where(x => x.Name == RequestData.City).Select(u => u.RegionId).FirstOrDefault();
                data.RegionId = regionid;
                rc.RegionId = regionid;
                data.Street = RequestData.Street;
                data.City = RequestData.City;
                data.State = RequestData.State;
                data.ZipCode = RequestData.ZipCode;

            }

            data.FirstName = RequestData.FirstName;
            data.LastName = RequestData.LastName;
            data.Email = RequestData.Email;
            data.Mobile = RequestData.PhoneNumber;
           
            
            data.CreatedDate = DateTime.Now;

            System.String sDate = RequestData.DateOfBirth.ToString();
            DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));

            int dy = datevalue.Day;
            System.String mn = datevalue.Month.ToString();
            int yy = datevalue.Year;

            data.IntYear = yy;
            data.StrMonth = mn;
            data.IntDate = dy;
            data.Status = 1;

            _context.Users.Add(data);
            _context.SaveChanges();


            //Request req = new Request();
            req.UserId = data.UserId;
            req.Status = 1;
            req.CreatedDate = DateTime.Now;
            int c = _context.Users.Where(x => x.CreatedDate.Date == DateTime.Today).Count();
            if (req.RequestTypeId == 4)
            {
                req.ConfirmationNumber = RequestData.ConciergeState.Substring(0, 2) + DateTime.Now.ToString().Replace("-", "").Substring(0, 4) + RequestData.LastName.Substring(0, 2) + RequestData.FirstName.Substring(0, 2) + c;
            }
            else
            {
                req.ConfirmationNumber = RequestData.State.Substring(0, 2) + DateTime.Now.ToString().Replace("-", "").Substring(0, 4) + RequestData.LastName.Substring(0, 2) + RequestData.FirstName.Substring(0, 2) + c;
            }


            if (RequestData.MultipleFiles != null)
            {
                foreach (var file in RequestData.MultipleFiles)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string fileNameWithPath = Path.Combine(path, file.FileName);
                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }


            _context.Requests.Add(req);
            _context.SaveChanges();
            if (RequestData.MultipleFiles != null)
            {
                foreach (var file in RequestData.MultipleFiles)
                {
                    RequestWiseFile rf = new RequestWiseFile();
                    rf.RequestId = req.RequestId;
                    rf.FileName = file.FileName;
                    rf.CreatedDate = DateTime.Now;
                    rf.IsDeleted = new BitArray(new bool[] { false });
                    _context.RequestWiseFiles.Add(rf);
                    _context.SaveChanges();

                }
            }

            //RequestClient rc = new RequestClient();
            rc.RequestId = req.RequestId;
            rc.FirstName = RequestData.FirstName;
            rc.LastName = RequestData.LastName;
            rc.PhoneNumber = RequestData.PhoneNumber;
            
            rc.Notes = RequestData.Notes;
            if (req.RequestTypeId == 4)
            {
                rc.Location = RequestData.ConciergeCity;
                rc.Street = RequestData.ConciergeStreet;
                rc.City = RequestData.ConciergeCity;
                rc.State = RequestData.ConciergeState;
                rc.ZipCode = RequestData.ConciergeZipCode;
                rc.Address = RequestData.ConciergeStreet + "," + RequestData.ConciergeCity + "," + RequestData.ConciergeState + " ," + RequestData.ConciergeZipCode;

            }
            else
            {
                rc.Location = RequestData.City;
                rc.Address = RequestData.Street + "," + RequestData.City + "," + RequestData.State + " ," + RequestData.ZipCode;
                rc.Street = RequestData.Street;
                rc.City = RequestData.City;
                rc.State = RequestData.State;
                rc.ZipCode = RequestData.ZipCode;
            }
           
            rc.Email = RequestData.Email;
            rc.StrMonth = mn;
            rc.IntDate = dy;
            rc.IntYear = yy;

            _context.RequestClients.Add(rc);
            _context.SaveChanges();

        }

        public void CreateRequest(patientInfo RequestData)
        {
            AspNetUser asp = new AspNetUser();
            User data = new User();
            Request req = new Request();
            RequestClient rc = new RequestClient();

            asp.Id = Guid.NewGuid().ToString();
            if(RequestData.PasswordHash != null)
            {
                var plainText = Encoding.UTF8.GetBytes(RequestData.PasswordHash);
                var passwordhash = Convert.ToBase64String(plainText);
                asp.PasswordHash = passwordhash;
            }
           
            data.AspNetUserId = asp.Id;
            data.CreatedBy = RequestData.FirstName;

            req.RequestTypeId = 2;

            req.FirstName = RequestData.FirstName;
            req.LastName = RequestData.LastName;
            req.PhoneNumber = RequestData.Mobile;
            req.Email = RequestData.Email;


            InsertPatientInfo(RequestData, asp, data, req, rc);
        }


        public string CreateFamilyRequest(patientInfo RequestData)
        {
            AspNetUser asp = new AspNetUser();
            User data = new User();
            Request req = new Request();
            RequestClient rc = new RequestClient();
            var newId = "";
            AspNetUser row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            if (row == null)
            {
                newId = Guid.NewGuid().ToString();
                asp.Id = newId;
                data.AspNetUserId = newId;
            }
            else
            {
                asp = row;
                data.AspNetUserId = row.Id;
            }
            data.CreatedBy = RequestData.FamilyFirstName;
            req.RequestTypeId = 3;

            req.FirstName = RequestData.FamilyFirstName;
            req.LastName = RequestData.FamilyLastName;
            req.PhoneNumber = RequestData.FamilyPhoneNumber;
            req.Email = RequestData.FamilyEmail;



            req.RelationName = RequestData.RelationName;


            InsertPatientInfo(RequestData, asp, data, req, rc);







            return newId;
        }

        public string CreateConciergeRequest(patientInfo RequestData)
        {
            AspNetUser asp = new AspNetUser();
            User data = new User();
            Request req = new Request();
            RequestClient rc = new RequestClient();
            var newId = "";
            AspNetUser row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            if (row == null)
            {
                newId = Guid.NewGuid().ToString();
                asp.Id = newId;
                data.AspNetUserId = newId;
            }
            else
            {
                asp = row;
                data.AspNetUserId = row.Id;
            }
            data.CreatedBy = RequestData.ConciergeFirstName;

            req.RequestTypeId = 4;

            req.FirstName = RequestData.ConciergeFirstName;
            req.LastName = RequestData.ConciergeLastName;
            req.PhoneNumber = RequestData.ConciergePhoneNumber;
            req.Email = RequestData.ConciergeEmail;

            InsertPatientInfo(RequestData, asp, data, req, rc);



            Concierge concierge = new Concierge();
            RequestConcierge requestConcierge = new RequestConcierge();
            concierge.ConciergeName = RequestData.ConciergeFirstName + "  " + RequestData.ConciergeLastName;
            concierge.Address = RequestData.ConciergeStreet + " " + RequestData.ConciergeCity + " " + RequestData.ConciergeState + " " + RequestData.ConciergeZipCode;
            concierge.Street = RequestData.ConciergeStreet;
            concierge.City = RequestData.ConciergeCity;
            concierge.State = RequestData.ConciergeState;
            concierge.ZipCode = RequestData.ConciergeZipCode;
            concierge.CreatedDate = DateTime.Now;
            concierge.Propertyname = RequestData.ConciergePropertyName;
            var regionid = _context.Regions.Where(x => x.Name == RequestData.ConciergeCity).Select(u => u.RegionId).FirstOrDefault();

            concierge.RegionId = regionid;
            //concierge.Propertyname = RequestData.ConciergePropertyName;
            _context.Concierges.Add(concierge);

            _context.SaveChanges();



            requestConcierge.RequestId = req.RequestId;
            requestConcierge.ConciergeId = concierge.ConciergeId;

            _context.RequestConcierges.Add(requestConcierge);
            _context.SaveChanges();
            return newId;
        }

        public string CreateBusinessRequest(patientInfo RequestData)
        {
            AspNetUser asp = new AspNetUser();
            User data = new User();
            Request req = new Request();
            RequestClient rc = new RequestClient();
            var newId = "";
            AspNetUser row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            if (row == null)
            {
                newId = Guid.NewGuid().ToString();
                asp.Id = newId;
                data.AspNetUserId = newId;
            }
            else
            {
                asp = row;
                data.AspNetUserId = row.Id;
            }
            data.CreatedBy = RequestData.BusinessFirstName;

            req.RequestTypeId = 1;

            req.FirstName = RequestData.BusinessFirstName;
            req.LastName = RequestData.BusinessLastName;
            req.PhoneNumber = RequestData.BusinessPhoneNumber;
            req.Email = RequestData.BusinessEmail;

            InsertPatientInfo(RequestData, asp, data, req, rc);

            return newId;
        }











        public List<Request> GetbyEmail(string email)
        {
        
           var userIds = _context.Users.Where(x => x.Email == email).Select(u => u.UserId).ToList();
            var userData = new List<Request>();
            foreach (var userId in userIds)
            {
                 userData.AddRange( _context.Requests.Where(ud => ud.UserId == userId).ToList());
            }
            return userData;
        }

        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId)
        {
            return _context.RequestWiseFiles.Where(d => d.RequestId == requestId).ToList();
        }

        public void UploadFiles(int requestId, List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine("wwwroot/Files", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    RequestWiseFile newFile = new RequestWiseFile
                    {
                        FileName = fileName,
                        RequestId = requestId,
                        CreatedDate = DateTime.Now,
                        IsDeleted = new BitArray(new bool[] { false })
                };

                    _context.RequestWiseFiles.Add(newFile);
                }
            }

            _context.SaveChanges();
        }

        public RequestWiseFile GetFileById(int fileId)
        {
            return _context.RequestWiseFiles.FirstOrDefault(f => f.RequestWiseFileId == fileId);
        }


        public User GetPatientData(string email)
        {

            var data = _context.Users.Where(x => x.Email == email).ToList().First();
            
            return data;
        }

        public void updateProfile(string email, User u)
        {

            var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(a=>a.Id).First();

           var listUsers = _context.Users.Where(x=>x.AspNetUserId==aspId).ToList();
            if(listUsers!=null)
            {
                foreach (var user in listUsers)
                {

                    user.FirstName = u.FirstName;
                    user.LastName = u.LastName;
                    user.Email = u.Email;
                    user.Street = u.Street;
                    user.City = u.City;
                    user.State = u.State;
                    user.ZipCode = u.ZipCode;
                    int rid = _context.Regions.Where(x=>x.Name == u.City).Select(x=>x.RegionId).FirstOrDefault();
                    user.RegionId = rid;
                    _context.SaveChanges();


                    var req = _context.Requests.Where(x => x.Email == user.Email).ToList();
                    foreach (var r in req)
                    {
                        r.FirstName = u.FirstName;
                        r.LastName = u.LastName;
                        r.Email = u.Email;
                        var c = _context.Users.Count(x => x.CreatedDate == DateTime.Now);
                        req.First().ConfirmationNumber = u.State.Substring(0, 2) + DateTime.Now.ToString().Substring(0, 4) + u.LastName.Substring(0, 2) + u.FirstName.Substring(0, 2) + c;
                        _context.SaveChanges();
                    }

                    var reqc = _context.RequestClients.Where(x => x.Email == user.Email).ToList();
                    foreach (var rc in reqc)
                    {
                        rc.FirstName = u.FirstName;
                        rc.LastName = u.LastName;
                        rc.PhoneNumber = u.Mobile;
                        rc.Location = u.State;
                        rc.Address = u.Street + "," + u.City + "," + u.State + " ," + u.ZipCode;
                        rc.Email = u.Email;


                        rc.RegionId = rid;
                        rc.Street = u.Street;
                        rc.City = u.City;
                        rc.State = u.State;
                        rc.ZipCode = u.ZipCode;
                        _context.SaveChanges();

                    }
                    var asp = _context.AspNetUsers.Where(x=>x.Id== aspId).ToList().First();
                    asp.Email = u.Email;
                    asp.UserName =  u.FirstName + u.LastName;
                    asp.PhoneNumber = u.Mobile;
                    

                }
            }




        }




        public string getName(string requestId)
        {
            int reqId = int.Parse(requestId);
            string name = _context.RequestClients.Where(x => x.RequestId == reqId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();

            return name;
        }

        public string getConfirmationNumber(string requestId)
        {
            int reqId = int.Parse(requestId);
            string number = _context.Requests.Where(x => x.RequestId == reqId).Select(u => u.ConfirmationNumber).FirstOrDefault();
            return number;
        }

        public IEnumerable<RequestWiseFile> GetAllFiles()
        {
            return _context.RequestWiseFiles.ToList();
        }
        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId)
        {
            return _context.RequestWiseFiles.Where(f => f.RequestId == requestId).ToList();
        }

        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds)
        {
            return _context.RequestWiseFiles.Where(f => fileIds.Contains(f.RequestWiseFileId)).ToList();
        }


        public void createPatientRequestMe(createPatientRequest RequestData)
        {

      
            User data = new User();
            var d = _context.AspNetUsers.FirstOrDefault(x => x.Email == RequestData.Email);
          
            data.AspNetUserId = d.Id;
            
         
            
            data.FirstName = RequestData.FirstName;
            data.LastName = RequestData.LastName;
            data.Email = RequestData.Email;
            data.Mobile = RequestData.Mobile;
            data.Street = RequestData.Street;
            data.City = RequestData.City;
            data.State = RequestData.State;
            data.ZipCode = RequestData.ZipCode;
            data.CreatedBy = RequestData.FirstName;
            data.CreatedDate = DateTime.Now;

            System.String sDate = RequestData.DateOfBirth.ToString();
            DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));

            int dy = datevalue.Day;
            System.String mn = datevalue.Month.ToString();
            int yy = datevalue.Year;

            data.IntYear = yy;
            data.StrMonth = mn;
            data.IntDate = dy;
            data.Status = 1;

            _context.Users.Add(data);
            _context.SaveChanges();


            Request req = new Request();
            req.RequestTypeId = 2;
            req.UserId = data.UserId;
            req.FirstName = RequestData.FirstName;
            req.LastName = RequestData.LastName;
            req.PhoneNumber = RequestData.Mobile;
            req.Email = RequestData.Email;
            req.Status = 1;
            var c = _context.Users.Count(x => x.CreatedDate == DateTime.Now);
            req.ConfirmationNumber = RequestData.State.Substring(0, 2) + DateTime.Now.ToString().Substring(0, 4) + RequestData.LastName.Substring(0, 2) + RequestData.FirstName.Substring(0, 2) + c;
            req.CreatedDate = DateTime.Now;
            if (RequestData.MultipleFiles != null)
            {
                foreach (var file in RequestData.MultipleFiles)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string fileNameWithPath = Path.Combine(path, file.FileName);
                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }


            _context.Requests.Add(req);
            _context.SaveChanges();
            if (RequestData.MultipleFiles != null)
            {
                foreach (var file in RequestData.MultipleFiles)
                {
                    RequestWiseFile rf = new RequestWiseFile();
                    rf.RequestId = req.RequestId;
                    rf.FileName = file.FileName;
                    rf.CreatedDate = DateTime.Now;
                    rf.IsDeleted = new BitArray(new bool[] { false });
                    _context.RequestWiseFiles.Add(rf);
                    _context.SaveChanges();

                }
            }



            RequestClient rc = new RequestClient();
            rc.RequestId = req.RequestId;
            rc.FirstName = RequestData.FirstName;
            rc.LastName = RequestData.LastName;
            rc.PhoneNumber = RequestData.Mobile;
            rc.Location = RequestData.State;
            rc.Address = RequestData.Street + "," + RequestData.City + "," + RequestData.State + " ," + RequestData.ZipCode;
            rc.Notes = RequestData.Symptoms;
            rc.Email = RequestData.Email;
            rc.StrMonth = mn;
            rc.IntDate = dy;
            rc.IntYear = yy;
            rc.Street = RequestData.Street;
            rc.City = RequestData.City;
            rc.State = RequestData.State;
            rc.ZipCode = RequestData.ZipCode;




            _context.RequestClients.Add(rc);
            _context.SaveChanges();


        }

        public void createPatientRequestSomeoneElse(string email ,requestSomeoneElse r)
        {
       
            var existUser =  _context.AspNetUsers.FirstOrDefault(x => x.Email == r.Email);
            if(existUser!=null)
            {
                /*User already exists*/
                User data = new User();
                data.AspNetUserId = existUser.Id;
                data.FirstName = r.FirstName;
                data.LastName = r.LastName;
                data.Email = r.Email;
                data.Mobile = r.Mobile;
                data.Street = r.Street;
                data.City = r.City;
                data.State = r.State;
                data.ZipCode = r.ZipCode;
                data.CreatedBy = r.FirstName;
                data.CreatedDate = DateTime.Now;
                System.String sDate = r.DateOfBirth.ToString();
                DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));

                int dy = datevalue.Day;
                System.String mn = datevalue.Month.ToString();
                int yy = datevalue.Year;

                data.IntYear = yy;
                data.StrMonth = mn;
                data.IntDate = dy;
                data.Status = 1;

                _context.Users.Add(data);
                _context.SaveChanges();
              
                var emailUser = _context.Users.FirstOrDefault(x => x.Email == email);
                Request req = new Request();
                req.RequestTypeId = 2;
                req.UserId = data.UserId;
                req.FirstName = emailUser.FirstName;
                req.LastName = emailUser.LastName;
                req.PhoneNumber = emailUser.Mobile;
                req.Email = emailUser.Email;
                req.Status = 1;
                var c = _context.Users.Count(x => x.CreatedDate == DateTime.Now);
                req.ConfirmationNumber = r.State.Substring(0, 2) + DateTime.Now.ToString().Substring(0, 4) + r.LastName.Substring(0, 2) + r.FirstName.Substring(0, 2) + c;
                req.CreatedDate = DateTime.Now;
                if (r.MultipleFiles != null)
                {
                    foreach (var file in r.MultipleFiles)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string fileNameWithPath = Path.Combine(path, file.FileName);
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }
                }

                _context.Requests.Add(req);
                _context.SaveChanges();
                if (r.MultipleFiles != null)
                {
                    foreach (var file in r.MultipleFiles)
                    {
                        RequestWiseFile rf = new RequestWiseFile();
                        rf.RequestId = req.RequestId;
                        rf.FileName = file.FileName;
                        rf.CreatedDate = DateTime.Now;
                        rf.IsDeleted = new BitArray(new bool[] { false });
                        _context.RequestWiseFiles.Add(rf);
                        _context.SaveChanges();

                    }
                }

                RequestClient rc = new RequestClient();
                rc.RequestId = req.RequestId;
                rc.FirstName = r.FirstName;
                rc.LastName = r.LastName;
                rc.PhoneNumber = r.Mobile;
                rc.Location = r.State;
                rc.Address = r.Street + "," + r.City + "," + r.State + " ," + r.ZipCode;
                rc.Notes = r.Symptoms;
                rc.Email = r.Email;
                rc.StrMonth = mn;
                rc.IntDate = dy;
                rc.IntYear = yy;
                rc.Street = r.Street;
                rc.City = r.City;
                rc.State = r.State;
                rc.ZipCode = r.ZipCode;




                _context.RequestClients.Add(rc);
                _context.SaveChanges();

            }
            else
            {
                /*User does not exists*/
                AspNetUser asp = new AspNetUser();
                asp.Id = Guid.NewGuid().ToString();
                asp.UserName = r.FirstName + r.LastName;
                asp.Email = r.Email;
                asp.PhoneNumber = r.Mobile;
                asp.CreatedDate = DateTime.Now;

                _context.AspNetUsers.Add(asp);
                _context.SaveChanges();

                User data = new User();
                data.AspNetUserId = asp.Id;
                data.FirstName = r.FirstName;
                data.LastName = r.LastName;
                data.Email = r.Email;
                data.Mobile = r.Mobile;
                data.Street = r.Street;
                data.City = r.City;
                data.State = r.State;
                data.ZipCode = r.ZipCode;
                data.CreatedBy = r.FirstName;
                data.CreatedDate = DateTime.Now;

                System.String sDate = r.DateOfBirth.ToString();
                DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));

                int dy = datevalue.Day;
                System.String mn = datevalue.Month.ToString();
                int yy = datevalue.Year;

                data.IntYear = yy;
                data.StrMonth = mn;
                data.IntDate = dy;
                data.Status = 1;

                _context.Users.Add(data);
                _context.SaveChanges();


                var emailUser = _context.Users.FirstOrDefault(x => x.Email == email);
                Request req = new Request();
                req.RequestTypeId = 2;
                req.UserId = data.UserId;
                req.FirstName = emailUser.FirstName;
                req.LastName = emailUser.LastName;
                req.PhoneNumber = emailUser.Mobile;
                req.Email = emailUser.Email;
                req.Status = 1;
                var c = _context.Users.Count(x => x.CreatedDate == DateTime.Now);
                req.ConfirmationNumber = r.State.Substring(0, 2) + DateTime.Now.ToString().Substring(0, 4) + r.LastName.Substring(0, 2) + r.FirstName.Substring(0, 2) + c;
                req.CreatedDate = DateTime.Now;
                if (r.MultipleFiles != null)
                {
                    foreach (var file in r.MultipleFiles)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string fileNameWithPath = Path.Combine(path, file.FileName);
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }
                }

                _context.Requests.Add(req);
                _context.SaveChanges();
                if (r.MultipleFiles != null)
                {
                    foreach (var file in r.MultipleFiles)
                    {
                        RequestWiseFile rf = new RequestWiseFile();
                        rf.RequestId = req.RequestId;
                        rf.FileName = file.FileName;
                        rf.CreatedDate = DateTime.Now;
                        rf.IsDeleted = new BitArray(new bool[] { false });
                        _context.RequestWiseFiles.Add(rf);
                        _context.SaveChanges();

                    }
                }



                RequestClient rc = new RequestClient();
                rc.RequestId = req.RequestId;
                rc.FirstName = r.FirstName;
                rc.LastName = r.LastName;
                rc.PhoneNumber = r.Mobile;
                rc.Location = r.State;
                rc.Address = r.Street + "," + r.City + "," + r.State + " ," + r.ZipCode;
                rc.Notes = r.Symptoms;
                rc.Email = r.Email;
                rc.StrMonth = mn;
                rc.IntDate = dy;
                rc.IntYear = yy;
                rc.Street = r.Street;
                rc.City = r.City;
                rc.State = r.State;
                rc.ZipCode = r.ZipCode;




                _context.RequestClients.Add(rc);
                _context.SaveChanges();


            }
        }






    }



}

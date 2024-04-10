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
using System.Web.Http;
using System.IO.Compression;
using System.Web.Helpers;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.ExtendedProperties;
using Twilio.TwiML.Voice;


namespace Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
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

        public RequestClient getPatientInfo(int requestId)
        {
            return _context.RequestClients.FirstOrDefault(x => x.RequestId == requestId);
        }
        public int GetUserByRequestId(string Id)
        {
            var user = _context.Users.Where(x => x.AspNetUserId == Id).Select(u=>u.UserId).FirstOrDefault();
            return _context.Requests.Where(u=>u.UserId == user).Select(x=>x.RequestId).FirstOrDefault();
        }
        public string getConfirmationNumber(int requestId)
        {
            return _context.Requests.Where(r => r.RequestId == requestId).Select(r => r.ConfirmationNumber).FirstOrDefault();
        }




        public viewNotes getNotes(int requestId, string email)
        {
            var res = _context.RequestNotes.FirstOrDefault(x => x.RequestId == requestId);
            var res1 = _context.RequestStatusLogs.Where(x => x.RequestId == requestId).ToList();
            viewNotes v = new viewNotes();
            if (res == null)
            {

                v.RequestId = requestId;
                v.AdditionalNote = "";
                _context.SaveChanges();
            }
            else
            {
                v.RequestId = res.RequestId;
                v.AdminNote = res.AdminNotes;
                _context.SaveChanges();
            }
            foreach (var row in res1)
            {
                if (row.Status == 3)
                {
                    v.AdminCancellationNotes = "Cancelled: " +   row.Notes;
                }
                else if (row.Status == 2)
                {
                    //admin tranfered to some physician
                    if (row.TransToAdmin == null)
                    {
                        var adminAspId = _context.Admins.Where(x => x.AdminId == row.AdminId).Select(u => u.AspNetUserId).FirstOrDefault();
                        var adminusername = _context.AspNetUsers.Where(x => x.Id == adminAspId).Select(u => u.UserName).FirstOrDefault();
                        var physicianname = _context.Physicians.Where(x => x.PhysicianId == row.TransToPhysicianId).Select(u => u.FirstName).FirstOrDefault();
                        v.TransferNote = v.TransferNote + adminusername + " " + "transfered to " + physicianname + " : " + row.Notes;
                    }
                    //physician transfered to admin
                    if (row.TransToPhysicianId == null)
                    {
                        var physicianusername = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                        v.TransferNote = physicianusername + "tranfered to" + row.AdminId + ":" + row.Notes;
                    }
                }
                else if (row.Status == 7)
                {
                    v.PatientCancellationNotes = _context.RequestStatusLogs.Where(x => x.RequestId == requestId && x.Status == 7).Select(u => u.Notes).FirstOrDefault();
                }
            }


            return v;
        }

        public void adminNotes(int requestId, viewNotes v, string email)
        {
            RequestNote rn = new RequestNote();
            var res = _context.RequestNotes.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (res == null)
            {
                rn.RequestId = requestId;
                rn.AdminNotes = v.AdditionalNote;
                rn.PhysicianNotes = null;
                rn.CreatedBy = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                rn.CreatedDate = DateTime.Now;
                _context.RequestNotes.Add(rn);
                _context.SaveChanges();
            }
            else
            {

                var r = _context.RequestNotes.Where(u => u.RequestNotesId == res.RequestNotesId).FirstOrDefault();
                r.RequestNotesId = r.RequestNotesId;
                r.AdminNotes = v.AdditionalNote;
                r.PhysicianNotes = null;
                if (r.CreatedBy == "")
                {
                    r.CreatedBy = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                    r.CreatedDate = DateTime.Now;
                }
                else
                {
                    r.ModifiedBy = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                    r.ModifiedDate = DateTime.Now;

                }


                _context.SaveChanges();

            }


        }

        public void adminCancelNote(string requestId, string reason, string additionalNotes, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            int reqId = int.Parse(requestId);
            //int CaseTagid = int.Parse(reason);
            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (reqId != null)
            {
                res.Status = 3;
                //var caseName = _context.CaseTags.Where(x => x.CaseTagId == CaseTagid).Select(u => u.Name).ToString();
                res.CaseTag = reason;
                res.DeclinedBy = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 3;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.Notes = additionalNotes;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
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

        public List<Physician> GetPhysicians(int regionId)
        {
            return _context.Physicians.Where(x => x.RegionId == regionId).ToList();
        }


        public void adminAssignNote(string requestId, string region, string physician, string additionalNotesAssign, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            int reqId = int.Parse(requestId);

            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (res != null && physician != "Select Physician" && region != "Select Region")
            {
                res.Status = 2;
                res.PhysicianId = int.Parse(physician);

                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 2;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.TransToPhysicianId = int.Parse(physician);
                rs.Notes = additionalNotesAssign;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }

        public void adminBlockNote(string requestId, string additionalNotesBlock, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            BlockRequest b = new BlockRequest();
            int reqId = int.Parse(requestId);
            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            string patient_email = _context.RequestClients.Where(x=>x.RequestId == reqId).Select(u=>u.Email).FirstOrDefault();
            
            if (res != null)
            {
                rs.RequestId = reqId;
                res.Status = 11;
                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 11;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.Notes = additionalNotesBlock;
                rs.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(rs);
                _context.SaveChanges();
                b.PhoneNumber = res.PhoneNumber;
                b.Email = patient_email;
                b.Reason = additionalNotesBlock;
                b.RequestId = reqId.ToString();
                b.CreatedDate = DateTime.Now;
                b.IsActive = new BitArray(new bool[] { false });
                _context.BlockRequests.Add(b);
                _context.SaveChanges();

            }
           
           
        }


        public void adminTransferCase(string requestId, string physician, string additionalNotesTransfer, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            int reqId = int.Parse(requestId);
            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (res != null)
            {
                res.Status = 2;
                res.PhysicianId = int.Parse(physician);

                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 2;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.TransToPhysicianId = int.Parse(physician);
                rs.Notes = additionalNotesTransfer;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }


        public void patientCancelNote(string requestId, string additionalNotesPatient)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            int reqId = int.Parse(requestId);

            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (res != null)
            {
                res.Status = 7;
                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 7;
                rs.Notes = additionalNotesPatient;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }


        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId)
        {


            return _context.RequestWiseFiles.Where(d => d.RequestId == requestId && d.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }


        public List<string> GetNameConfirmation(int requestId)
        {
            List<string> result = new List<string>();
            string name = _context.RequestClients.Where(x => x.RequestId == requestId).Select(x => x.FirstName).ToString();
            result.Add(name);
            string Cnum = _context.Requests.Where(x => x.RequestId == requestId).Select(x => x.ConfirmationNumber).ToString();
            result.Add(Cnum);
            return result;
        }

        public IEnumerable<RequestandRequestClient> getRequestStateData(int type)
        {
            var query = (from req in _context.Requests
                         join client in _context.RequestClients on req.RequestId equals client.RequestId
                         select new
                         {
                             Request = req,
                             Client = client,
                             Status = req.Status
                         })
                         .Select(x => new RequestandRequestClient
                         {
                             requestId = x.Client.RequestId,
                             patientName = x.Client.FirstName + " " + x.Client.LastName,
                             patientDOB = x.Client.IntDate + "/" + x.Client.StrMonth + "/" + x.Client.IntYear,
                             requestorName = x.Request.FirstName + " " + x.Request.LastName,
                             requestedDate = x.Request.CreatedDate,
                             patientContact = x.Client.PhoneNumber,
                             requestorContact = x.Request.PhoneNumber,

                             patientAddress = x.Client.Address,
                             patientCity = x.Client.City,
                             physicianName = _context.Physicians.Where(u => u.PhysicianId == x.Request.PhysicianId).Select(u => u.FirstName).FirstOrDefault(),
                             Status = x.Status,
                             RequestTypeId = x.Request.RequestTypeId,
                             patientEmail = x.Client.Email,
                             PhysicianId = x.Request.PhysicianId,
                             CaseTag = _context.CaseTags.ToList(),
                             Region = _context.Regions.ToList(),
                             Physician = _context.Physicians.ToList()
                         });

            if (type == 1)
            {
                query = query.Where(req => req.Status == 1);
            }
            else if (type == 2)
            {
                query = query.Where(req => req.Status == 2);
            }
            else if (type == 3)
            {
                query = query.Where(req => req.Status == 4 || req.Status == 5);
            }
            else if (type == 4)
            {

                query = query.Where(req => req.Status == 6);
            }
            else if (type == 5)
            {
                query = query.Where(req => req.Status == 3 || req.Status == 7 || req.Status == 8);
            }
            else if (type == 6)
            {
                query = query.Where(req => req.Status == 9);
            }


            return query.ToList();
        }
        public List<Region> getAllRegions()
        {
            return _context.Regions.ToList();
        }
        public List<ShiftDetailsModel> getshiftDetail()        {            var data = from sd in _context.ShiftDetails                       join                       s in _context.Shifts on sd.ShiftId equals s.ShiftId                       join phy in _context.Physicians on s.PhysicianId equals phy.PhysicianId                       join reg in _context.Regions on sd.RegionId equals reg.RegionId where sd.IsDeleted == new BitArray(new bool[] {false})                       select new ShiftDetailsModel                       {                           PhysicianName = phy.FirstName + " " + phy.LastName,                           Physicianid = phy.PhysicianId,                           RegionName = reg.Name,                           Status = sd.Status,                           Starttime = sd.StartTime,                           Endtime = sd.EndTime,                           Shiftdate =DateOnly.FromDateTime(sd.ShiftDate),
                                                       Shiftdetailid = sd.ShiftDetailId,                       };            return data.ToList();        }

        public List<RequestandRequestClient> getFilterByRegions(IEnumerable<RequestandRequestClient> r, int regionId)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();
            var region = _context.Regions.Where(x => x.RegionId == regionId).Select(u => u.Name).FirstOrDefault();
            if (regionId == 0)
            {
                return r.ToList();
            }
            foreach (var r2 in r)
            {
                if (r2.patientAddress != null)
                {
                    if (r2.patientAddress.Contains(region))
                    {
                        s.Add(r2);
                    }
                }



            }
            return s;

        }

        public List<RequestandRequestClient> getByRequesttypeId(IEnumerable<RequestandRequestClient> r, int requesttypeId)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();

            foreach (var r2 in r)
            {
                if (r2.RequestTypeId == requesttypeId)
                {
                    s.Add(r2);
                }

            }
            return s;

        }

        public List<RequestandRequestClient> getByRequesttypeIdRegionAndName(IEnumerable<RequestandRequestClient> r, int requesttypeId, int? regionId, string? patient_name)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();

            if (regionId == 0 && patient_name != null)
            {
                foreach (var r2 in r)
                {
                    if (r2.RequestTypeId == requesttypeId && r2.patientName.ToLower().Contains(patient_name.ToLower()))
                    {
                        s.Add(r2);
                    }

                }
                return s;

            }
            else if (patient_name == null && regionId == 0)
            {
                return getByRequesttypeId(r, requesttypeId);
            }
            else if (patient_name == null && regionId != 0)
            {
                var region = _context.Regions.Where(x => x.RegionId == regionId).Select(u => u.Name).FirstOrDefault();
                foreach (var r2 in r)
                {
                    if (r2.RequestTypeId == requesttypeId && r2.patientAddress.Contains(region))
                    {
                        s.Add(r2);
                    }

                }
                return s;
            }

            else
            {
                var region = _context.Regions.Where(x => x.RegionId == regionId).Select(u => u.Name).FirstOrDefault();
                foreach (var r2 in r)
                {
                    if (r2.RequestTypeId == requesttypeId && r2.patientAddress.Contains(region) && r2.patientName.ToLower().Contains(patient_name.ToLower()))
                    {
                        s.Add(r2);
                    }

                }
                return s;
            }



        }
        public List<RequestandRequestClient> getFilterByName(IEnumerable<RequestandRequestClient> r, string patient_name)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();

            foreach (var r2 in r)
            {
                if (patient_name != null)
                {
                    if (r2.patientName.ToLower().Contains(patient_name.ToLower()))
                    {
                        s.Add(r2);
                    }
                }
                if (patient_name == null)
                {
                    return r.ToList();
                }



            }
            return s;
        }


        public List<RequestandRequestClient> getFilterByRegionAndName(IEnumerable<RequestandRequestClient> r, string patient_name, int regionId)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();
            var region = _context.Regions.Where(x => x.RegionId == regionId).Select(u => u.Name).FirstOrDefault();
            if (regionId == 0 && patient_name == null)
            {
                return r.ToList();
            }
            if (regionId != 0 && patient_name == null)
            {
                return getFilterByRegions(r, regionId);
            }
            if (regionId == 0 && patient_name != null)
            {
                return getFilterByName(r, patient_name);
            }
            foreach (var r2 in r)
            {
                if (r2.patientName != null && r2.patientAddress != null)
                {
                    if (r2.patientName.ToLower().Contains(patient_name.ToLower()) && r2.patientAddress.Contains(region))
                    {
                        s.Add(r2);
                    }
                }

            }
            return s;
        }
        public List<RequestandRequestClient> getFilterByRegionsAfter(int regionId, IEnumerable<RequestandRequestClient> r)
        {
            List<RequestandRequestClient> s = new List<RequestandRequestClient>();
            var region = _context.Regions.Where(x => x.RegionId == regionId).Select(u => u.Name).FirstOrDefault();
            foreach (var r2 in r)
            {
                if (r2.patientAddress != null)
                {
                    if (r2.patientAddress.Contains(region))
                    {
                        s.Add(r2);
                    }
                }



            }
            return s;

        }

        public void UploadFiles(int requestId, List<IFormFile> files, string email)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var filePath = System.IO.Path.Combine("wwwroot/Files", fileName);
                    var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                    var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    RequestWiseFile newFile = new RequestWiseFile
                    {
                        FileName = fileName,
                        RequestId = requestId,
                        AdminId = id,
                        IsDeleted = new BitArray(new bool[] { false }),
                        CreatedDate = DateTime.Now
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

        public void DeleteFile(int fileId)
        {
            var file = _context.RequestWiseFiles.Where(f => f.RequestWiseFileId == fileId).ExecuteUpdate(setters => setters.SetProperty(b => b.IsDeleted, new BitArray(new bool[] { true })));

            _context.SaveChanges();
        }

        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId)
        {
            return _context.RequestWiseFiles.Where(f => f.RequestId == requestId && f.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }

        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds)
        {
            return _context.RequestWiseFiles.Where(f => fileIds.Contains(f.RequestWiseFileId) && f.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }


        public void GetFilesByIdsDelete(List<int> fileIds)
        {

            foreach (var id in fileIds)
            {
                var file = _context.RequestWiseFiles.Where(f => f.RequestWiseFileId == id);
                file.ExecuteUpdate(setters => setters.SetProperty(b => b.IsDeleted, new BitArray(new bool[] { true })));
                _context.SaveChanges();
            }


        }

        public void GetFilesByRequestIdDelete(int requestId)
        {
            List<RequestWiseFile> files = _context.RequestWiseFiles.Where(f => f.RequestId == requestId).ToList();
            foreach (var file in files)
            {
                file.IsDeleted = new BitArray(new bool[] { true });
                _context.SaveChanges();
            }


        }
        public string GetPatientEmail(int requestId)
        {
            string email = _context.RequestClients.Where(x => x.RequestId == requestId).Select(f => f.Email).FirstOrDefault().ToString();
            return email;
        }

        public List<string> GetAllFiles(int requestId)
        {
            return _context.RequestWiseFiles.Where(x => x.RequestId == requestId && x.IsDeleted == new BitArray(new bool[] { false })).Select(f => f.FileName).ToList();

        }


        public List<string> GetSelectedFiles(List<int> ids)
        {
            List<string> selectedfilenames = new List<string>();
            foreach (var id in ids)
            {
                var name = _context.RequestWiseFiles
                                  .Where(x => x.RequestWiseFileId == id && x.IsDeleted == new BitArray(new bool[] { false }))
                                  .Select(x => x.FileName)
                                  .FirstOrDefault();
                if (name != null)
                {
                    selectedfilenames.Add(name);
                }
            }

            return selectedfilenames;
        }
        public void sendOrderDetails(int requestId, sendOrder s, string email)
        {
            OrderDetail o = new OrderDetail();
            var htype = s.type;
            o.VendorId = s.hname;
            o.RequestId = requestId;
            o.FaxNumber = s.FaxNumber;
            o.Email = s.Email;
            o.BusinessContact = s.BusinessContact;
            o.Prescription = s.Prescription;
            o.NoOfRefill = s.NoOfRefill;
            o.CreatedDate = DateTime.Now;
            var name = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
            o.CreatedBy = name;
            _context.OrderDetails.Add(o);
            _context.SaveChanges();
        }
        public List<HealthProfessionalType> GetAllHealthProfessionalType()
        {
            return _context.HealthProfessionalTypes.ToList();
        }
        public List<HealthProfessional> GetAllHealthProfessional()
        {
            return _context.HealthProfessionals.ToList();
        }
        public List<HealthProfessional> GetHealthProfessionals()
        {
            return _context.HealthProfessionals.Where(x=>x.IsDeleted == new BitArray(new bool[] {false})).ToList();
        }
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId)
        {
            return _context.HealthProfessionals.Where(x => x.Profession == healthprofessionalId).ToList();
        }
        public HealthProfessional GetProfessionInfo(int vendorId)
        {
            return _context.HealthProfessionals.Where(x => x.VendorId == vendorId).FirstOrDefault();
        }


        public void adminClearCase(string requestId, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();

            int reqId = int.Parse(requestId);
            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (res != null)
            {
                rs.RequestId = reqId;
                res.Status = 10;
                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 10;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;

                rs.CreatedDate = DateTime.Now;




            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }


        public List<string> adminSendAgreementGet(string requestId)
        {
            List<string> res = new List<string>();
            int reqId = int.Parse(requestId);
            string mob = _context.RequestClients.Where(x => x.RequestId == reqId).Select(x => x.PhoneNumber).FirstOrDefault();
            res.Add(mob);
            string mail = _context.RequestClients.Where(x => x.RequestId == reqId).Select(x => x.Email).FirstOrDefault();
            res.Add(mail);
            return res;
        }

        public void closeCaseAdmin(int requestId, string email)
        {

            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();

            var res = _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (res != null)
            {

                res.Status = 9;
                _context.SaveChanges();
                rs.RequestId = requestId;
                rs.Status = 9;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }
        public string adminTransferNotes(string requestId, string email)
        {
            int reqId = int.Parse(requestId);
            var res = getNotes(reqId, email);
            return res.TransferNote;
        }
        public Admin getAdminInfo(string email)
        {
            return _context.Admins.Where(x => x.Email == email).FirstOrDefault();
        }
        public string adminCreateRequest(createAdminRequest RequestData, string email)
        {
            AspNetUser asp = new AspNetUser();
            User data = new User();
            var newId = "";
            var row = _context.AspNetUsers.Where(x => x.Email == RequestData.Email).FirstOrDefault();
            if (row == null)
            {
                newId = Guid.NewGuid().ToString();
                asp.Id = newId;
                asp.Email = RequestData.Email;
                asp.UserName = RequestData.FirstName + RequestData.LastName;
                asp.PhoneNumber = RequestData.Mobile;
                asp.CreatedDate = DateTime.Now;
                _context.AspNetUsers.Add(asp);
                _context.SaveChanges();

                data.AspNetUserId = newId;


            }
            else
            {

                data.AspNetUserId = row.Id;
            }


            data.Email = RequestData.Email;
            data.FirstName = RequestData.FirstName;
            data.LastName = RequestData.LastName;
            data.Mobile = RequestData.Mobile;
            data.Street = RequestData.Street;
            data.City = RequestData.City;
            data.State = RequestData.State;
            data.ZipCode = RequestData.ZipCode;
            System.String sDate = RequestData.DateOfBirth.ToString();
            DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));

            int dy = datevalue.Day;
            System.String mn = datevalue.Month.ToString();
            int yy = datevalue.Year;

            data.IntYear = yy;
            data.StrMonth = mn;
            data.IntDate = dy;

            var admin = _context.Admins.Where(x => x.Email == email).FirstOrDefault();
            data.CreatedBy = admin.FirstName;
            data.CreatedDate = DateTime.Now;
            data.Status = 1;
            _context.Users.Add(data);
            _context.SaveChanges();


            Request req = new Request();
            req.RequestTypeId = 1;
            req.UserId = data.UserId;
            req.FirstName = admin.FirstName;
            req.LastName = admin.LastName;
            req.PhoneNumber = admin.Mobile;
            req.Email = admin.Email;
            req.Status = 1;
            int c = _context.Users.Where(x => x.CreatedDate.Date == DateTime.Today).Count();
            req.ConfirmationNumber = RequestData.State.Substring(0, 2) + DateTime.Now.ToString().Replace("-", "").Substring(0, 4) + RequestData.LastName.Substring(0, 2) + RequestData.FirstName.Substring(0, 2) + c;
            req.CreatedDate = DateTime.Now;



            _context.Requests.Add(req);
            _context.SaveChanges();


            RequestClient rc = new RequestClient();
            rc.RequestId = req.RequestId;
            rc.FirstName = RequestData.FirstName;
            rc.LastName = RequestData.LastName;
            rc.PhoneNumber = RequestData.Mobile;
            rc.Location = RequestData.State;
            rc.Address = RequestData.Street + "," + RequestData.City + "," + RequestData.State + " ," + RequestData.ZipCode;

            rc.Email = RequestData.Email;
            rc.StrMonth = mn;
            rc.IntDate = dy;
            rc.IntYear = yy;
            rc.Street = RequestData.Street;
            rc.City = RequestData.City;
            rc.State = RequestData.State;
            rc.ZipCode = RequestData.ZipCode;
            var regionid = _context.Regions.Where(x => x.Name == RequestData.City).Select(u => u.RegionId).FirstOrDefault();
            rc.RegionId = regionid;
            _context.RequestClients.Add(rc);
            _context.SaveChanges();

            viewNotes v = new viewNotes();
            v.AdditionalNote = RequestData.AdditionalNotes;

            adminNotes(req.RequestId, v, email);
            return newId;
        }

        public void passwordresetInsert(string Email, string id)
        {
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

        }





        public List<Region> getAdminRegions(string email)
        {
            var admin = _context.Admins.FirstOrDefault(x => x.Email == email);
            if (admin == null)
            {
                return new List<Region>();
            }

            var regionIds = _context.AdminRegions
                                    .Where(x => x.AdminId == admin.AdminId)
                                    .Select(x => x.RegionId)
                                    .ToList();

            var regions = _context.Regions.Where(x => regionIds.Contains(x.RegionId)).ToList();
            return regions;
        }


        public void adminProfileUpdatePassword(string email, string password)
        {
            var aspuser = _context.AspNetUsers.Where(x => x.Email == email).First();


            if (aspuser != null)
            {
                var plainText = Encoding.UTF8.GetBytes(password);
                aspuser.PasswordHash = Convert.ToBase64String(plainText);
                _context.SaveChanges();
            }
        }

        public void adminProfileUpdateStatus(string email, Admin a)
        {
            var admin = _context.Admins.Where(x => x.Email == email).FirstOrDefault();


            if (admin != null)
            {


                admin.Status = a.Status;
                admin.RoleId = a.RoleId;
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                admin.ModifiedBy = aid;
                admin.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

            }
        }

        public void adminUpdateProfile(string email, Admin a, string? uncheckedCheckboxes)
        {

            var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(a => a.Id).First();

            var admin = _context.Admins.Where(x => x.AspNetUserId == aspId).FirstOrDefault();
            if (admin != null)
            {

                admin.FirstName = a.FirstName;
                admin.LastName = a.LastName;
                admin.Email = a.Email;
                admin.Mobile = a.Mobile;

                admin.ModifiedBy = admin.AspNetUserId;
                admin.ModifiedDate = DateTime.Now;
                _context.SaveChanges();


                if(uncheckedCheckboxes != null)
                {
                    string[] boxes = uncheckedCheckboxes.Split(',');

                    if (boxes != null)
                    {
                        foreach (var box in boxes)
                        {
                            if (box != "")
                            {
                                int regionid = int.Parse(box);
                                var row = _context.AdminRegions.Where(x => x.AdminId == admin.AdminId && x.RegionId == regionid).FirstOrDefault();
                                if (row != null)
                                {
                                    _context.AdminRegions.Remove(row);
                                    _context.SaveChanges();

                                }
                            }

                        }
                    }
                }

                


                

            }





        }

        public void adminUpdateProfileBilling(string email, Admin a)
        {

            var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(a => a.Id).First();

            var admin = _context.Admins.Where(x => x.AspNetUserId == aspId).FirstOrDefault();
            if (admin != null)
            {


                admin.Address1 = a.Address1;
                admin.Address2 = a.Address2;
                admin.City = a.City;

                var regionid = _context.Regions.Where(x => x.Name == a.City).Select(u => u.RegionId).FirstOrDefault();
                admin.RegionId = regionid;
                admin.Zip = a.Zip;
                admin.AltPhone = a.AltPhone;
                admin.ModifiedBy = admin.AspNetUserId;
                admin.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

            }




        }
        public void createPhysicianAccount(Physician p, IFormFile photo, string password, string role, List<int> region, string email, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc)
        {
            AspNetUser asp = new AspNetUser();
            Physician physician = new Physician();
            if (p != null)
            {

                asp.Id = Guid.NewGuid().ToString();
                asp.UserName = "MD." + p.LastName + p.FirstName;
                var plainText = Encoding.UTF8.GetBytes(password);
                asp.PasswordHash = Convert.ToBase64String(plainText);

                asp.Email = p.Email;
                asp.PhoneNumber = p.Mobile;
                asp.CreatedDate = DateTime.Now;

                _context.AspNetUsers.Add(asp);
                _context.SaveChanges();

                physician.AspNetUserId = asp.Id;
                physician.FirstName = p.FirstName;
                physician.LastName = p.LastName;
                physician.Email = p.Email;
                physician.Mobile = p.Mobile;
                physician.MedicalLicense = p.MedicalLicense;
                physician.AdminNotes = p.AdminNotes;
                physician.Address1 = p.Address1;
                physician.Address2 = p.Address2;
                physician.City = p.City;
                var rid = _context.Regions.Where(x => x.Name == p.City).Select(u => u.RegionId).FirstOrDefault();
                physician.RegionId = rid;
                physician.Zip = p.Zip;
                physician.AltPhone = p.AltPhone;
                physician.IsDeleted = new BitArray(new bool[] { false });
                var aspid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                physician.CreatedBy = aspid;
                physician.Status = 1;
                physician.BusinessName = p.BusinessName;
                physician.BusinessWebsite = p.BusinessWebsite;
                physician.Npinumber = p.Npinumber;
                physician.CreatedDate = DateTime.Now;
                int roleid = int.Parse(role);
                physician.RoleId = roleid;
       
                if (photo != null && photo.Length > 0)
                {

                    string fileName = System.IO.Path.GetFileName(photo.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string filePath = System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }
                    physician.Photo = photo.FileName;
                }
                _context.Physicians.Add(physician);
                _context.SaveChanges();
                physicianUpdateUpload(email, physician.PhysicianId, agreementDoc, backgroundDoc, hippaDoc, disclosureDoc, licenseDoc);

              
               
            }
            if (region != null)
            {
               
                foreach (var a in region)
                {
                    PhysicianRegion physicianRegion = new PhysicianRegion();
                    physicianRegion.PhysicianId = physician.PhysicianId;
                    physicianRegion.RegionId = a;
                    _context.PhysicianRegions.Add(physicianRegion);

                }
                _context.SaveChanges();
            }
           
        }
        public List<Physician> GetAllPhysicians()
        {
            return _context.Physicians.Where(x=>x.IsDeleted == new BitArray(new bool[] {false})).ToList();
        }
        public Physician getPhysicianDetails(int physicianId)
        {
            //int pid = int.Parse(physicianId);
            var res = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();
            return res;
        }
        public List<Region> getPhysicianRegions(int physicianId)
        {
            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).ToList();

            if (physician == null)
            {
                return new List<Region>();
            }

            var regionIds = _context.PhysicianRegions
                                    .Where(x => x.PhysicianId == physicianId)
                                    .Select(x => x.RegionId)
                                    .ToList();

            var regions = _context.Regions.Where(x => regionIds.Contains(x.RegionId)).ToList();
            return regions;
        }
        public void physicianUpdateStatus(string email, int physicianId, Physician p)
        {
            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();


            if (physician != null)
            {


                physician.Status = p.Status;
                physician.RoleId = p.RoleId;
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                physician.ModifiedBy = aid;
                physician.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

            }
        }
        public void physicianUpdatePassword(string email, int physicianId, string password)
        {


            var physicianAspId = _context.Physicians.Where(x => x.PhysicianId == physicianId).Select(u => u.AspNetUserId).FirstOrDefault();
            if (physicianAspId != null && password != null)
            {

                var physician = _context.AspNetUsers.Where(x => x.Id == physicianAspId).FirstOrDefault();
                var plainText = Encoding.UTF8.GetBytes(password);
                physician.PasswordHash = Convert.ToBase64String(plainText);
                physician.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
        public void physicianUpdateAccount(string email, int physicianId, Physician p, string uncheckedCheckboxes)
        {

            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();


            if (physician != null)
            {

                physician.FirstName = p.FirstName;
                physician.LastName = p.LastName;
                physician.Email = p.Email;
                physician.Mobile = p.Mobile;
                physician.MedicalLicense = p.MedicalLicense;
                physician.Npinumber = p.Npinumber;
                physician.SyncEmailAddress = p.SyncEmailAddress;
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                physician.ModifiedBy = aid;
                physician.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
                string[] boxes = uncheckedCheckboxes.Split(',');


                if (boxes != null)
                {

                    foreach (var box in boxes)
                    {
                        if (box != "")
                        {
                            int regionid = int.Parse(box);
                            var row = _context.PhysicianRegions.Where(x => x.PhysicianId == physicianId && x.RegionId == regionid).FirstOrDefault();
                            if (row != null)
                            {
                                _context.PhysicianRegions.Remove(row);
                                _context.SaveChanges();

                            }
                        }


                    }
                }


            }
        }

        public void physicianUpdateBilling(string email, int physicianId, Physician p)
        {

            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();


            if (physician != null)
            {


                physician.Address1 = p.Address1;
                physician.Address2 = p.Address2;
                physician.City = p.City;

                var regionid = _context.Regions.Where(x => x.Name == p.City).Select(u => u.RegionId).FirstOrDefault();
                physician.RegionId = regionid;
                physician.Zip = p.Zip;
                physician.AltPhone = p.AltPhone;
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                physician.ModifiedBy = aid;
                physician.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

            }




        }

        public void physicianUpdateBusiness(string email, int physicianId, Physician p, IFormFile[] files)
        {
            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();

            if (physician != null)
            {
                physician.BusinessName = p.BusinessName;
                physician.BusinessWebsite = p.BusinessWebsite;
                physician.AdminNotes = p.AdminNotes;
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                physician.ModifiedBy = aid;
                physician.ModifiedDate = DateTime.Now;
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        string fileName = System.IO.Path.GetFileName(file.FileName);
                        string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        string filePath = System.IO.Path.Combine(path, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }
                }
                physician.Photo = files[0].FileName;
                physician.Signature = files[1].FileName;

                _context.SaveChanges();



            }






        }

        public void physicianUpdateUpload(string email, int physicianId, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc)
        {
            var physician = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();
            if( physician != null )
            {
                if(agreementDoc != null && agreementDoc.Length > 0)
                {
                    
                    string fileName = "Agreement" + System.IO.Path.GetExtension(agreementDoc?.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/AdminFiles/{physicianId}");


                  
                    if (Directory.Exists(path))
                    {
                        foreach (string filePath1 in Directory.GetFiles(path))
                        {
                            string fileName1 = System.IO.Path.GetFileName(filePath1);
                       
                            if (fileName.Contains("Agreement"))
                            {
                              
                                File.Delete(filePath1);
                                physician.IsAgreementDoc = new BitArray(new bool[] { false });
                            }
                        }
                    }


                    Directory.CreateDirectory(path);
                    string filePath = System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        agreementDoc.CopyTo(stream);
                    }
              
                    physician.IsAgreementDoc = new BitArray(new bool[] { true });
                }
                if (backgroundDoc != null && backgroundDoc.Length > 0)
                {
                    string fileName = "BackgroundDoc" + System.IO.Path.GetFileName(backgroundDoc.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");

                    if (Directory.Exists(path))
                    {
                        foreach (string filePath1 in Directory.GetFiles(path))
                        {
                            string fileName1 = System.IO.Path.GetFileName(filePath1);

                            if (fileName.Contains("BackgroundDoc"))
                            {

                                File.Delete(filePath1);
                            }
                        }
                    }
                    Directory.CreateDirectory(path);

                    string filePath =  System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        backgroundDoc.CopyTo(stream);
                    }
                    physician.IsBackgroundDoc = new BitArray(new bool[] { true });
                }
                if (hippaDoc != null && hippaDoc.Length > 0)
                {
                    string fileName = "Hippa" + System.IO.Path.GetFileName(hippaDoc.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");

                    if (Directory.Exists(path))
                    {
                        foreach (string filePath1 in Directory.GetFiles(path))
                        {
                            string fileName1 = System.IO.Path.GetFileName(filePath1);

                            if (fileName.Contains("Hippa"))
                            {

                                File.Delete(filePath1);
                            }
                        }
                    }
                    Directory.CreateDirectory(path);

                    string filePath = System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        hippaDoc.CopyTo(stream);
                    }
                    
                }
                if (disclosureDoc != null && disclosureDoc.Length > 0)
                {
                    string fileName = "DisclosureDoc" + System.IO.Path.GetFileName(disclosureDoc.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");

                    if (Directory.Exists(path))
                    {
                        foreach (string filePath1 in Directory.GetFiles(path))
                        {
                            string fileName1 = System.IO.Path.GetFileName(filePath1);

                            if (fileName.Contains("DisclosureDoc"))
                            {

                                File.Delete(filePath1);
                            }
                        }
                    }
                    Directory.CreateDirectory(path);

                    string filePath = System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        disclosureDoc.CopyTo(stream);
                    }
                    physician.IsNonDisclosureDoc = new BitArray(new bool[] { true });
                }
                if (licenseDoc != null && licenseDoc.Length > 0)
                {
                    string fileName = "LicenseDoc" + System.IO.Path.GetFileName(licenseDoc.FileName);
                    string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AdminFiles");

                    if (Directory.Exists(path))
                    {
                        foreach (string filePath1 in Directory.GetFiles(path))
                        {
                            string fileName1 = System.IO.Path.GetFileName(filePath1);

                            if (fileName.Contains("LicenseDoc"))
                            {

                                File.Delete(filePath1);
                            }
                        }
                    }
                    Directory.CreateDirectory(path);

                    string filePath = System.IO.Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        licenseDoc.CopyTo(stream);
                    }
                    physician.IsLicenseDoc = new BitArray(new bool[] { true });
                }
                var aid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                if(physician.CreatedBy == null)
                {
                    physician.CreatedBy = aid;
                    physician.CreatedDate = DateTime.Now;
                }
                else
                {
                    physician.ModifiedBy = aid;
                    physician.ModifiedDate = DateTime.Now;
                }
                
                _context.SaveChanges();

            }
           
        }

        public List<Role> GetPhysiciansRoles()
        {
           List<Role> r = _context.Roles.Where(x => x.AccountType == 2).ToList();
            return r;
        }

        public void deletePhysicianAccount(string email, int physicianId)
        {
         var res =   _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();
            if(res != null)
            {
                res.IsDeleted = new BitArray(new bool[]  { true });
                var aspid = _context.AspNetUsers.Where(x=>x.Email == email).Select(u=>u.Id).FirstOrDefault();
                res.ModifiedBy = aspid;
                res.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
        public List<Menu> menuByAccountType(int accountType)
        {
            if (accountType == 3)
            {
                return _context.Menus.ToList();
            }
            var m = _context.Menus.Where(x => x.AccountType == accountType).ToList();
            return m;
        }

        public List<int> menuByAccountTypeRoleId(int accountType, int roleId)
        {


            List<int> m = _context.RoleMenus.Where(x => x.RoleId == roleId).Select(x => x.MenuId).ToList();

            return m;
        }

        public void createRole(createRole r, List<string> menu, string email)
        {
            Role role = new Role();
            if (r != null)
            {
                role.Name = r.Name;
                role.AccountType = r.AccountType;
                var asp_name = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                role.CreatedBy = asp_name;
                role.CreatedDate = DateTime.Now;
                role.IsDeleted = new BitArray(new bool[] { false });
                _context.Roles.Add(role);
                _context.SaveChanges();




                if (menu != null)
                {

                    foreach (var m in menu)
                    {
                        if (m != "")
                        {
                            RoleMenu roleMenu = new RoleMenu();
                            int menuId = int.Parse(m);
                            Menu row = _context.Menus.Where(x => x.MenuId == menuId).FirstOrDefault();
                            if (row != null)
                            {

                                roleMenu.MenuId = menuId;
                                roleMenu.RoleId = role.RoleId;

                            }
                            _context.RoleMenus.Add(roleMenu);

                        }

                        _context.SaveChanges();
                    }
                }
            }
        }

        public List<Role> GetAllRoles()
        {
            return _context.Roles.Where(x => x.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }

        public void adminDeleteRole(int roleId, string email)
        {
            if (roleId != 0)
            {
                List<RoleMenu> rows = _context.RoleMenus.Where(x => x.RoleId == roleId).ToList();
                foreach (var item in rows)
                {
                    _context.RoleMenus.Remove(item);
                    _context.SaveChanges();
                }

                Role r = _context.Roles.Where(x => x.RoleId == roleId).FirstOrDefault();
                if (r != null)
                {
                    string username = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).First();
                    r.ModifiedBy = username;
                    r.ModifiedDate = DateTime.Now;
                    r.IsDeleted = new BitArray(new bool[] { true });

                    _context.SaveChanges();
                }

            }
        }

        public Role getRoleData(int roleId)
        {
            Role r = _context.Roles.Where(u => u.RoleId == roleId).First();
            return r;
        }

        public void updateRole(Role r, List<int> menu, int roleId, string email)
        {
            List<int> todelete = new List<int>();
            List<int> toinsert = new List<int>();
            var res = _context.Roles.Where(x => x.RoleId == roleId).First();
            if (res != null)
            {
                res.RoleId = res.RoleId;
                res.Name = r.Name;
                res.AccountType = r.AccountType;
                var asp_name = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                res.ModifiedBy = asp_name;
                res.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

            }


            if (menu != null)
            {

                List<int> rm = _context.RoleMenus.Where(x => x.RoleId == roleId).Select(u => u.MenuId).ToList();
                if (rm != null)
                {
                    foreach (var a in rm)
                    {
                        if (!menu.Contains(a))
                        {
                            todelete.Add(a);
                        }
                    }
                    foreach (var b in menu)
                    {
                        if (!rm.Contains(b))
                        {
                            toinsert.Add(b);
                        }
                    }
                }
                foreach (var a in todelete)
                {
                   
                    var row = _context.RoleMenus.Where(x => x.RoleId == roleId && x.MenuId == a).First();
                    _context.RoleMenus.Remove(row);
                    _context.SaveChanges(true);
                }
                foreach (var a in toinsert)
                {
                    RoleMenu roleMenu = new RoleMenu();
                  
                    roleMenu.MenuId = a;
                    roleMenu.RoleId = roleId;
                    _context.RoleMenus.Add(roleMenu);
                    _context.SaveChanges();
                }



            }


        }

        public void createAdmin(Admin a, string password, List<int> region, string role, string email)
        {
            AspNetUser asp = new AspNetUser();
            Admin admin = new Admin();
            if (a != null)
            {
                asp.UserName = a.LastName + a.FirstName;
                asp.Id = Guid.NewGuid().ToString();
                var plainText = Encoding.UTF8.GetBytes(password);
                asp.PasswordHash = Convert.ToBase64String(plainText);
                asp.Email = a.Email;
                asp.PhoneNumber = a.Mobile;
                asp.CreatedDate = DateTime.Now;
                _context.AspNetUsers.Add(asp);
                _context.SaveChanges();


                admin.AspNetUserId = asp.Id;
                admin.FirstName = a.FirstName;
                admin.LastName = a.LastName;
                admin.Email = a.Email;
                admin.Mobile = a.Mobile;
                admin.Address1 = a.Address1;
                admin.Address2 = a.Address2;
                admin.City = a.City;
                var rid = _context.Regions.Where(x => x.Name == a.City).Select(u => u.RegionId).FirstOrDefault();
                admin.RegionId = rid;
                admin.Zip = a.Zip;
                admin.AltPhone = a.AltPhone;
                var aspid = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                admin.CreatedBy = aspid;
                admin.CreatedDate = DateTime.Now;
                int roleid = int.Parse(role);
                admin.RoleId = roleid;

                _context.Admins.Add(admin);
                _context.SaveChanges();
                

            }
            if (region != null)
            {
                
                foreach (var id in region)
                {
                    AdminRegion adminRegion = new AdminRegion();
                    adminRegion.AdminId = admin.AdminId;
                    adminRegion.RegionId = id;
                    _context.AdminRegions.Add(adminRegion);


                }

                _context.SaveChanges(true);

            }
            


        }

        public List<Role> GetAdminsRoles()
        {
            List<Role> r = _context.Roles.Where(x => x.AccountType == 1).ToList();
            return r;
        }
        public void insertShift(shiftViewModel s, string checktoggle, int[] dayList, string email)
        {
            var weekdays = "";
            foreach (var i in dayList)
            {

                weekdays += i;
            }
            Shift shift = new Shift();
            shift.PhysicianId = s.PhysicianId;
           
            DateOnly testDateOnly = DateOnly.FromDateTime(s.ShiftDate);
            shift.StartDate = testDateOnly;
            shift.RepeatUpto = s.RepeatUpto;
            shift.WeekDays = weekdays;
            var asp_id = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).First();
            shift.CreatedBy = asp_id;
            shift.CreatedDate = DateTime.Now;
            if (checktoggle == "on")
            {
                shift.IsRepeat = new BitArray(new bool[] { true });
            }
            else
            {
                shift.IsRepeat = new BitArray(new bool[] { false });
            }
            _context.Shifts.Add(shift);
            _context.SaveChanges();


            List<ShiftDetail> shiftDetails = new List<ShiftDetail>();
            ShiftDetail shiftDetail = new ShiftDetail();
            shiftDetail.ShiftId = shift.ShiftId;
            shiftDetail.ShiftDate = s.ShiftDate;
            shiftDetail.RegionId = s.RegionId;
            shiftDetail.StartTime = s.StartTime;
            shiftDetail.EndTime = s.EndTime;
            shiftDetail.IsDeleted = new BitArray(new bool[] { false });
            _context.ShiftDetails.Add(shiftDetail);
            _context.SaveChanges();



            if (shift.IsRepeat.Get(0) == true )
            {
                List<DateOnly> days = new();
                days.Add(testDateOnly);

                for (var i = 0; i < s.RepeatUpto; i++)
                {
                    for (int j = 0; j < dayList.Count(); j++)
                    {

                        int temp;
                        switch (dayList[j])
                        {
                            case 1:
                                temp = (int)DayOfWeek.Sunday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                            case 2:
                                temp = (int)DayOfWeek.Monday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                            case 3:
                                temp = (int)DayOfWeek.Tuesday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;

                                break;
                            case 4:
                                temp = (int)DayOfWeek.Wednesday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                            case 5:
                                temp = (int)DayOfWeek.Thursday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                            case 6:
                                temp = (int)DayOfWeek.Friday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                            default:
                                temp = (int)DayOfWeek.Saturday - (int)DateTime.Parse(days.Last().ToString()).DayOfWeek;
                                break;
                        }
                        if (temp <= 0)
                        {
                            temp += 7;
                        }
                        days.Add(days.Last().AddDays(temp));
                    }

                }

                foreach (var day in days)
                {

                    ShiftDetail shiftdetail1 = new ShiftDetail();

                    shiftdetail1.ShiftId = shift.ShiftId;
                    shiftdetail1.ShiftDate = day.ToDateTime(s.StartTime);
                    shiftdetail1.RegionId = s.RegionId;
                    shiftdetail1.StartTime = s.StartTime;
                    shiftdetail1.EndTime = s.EndTime;
                    shiftdetail1.IsDeleted = new BitArray(new bool[] { false });



                    var d1 = DateOnly.FromDateTime(shiftDetail.ShiftDate);
                    var d2 = DateOnly.FromDateTime(shiftdetail1.ShiftDate);



                    if ( d1 == d2)
                    {
                        continue;
                    }
                    shiftDetails.Add(shiftdetail1);
                    _context.ShiftDetails.Add(shiftdetail1);
                     _context.SaveChanges();
                }
            }



            
        }

        public List<PhysicianLocation> getAllPhysicianLocation()
        {
            return   _context.PhysicianLocations.ToList();
        }

        public void addHealthProfessional(HealthProfessional h)
        {
            HealthProfessional healthProfessional = new HealthProfessional
            {
                VendorName = h.VendorName,
                PhoneNumber = h.PhoneNumber,
                BusinessContact = h.BusinessContact,
                
              
                FaxNumber = h.FaxNumber,
                Address = h.City + h.State,
                City = h.City,
                State = h.State,
                Zip = h.Zip,
                Email = h.Email
            };
           int hid = _context.HealthProfessionalTypes.Where(x => x.HealthProfessionalId == h.Profession).Select(u => u.HealthProfessionalId).First();
            healthProfessional.Profession = hid;
            int rid = _context.Regions.Where(x => x.Name == h.City).Select(u => u.RegionId).First();
            healthProfessional.RegionId = rid;
            healthProfessional.CreatedDate = DateTime.Now;
           
            healthProfessional.IsDeleted = new BitArray(new bool[] { false });
           
           
            _context.HealthProfessionals.Add(healthProfessional);
            _context.SaveChanges();
        }

        public void editBusinessPost(int VendorId, HealthProfessional h)
        {
            HealthProfessional healthProfessional = _context.HealthProfessionals.Where(x=>x.VendorId ==  h.VendorId).First();
            if(healthProfessional != null)
            {
                healthProfessional.VendorName = h.VendorName;
                int hid = _context.HealthProfessionalTypes.Where(x => x.HealthProfessionalId == h.Profession).Select(u => u.HealthProfessionalId).First();
                healthProfessional.Profession = hid;
                healthProfessional.FaxNumber = h.FaxNumber;
                healthProfessional.Address = h.Address;
                healthProfessional.City = h.City;
                healthProfessional.State = h.State;
                healthProfessional.Zip  = h.Zip;
                int rid = _context.Regions.Where(x => x.Name == h.City).Select(u => u.RegionId).First();
                healthProfessional.RegionId = rid;
                healthProfessional.ModifiedDate = DateTime.Now;
                healthProfessional.PhoneNumber = h.PhoneNumber;
                healthProfessional.Email = h.Email;
                healthProfessional.BusinessContact = h.BusinessContact;
                _context.SaveChanges();
            }
        }
        public void adminDeletePartner(int VendorId, string email)
        {
          HealthProfessional h =  _context.HealthProfessionals.Where(x => x.VendorId == VendorId).First();
            h.IsDeleted = new BitArray(new bool[] { true });
            _context.SaveChanges();
        }

        public List<BlockRequest> GetAllBlockRequests()
        {

          return  _context.BlockRequests.Where(x=>x.IsActive == new BitArray(new bool[] {false})).ToList();
        }
        public void unblockPatient(int RequestId, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            BlockRequest b = _context.BlockRequests.Where(x=>x.RequestId == RequestId.ToString()).First();
            Request patient = _context.Requests.Where(x => x.RequestId == RequestId).First();
            if (patient != null)
            {
                patient.Status = 1;
                _context.SaveChanges();
                rs.RequestId = RequestId;
                rs.Status = 1;

                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Admins.Where(x => x.AspNetUserId == aspId).Select(u => u.AdminId).FirstOrDefault();
                rs.AdminId = id;
                rs.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(rs);
                _context.SaveChanges();

                b.IsActive = new BitArray(new bool[] { true });
                b.ModifiedDate = DateTime.Now;

                _context.SaveChanges();
            }
        }

        //public List<User> patientHistory()
        //{
        //    List<User> patient = new List<User>();
        //    foreach (var item in _context.AspNetUsers.Select(x => x.Email))
        //    {
        //        User u = _context.Users.Where(x => x.Email == item).FirstOrDefault();
        //        if (u != null)
        //        {
        //            patient.Add(u);
        //        }


        //    }
        //    return patient;


        //}
        public List<User> patientHistory()
        {
            List<User> patients = new List<User>();
            var asp = _context.AspNetUsers.ToList();
           
                foreach (var aspNetUser in asp)
                {
                    var user = _context.Users.Where(u => u.AspNetUserId == aspNetUser.Id).FirstOrDefault();
                    if (user != null)
                    {
                        patients.Add(user);
                    }
                }
            

            return patients;
        }

        public List<Request> explorePatientHistory(int UserId)
        {
            List<Request> patients = new List<Request>();
            string asp_id = _context.Users.Where(x => x.UserId == UserId).Select(x => x.AspNetUserId).FirstOrDefault();
            List<int> uids = _context.Users.Where(x => x.AspNetUserId == asp_id).Select(u=>u.UserId).ToList();
            if(uids != null)
            {
                foreach (var u in uids)
                {
                    var row = _context.Requests.Where(x => x.UserId == u).First();
                    patients.Add(row);
                }
            }
           
            return patients;
        }

        public List<searchRecords> searchRecords(string email)
        {
            var query = (from req in _context.Requests
                         join client in _context.RequestClients on req.RequestId equals client.RequestId
                         select new
                         {
                             Request = req,
                             Client = client,
                             Status = req.Status
                         })
                         .Select(x => new searchRecords
                         {
                             requestId = x.Client.RequestId,
                             patientName = x.Client.FirstName + " " + x.Client.LastName,
                           
                             requestorName = x.Request.FirstName + " " + x.Request.LastName,
                             patientEmail = x.Client.Email,
                             requestedDate = x.Request.CreatedDate,
                             patientContact = x.Client.PhoneNumber,
                             requestorContact = x.Request.PhoneNumber,
                             patientZipCode = x.Client.ZipCode,
                             patientAddress = x.Client.Address,
                             patientCity = x.Client.City,
                             physicianName = _context.Physicians.Where(u => u.PhysicianId == x.Request.PhysicianId).Select(u => u.FirstName).FirstOrDefault(),
                             Status = x.Status,
                             RequestTypeId = x.Request.RequestTypeId,
                            
                             PhysicianId = x.Request.PhysicianId,
                            
                           
                         });

            return query.ToList();

        }

        public List<searchRecords> filterSearchRecords(string sessionemail, int? requestStatus, string? patientName, int? requestType, DateOnly? fromDate, DateOnly? toDate, string? providerName, string? email, string? phone)
        {
            if(patientName != null)
            {
                patientName = patientName.ToLower();
            }
           if(providerName != null)
            {
                providerName = providerName.ToLower();
            }
           
            List<searchRecords> res = searchRecords(sessionemail);
            if(!string.IsNullOrWhiteSpace(patientName))
            {
                res = res.Where(u => u.patientName.ToLower().Contains(patientName)).ToList();
            }
            if(requestStatus != null)
            {
                res = res.Where(u => u.Status == requestStatus).ToList();
            }
            if(requestType != null)
            {
                res = res.Where(u => u.RequestTypeId == requestType).ToList();
            }
            if(!string.IsNullOrWhiteSpace(providerName))
            {
                res = res.Where(u => u.physicianName.ToLower().Contains(providerName)).ToList();
            }
            if(!string.IsNullOrWhiteSpace(email))
            {
                res = res.Where(x=>x.patientEmail ==  email).ToList();
            }
            if(!string.IsNullOrWhiteSpace(phone))
            {
                res = res.Where(x=>x.patientContact == phone).ToList();
            }
           


            return res;
        }



        public List<User> filterPatientHistory(string patientFirstName, string patientLastName, string email, string phone)
        {
            List<User> res = patientHistory();
            if(patientLastName != null)
            {
                patientLastName = patientLastName.ToLower();
            }
            if(patientFirstName != null)
            {
                patientFirstName = patientFirstName.ToLower();
            }
          
           

            if (!string.IsNullOrWhiteSpace(patientFirstName))
            {
                res = res.Where(u => u.FirstName.ToLower().Contains(patientFirstName)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(patientLastName))
            {
                res = res.Where(u => u.LastName.ToLower().Contains(patientLastName)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                res = res.Where(x => x.Email == email).ToList();
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                res = res.Where(x => x.Mobile == phone).ToList();
            }



            return res;
        }

        public List<BlockRequest> filterBlockedHistory(string patientName, DateOnly date, string email, string phone)
        {
            List<BlockRequest> res = GetAllBlockRequests();
            if (patientName != null)
            {
                patientName = patientName.ToLower();
            }
           



            //if (!string.IsNullOrWhiteSpace(patientName))
            //{
            //    res = res.Where(u => u.FirstName.ToLower().Contains(patientName)).ToList();
            //}
            //if(date != null)
            //{
            //    res = res.Where(x=>x.CreatedDate == date)
            //}
            if (!string.IsNullOrWhiteSpace(email))
            {
                res = res.Where(x => x.Email == email).ToList();
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                res = res.Where(x => x.PhoneNumber == phone).ToList();
            }



            return res;
        }
        public void insertEmailLog(string? emailTemplate,  string? subjectName, string? emailId, int? requestId,  string sessionemail, string? filePath)
        {
            EmailLog emailLog = new EmailLog();
            emailLog.EmailTemplate = emailTemplate;
            emailLog.SubjectName = subjectName;
            emailLog.EmailId = emailId;
            string confirmationNumber = _context.Requests.Where(x => x.RequestId == requestId).Select(u => u.ConfirmationNumber).FirstOrDefault();
            emailLog.ConfirmationNumber = confirmationNumber;
            emailLog.FilePath = filePath;
            emailLog.RequestId = requestId;
           int adminId =  _context.Admins.Where(x => x.Email == sessionemail).Select(x => x.AdminId).FirstOrDefault();
            emailLog.AdminId = adminId;
            int physicianId = _context.Physicians.Where(x => x.Email == sessionemail).Select(x => x.PhysicianId).FirstOrDefault();
            emailLog.PhysicianId = physicianId;
            emailLog.CreateDate = DateTime.Now;
            emailLog.SentDate = DateTime.Now;

            _context.EmailLogs.Add(emailLog);
            _context.SaveChanges();
        }

        public void insertSMSLog(string? SMSTemplate, string? mobile, int? requestId, string sessionemail)
        {
            Smslog smslog = new Smslog();
            smslog.Smstemplate = SMSTemplate;

            smslog.MobileNumber = mobile;
            string confirmationNumber = _context.Requests.Where(x => x.RequestId == requestId).Select(u => u.ConfirmationNumber).FirstOrDefault();
            smslog.ConfirmationNumber = confirmationNumber;

            smslog.RequestId = requestId;
            int adminId = _context.Admins.Where(x => x.Email == sessionemail).Select(x => x.AdminId).FirstOrDefault();
            smslog.AdminId = adminId;
            int physicianId = _context.Physicians.Where(x => x.Email == sessionemail).Select(x => x.PhysicianId).FirstOrDefault();
            smslog.PhysicianId = physicianId;
            smslog.CreateDate = DateTime.Now;
            smslog.SentDate = DateTime.Now;
            smslog.SentTries = 1;
            _context.Smslogs.Add(smslog);
            _context.SaveChanges();
        }

        public List<EmailLog> emailLogs()
        {
            return _context.EmailLogs.ToList();
        }
        public List<EmailLog> emailLogs(int? role, string? recieverName, string? email, DateOnly? createdDate, DateOnly? sentDate)
        {
            if(recieverName != null)
            {
                recieverName = recieverName.ToLower();
            }
           
            List<EmailLog> res =  emailLogs();
            if(role != null)
            {
                res = res.Where(x => x.RoleId == role).ToList();
            }
            //if(recieverName != null)
            //{
            //    res = res.Where(x=>x.)
            //}
            if(email != null)
            {
                res = res.Where(x=>x.EmailId == email).ToList();
            }
            //if(createdDate != null)
            //{
            //    res = res.Where(x=>x.CreateDate.Date() == createdDate).ToList();
            //}

            return res;
        }

        public List<Smslog> SMSLogs()
        {
            return _context.Smslogs.ToList();
        }

        public List<Smslog> SMSLogs(int? role, string? recieverName, string? mobile, DateOnly? createdDate, DateOnly? sentDate)
        {
            if (recieverName != null)
            {
                recieverName = recieverName.ToLower();
            }
            List<Smslog> res = SMSLogs();
            if (role != null)
            {
                res = res.Where(x => x.RoleId == role).ToList();
            }
            //if(recieverName != null)
            //{
            //    res = res.Where(x=>x.)
            //}
            if (mobile != null)
            {
                res = res.Where(x => x.MobileNumber == mobile).ToList();
            }
            //if(createdDate != null)
            //{
            //    res = res.Where(x=>x.CreateDate.Date() == createdDate).ToList();
            //}

            return res;
        }
        public ShiftDetail getShiftDetailByShiftDetailId(int id)
        {
            return _context.ShiftDetails.FirstOrDefault(e => e.ShiftDetailId == id && e.IsDeleted != new BitArray(1, true));
        }
       
        public Shift getShiftByID(int shiftid)        {            return _context.Shifts.FirstOrDefault(e => e.ShiftId == shiftid);        }
        public List<Physician> getPhysicianListByregion(int regid)        {            return _context.Physicians.Where(m => m.RegionId == regid && m.IsDeleted == null).ToList();        }

        public ShiftDetailsModel getViewShiftData(int id)        {            ShiftDetailsModel model = new ShiftDetailsModel();            ShiftDetail sd = getShiftDetailByShiftDetailId(id);

            Shift s = getShiftByID(sd.ShiftId);            model.RegionId = (int)sd.RegionId;
            model.physicians = GetAllPhysicians();


            DateOnly date = DateOnly.Parse(sd.ShiftDate.ToString("yyyy-MM-dd"));            model.regions = getAllRegions();            model.Shiftdate = date;            model.Physicianid = s.PhysicianId;            model.shiftData = s;            model.ShiftDetailData = sd;            return model;        }


        public void UpdateShiftDetailData(ShiftDetailsModel model, string email)
        {

            Admin admin = getAdminTableDataByEmail(email)
;
            ShiftDetail sd = getShiftDetailByShiftDetailId(model.ShiftDetailData.ShiftDetailId);
            sd.ModifiedBy = admin.AspNetUserId.ToString();
            sd.StartTime = model.ShiftDetailData.StartTime;
            sd.EndTime = model.ShiftDetailData.EndTime;
            sd.ShiftDate = model.ShiftDetailData.ShiftDate;
            sd.ModifiedDate = DateTime.Now;
            _context.SaveChanges();
        }
        public void DeleteShiftDetails(int id, string email)
        {
            Admin admin = getAdminTableDataByEmail(email);
            ShiftDetail sd = getShiftDetailByShiftDetailId(id);
            sd.ModifiedBy = admin.AspNetUserId.ToString();
            sd.IsDeleted = new BitArray(1, true);
            sd.ModifiedDate = DateTime.Now;
            UpdateShiftDetailTable(sd);
        }

        public void UpdateShiftDetailTable(ShiftDetail sd)        {            _context.ShiftDetails.Update(sd);            _context.SaveChanges();        }
        public void UpdateShiftDetailsStatus(int id)
        {
            ShiftDetail sd = getShiftDetailByShiftDetailId(id)
;
            if (sd.Status == 0)
            {
                sd.Status = 1;
            }
            else
            {
                sd.Status = 0;
            }
            UpdateShiftDetailTable(sd);
        }
        public ShiftDetailsModel getReviewShiftData(int reg)
        {
            return new ShiftDetailsModel
            {
                regions = getAllRegions(),
                shiftdetail = getShiftDetailByRegion(reg),
                RegionId = reg
            };

        }
        public List<ShiftDetail> getShiftDetailByRegion(int reg)        {
            if (reg != 0)
                return _context.ShiftDetails.Where(e => e.Status == 0 && e.IsDeleted == new BitArray(1, false) && e.RegionId == reg).Include(e => e.Shift).ThenInclude(e => e.Physician).ThenInclude(e => e.Region).ToList();
            else
                return _context.ShiftDetails.Where(e => e.Status == 0 && e.IsDeleted == new BitArray(1, false)).Include(e => e.Shift).ThenInclude(e => e.Physician).ThenInclude(e => e.Region).ToList();        }
        public Admin getAdminTableDataByEmail(string? email)        {            return _context.Admins.FirstOrDefault(a => a.Email == email);        }
        public void ApproveShift(string[] selectedShifts)        {            var shifts = selectedShifts[0].Split(',');            foreach (var shift in shifts)            {                ShiftDetail shiftdetail = getShiftDetailByShiftDetailId(int.Parse(shift));                shiftdetail.Status = 1;                UpdateShiftDetailTable(shiftdetail);            }        }
        public void DeleteShift(string[] selectedShifts)        {            var shifts = selectedShifts[0].Split(',');            foreach (var shift in shifts)
            {                ShiftDetail shiftdetail = getShiftDetailByShiftDetailId(int.Parse(shift));                shiftdetail.IsDeleted = new BitArray(1, true);                UpdateShiftDetailTable(shiftdetail);            }        }
        public ShiftDetailsModel SchedulingMonth(int monthNum)
        {
            return new ShiftDetailsModel
            {
                regions = getAllRegions(),
                shiftdetail = _context.ShiftDetails.Where(e => e.Status == 0 && e.IsDeleted == new BitArray(1, false) && e.ShiftDate.Month == monthNum).Include(e => e.Shift).ThenInclude(e => e.Physician).ThenInclude(e => e.Region).ToList(),
              
                  
            };
        }
    }
}

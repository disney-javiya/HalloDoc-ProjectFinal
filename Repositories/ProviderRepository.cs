using HalloDoc.DataAccessLayer.DataContext;
using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.IRepository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly ApplicationDbContext _context;

        public ProviderRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public IEnumerable<RequestandRequestClient> getRequestStateData(int type, string email)
        {
            int phyId = _context.Physicians.Where(x=>x.Email == email).Select(u=>u.PhysicianId).FirstOrDefault();
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
                             acceptedDate = x.Request.AcceptedDate,
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
                query = query.Where(req => req.Status == 1 && req.acceptedDate == null && req.PhysicianId == phyId);
            }
            else if (type == 2)
            {
                query = query.Where(req => req.Status == 2 && req.acceptedDate != null && req.PhysicianId == phyId);
            }
            else if (type == 3)
            {
                query = query.Where(req => req.Status == 4 || req.Status == 5);
            }
            else if (type == 4)
            {

                query = query.Where(req => req.Status == 6);
            }


            return query.ToList();
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


        public List<RequestandRequestClient> getFilterByrequestTypeAndName(IEnumerable<RequestandRequestClient> r, int requesttypeId, string patient_name)
        {
           
            List<RequestandRequestClient> bytid = getByRequesttypeId(r, requesttypeId);
            List< RequestandRequestClient> byname = getFilterByName(bytid, patient_name);
           
            return byname;

        }

        public void providerAccept(int requestId, string email)
        {
            Request r = new Request();
            RequestStatusLog rs = new RequestStatusLog();
            int phyId = _context.Physicians.Where(x=>x.Email == email).Select(u=>u.PhysicianId).FirstOrDefault();
            var res = _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (res != null)
            {
                res.Status = 2;
                res.AcceptedDate = DateTime.Now;
                _context.SaveChanges();
                rs.RequestId = requestId;
                rs.Status = 2;
                rs.PhysicianId = phyId;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }

        public RequestClient getPatientInfo(int requestId)
        {
            return _context.RequestClients.FirstOrDefault(x => x.RequestId == requestId);
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
                v.PhysicianNote = res.PhysicianNotes;
                _context.SaveChanges();
            }
            foreach (var row in res1)
            {
                if (row.Status == 3)
                {
                    v.AdminCancellationNotes = "Cancelled: " + row.Notes;
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
                else if (row.Status == 1)
                {
                    if (row.TransToPhysicianId == null)
                    {
                        var physicianusername = _context.Physicians.Where(x => x.PhysicianId == row.PhysicianId).Select(u => u.FirstName).FirstOrDefault();
                        v.TransferNote = v.TransferNote + physicianusername + "tranfered Again" + ":" + row.Notes;
                    }
                }
                else if (row.Status == 7)
                {
                    v.PatientCancellationNotes = _context.RequestStatusLogs.Where(x => x.RequestId == requestId && x.Status == 7).Select(u => u.Notes).FirstOrDefault();
                }
            }


            return v;
        }

        public void providerNotes(int requestId, viewNotes v, string email)
        {
            RequestNote rn = new RequestNote();
            var res = _context.RequestNotes.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (res == null)
            {
                rn.RequestId = requestId;
                rn.PhysicianNotes = v.AdditionalNote;
                rn.AdminNotes = null;
                rn.CreatedBy = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.UserName).FirstOrDefault();
                rn.CreatedDate = DateTime.Now;
                _context.RequestNotes.Add(rn);
                _context.SaveChanges();
            }
            else
            {

                var r = _context.RequestNotes.Where(u => u.RequestNotesId == res.RequestNotesId).FirstOrDefault();
                r.RequestNotesId = r.RequestNotesId;
                r.PhysicianNotes = v.AdditionalNote;
               
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

        public void providerTransferCase(string requestId, string additionalNotesTransfer, string email)
        {
            RequestStatusLog rs = new RequestStatusLog();
            Request r = new Request();
            int reqId = int.Parse(requestId);
            var res = _context.Requests.Where(x => x.RequestId == reqId).FirstOrDefault();
            if (res != null)
            {
                res.Status = 1;
                res.PhysicianId = null;

                _context.SaveChanges();
                rs.RequestId = reqId;
                rs.Status = 1;
                var aspId = _context.AspNetUsers.Where(x => x.Email == email).Select(u => u.Id).FirstOrDefault();
                var id = _context.Physicians.Where(x => x.AspNetUserId == aspId).Select(u => u.PhysicianId).FirstOrDefault();
                rs.PhysicianId = id;
                rs.TransToAdmin = new BitArray(new bool[] { true });
                rs.Notes = additionalNotesTransfer;
                rs.CreatedDate = DateTime.Now;
            }
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }
        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId)
        {


            return _context.RequestWiseFiles.Where(d => d.RequestId == requestId && d.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }
        public string getName(string requestId)
        {
            int reqId = int.Parse(requestId);
            string name = _context.RequestClients.Where(x => x.RequestId == reqId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();

            return name;
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
                    var id = _context.Physicians.Where(x => x.AspNetUserId == aspId).Select(u => u.PhysicianId).FirstOrDefault();

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    RequestWiseFile newFile = new RequestWiseFile
                    {
                        FileName = fileName,
                        RequestId = requestId,
                        PhysicianId = id,
                        IsDeleted = new BitArray(new bool[] { false }),
                        CreatedDate = DateTime.Now
                    };

                    _context.RequestWiseFiles.Add(newFile);
                    _context.SaveChanges();
                }
            }

           
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
        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds)
        {
            return _context.RequestWiseFiles.Where(f => fileIds.Contains(f.RequestWiseFileId) && f.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }
        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId)
        {
            return _context.RequestWiseFiles.Where(f => f.RequestId == requestId && f.IsDeleted == new BitArray(new bool[] { false })).ToList();
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
        public List<string> GetAllFiles(int requestId)
        {
            return _context.RequestWiseFiles.Where(x => x.RequestId == requestId && x.IsDeleted == new BitArray(new bool[] { false })).Select(f => f.FileName).ToList();

        }

        public void insertEmailLog(string? emailTemplate, string? subjectName, string? emailId, int? requestId, string sessionemail, string? filePath)
        {
            EmailLog emailLog = new EmailLog();
            emailLog.EmailTemplate = emailTemplate;
            emailLog.SubjectName = subjectName;
            emailLog.EmailId = emailId;
            string confirmationNumber = _context.Requests.Where(x => x.RequestId == requestId).Select(u => u.ConfirmationNumber).FirstOrDefault();
            emailLog.ConfirmationNumber = confirmationNumber;
            emailLog.FilePath = filePath;
            emailLog.RequestId = requestId;
            int adminId = _context.Admins.Where(x => x.Email == sessionemail).Select(x => x.AdminId).FirstOrDefault();
            emailLog.AdminId = adminId;
            int physicianId = _context.Physicians.Where(x => x.Email == sessionemail).Select(x => x.PhysicianId).FirstOrDefault();
            emailLog.PhysicianId = physicianId;
            emailLog.CreateDate = DateTime.Now;
            emailLog.SentDate = DateTime.Now;

            _context.EmailLogs.Add(emailLog);
            _context.SaveChanges();
        }
    }
}

﻿using DocumentFormat.OpenXml.Spreadsheet;
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
                             CompletedByPhysician = x.Request.CompletedByPhysician,
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
                query = query.Where(req => req.Status == 1 &&  req.PhysicianId == phyId);
            }
            else if (type == 2)
            {
                query = query.Where(req => req.Status == 2  && req.PhysicianId == phyId);
            }
            else if (type == 3)
            {
                query = query.Where(req => (req.Status == 4 || req.Status == 5 ) && req.PhysicianId == phyId );
            }
            else if (type == 4)
            {

                query = query.Where(req => req.Status == 6 && req.PhysicianId == phyId);
            }
            else
            {
                return query.ToList();
            }

            return query.ToList();
        }       
        public AspNetUser GetUserByEmail(string email)
        {
            return _context.AspNetUsers.Where(x => x.Email == email).FirstOrDefault();
        }
        public AspNetUser GetUserById(string id)
        {
           return _context.AspNetUsers.Where(x=>x.Id ==  id).FirstOrDefault();
        }
        public Physician getProviderInfo(string email)
        {
            return _context.Physicians.Where(x => x.Email == email).FirstOrDefault();
        }

        public List<Region> getPhysicianRegions(string email)
        {
            var physician = _context.Physicians.FirstOrDefault(x => x.Email == email);
            if (physician == null)
            {
                return new List<Region>();
            }

            var regionIds = _context.PhysicianRegions
                                    .Where(x => x.PhysicianId == physician.PhysicianId)
                                    .Select(x => x.RegionId)
                                    .ToList();

            var regions = _context.Regions.Where(x => regionIds.Contains(x.RegionId)).ToList();
            return regions;
        }
       
        public void physicianUpdatePassword(string email, string password)
        {
            var aspuser = _context.AspNetUsers.Where(x => x.Email == email).First();


            if (aspuser != null)
            {
                var plainText = Encoding.UTF8.GetBytes(password);
                aspuser.PasswordHash = Convert.ToBase64String(plainText);
                aspuser.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
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

        public List<HealthProfessionalType> GetAllHealthProfessionalType()
        {
            return _context.HealthProfessionalTypes.ToList();
        }
        public List<HealthProfessional> GetAllHealthProfessional()
        {
            return _context.HealthProfessionals.ToList();
        }
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId)
        {
            return _context.HealthProfessionals.Where(x => x.Profession == healthprofessionalId).ToList();
        }
        public HealthProfessional GetProfessionInfo(int vendorId)
        {
            return _context.HealthProfessionals.Where(x => x.VendorId == vendorId).FirstOrDefault();
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

       public void providerEncounterCase(int requestId, string calltype, string email)
       {
            var res = _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            RequestStatusLog rs = new RequestStatusLog();
            if(res != null && calltype != null)
            {
                if(calltype == "housecall")
                {
                    res.Status = 5;
                    res.CallType = 1;
                    res.ModifiedDate = DateTime.Now;
                    _context.SaveChanges();

                    rs.RequestId = requestId;
                    rs.Status = 5;
                    rs.PhysicianId = res.PhysicianId;
                    rs.CreatedDate = DateTime.Now;
                    _context.RequestStatusLogs.Add(rs);
                    _context.SaveChanges();
                }

                if (calltype == "consult")
                {
                    res.Status = 6;
                    res.CallType = 2;
                    res.ModifiedDate = DateTime.Now;
                    _context.SaveChanges();

                    rs.RequestId = requestId;
                    rs.Status = 6;
                    rs.PhysicianId = res.PhysicianId;
                    rs.CreatedDate = DateTime.Now;
                    _context.RequestStatusLogs.Add(rs);
                    _context.SaveChanges();
                }
            }

       }

        public encounterModel providerEncounterForm(int requestId)
        {

            var e = _context.EncounterForms.Where(x => x.RequestId == requestId).FirstOrDefault();
            if(e == null)
            {
              var rc =  _context.RequestClients.Where(x=>x.RequestId == requestId).FirstOrDefault();
                encounterModel result = new encounterModel();
                result.FirstName = rc.FirstName;
                result.LastName = rc.LastName;
                result.Location = rc.Location;
                result.DateOfBirth = rc.IntDate + rc.StrMonth + rc.IntYear;
                result.phone = rc.PhoneNumber;
                result.Email = rc.Email;
                return result;
            }
            else
            {
                var query = (from client in _context.RequestClients
                             join encounter in _context.EncounterForms on client.RequestId equals encounter.RequestId
                             select new
                             {

                                 Encounter = encounter,
                                 client = client,

                             })
                       .Select(x => new encounterModel
                       {
                           requestId = x.client.RequestId,
                           FirstName = x.client.FirstName,
                           LastName = x.client.LastName,
                           Location = x.client.Address,
                           DateOfBirth = x.client.IntDate + "/" + x.client.StrMonth + "/" + x.client.IntYear,
                           phone = x.client.PhoneNumber,
                           Email = x.client.Email,
                           IsFinalized = x.Encounter.IsFinalized,
                           HistoryIllness = x.Encounter.HistoryIllness,
                           MedicalHistory = x.Encounter.MedicalHistory,
                           Date = x.Encounter.Date,
                           Medications = x.Encounter.Medications,
                           Allergies = x.Encounter.Allergies,
                           Temp = x.Encounter.Temp,
                           Hr = x.Encounter.Hr,
                           Rr = x.Encounter.Rr,
                           BpS = x.Encounter.BpS,
                           BpD = x.Encounter.BpD,
                           O2 = x.Encounter.O2,
                           Pain = x.Encounter.Pain,
                           Heent = x.Encounter.Heent,
                           Cv = x.Encounter.Cv,
                           Chest = x.Encounter.Chest,
                           Abd = x.Encounter.Abd,
                           Extr = x.Encounter.Extr,
                           Skin = x.Encounter.Skin,
                           Neuro = x.Encounter.Neuro,
                           Other = x.Encounter.Other,
                           Diagnosis = x.Encounter.Diagnosis,
                           TreatmentPlan = x.Encounter.TreatmentPlan,
                           MedicationDispensed = x.Encounter.MedicationDispensed,
                           Procedures = x.Encounter.Procedures,
                           FollowUp = x.Encounter.FollowUp,

                       });

                encounterModel res = query.Where(x => x.requestId == requestId).FirstOrDefault();
                return res;

            }

        }

        public void providerEncounterFormPost(int requestId, encounterModel em)
        {
            var e = _context.EncounterForms.Where(x => x.RequestId == requestId).FirstOrDefault();
            if(e == null)
            {
                EncounterForm encounterForm = new EncounterForm();
                encounterForm.RequestId = requestId;
                encounterForm.IsFinalized = new BitArray(new bool[] { false });
                encounterForm.HistoryIllness = em.HistoryIllness;
                encounterForm.MedicalHistory = em.MedicalHistory;
                encounterForm.Date = em.Date;
                encounterForm.Medications = em.Medications;
                encounterForm.Allergies = em.Allergies;
                encounterForm.Temp = em.Temp;
                encounterForm.Hr = em.Hr;
                encounterForm.Rr = em.Rr;
                encounterForm.BpS = em.BpS;
                encounterForm.BpD = em.BpD;
                encounterForm.O2 = em.O2;
                encounterForm.Pain = em.Pain;
                encounterForm.Heent = em.Heent;
                encounterForm.Cv = em.Cv;
                encounterForm.Chest = em.Chest;
                encounterForm.Abd = em.Abd;
                encounterForm.Extr = em.Extr;
                encounterForm.Skin = em.Skin;
                encounterForm.Neuro = em.Neuro;
                encounterForm.Other = em.Other;
                encounterForm.Diagnosis = em.Diagnosis;
                encounterForm.TreatmentPlan = em.TreatmentPlan;
                encounterForm.MedicationDispensed = em.MedicationDispensed;
                encounterForm.Procedures = em.Procedures;
                encounterForm.FollowUp = em.FollowUp;
                _context.EncounterForms.Add(encounterForm);
                _context.SaveChanges();
            }
            else
            {
               
                e.IsFinalized = new BitArray(new bool[] { false });
                e.HistoryIllness = em.HistoryIllness;
                e.MedicalHistory = em.MedicalHistory;
                e.Date = em.Date;
                e.Medications = em.Medications;
                e.Allergies = em.Allergies;
                e.Temp = em.Temp;
                e.Hr = em.Hr;
                e.Rr = em.Rr;
                e.BpS = em.BpS;
                e.BpD = em.BpD;
                e.O2 = em.O2;
                e.Pain = em.Pain;
                e.Heent = em.Heent;
                e.Cv = em.Cv;
                e.Chest = em.Chest;
                e.Abd = em.Abd;
                e.Extr = em.Extr;
                e.Skin = em.Skin;
                e.Neuro = em.Neuro;
                e.Other = em.Other;
                e.Diagnosis = em.Diagnosis;
                e.TreatmentPlan = em.TreatmentPlan;
                e.MedicationDispensed = em.MedicationDispensed;
                e.Procedures = em.Procedures;
                e.FollowUp = em.FollowUp;
                _context.SaveChanges();
            }
            
        }


        public void transferToConcludeState(int requestId)
        {
            var res = _context.Requests.Where(x=>x.RequestId == requestId).FirstOrDefault();
            RequestStatusLog rs = new RequestStatusLog();
            if(res != null) {
                res.Status = 6;
                res.ModifiedDate = DateTime.Now;
                _context.SaveChanges();

                rs.RequestId = requestId;
                rs.Status = 6;
                rs.PhysicianId = res.PhysicianId;
                rs.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(rs);
                _context.SaveChanges();
            }
        }

        public void providerIsFinal(int requestId)
        {
            var res = _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            var e = _context.EncounterForms.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (res != null && e!=null)
            {
                res.CompletedByPhysician = new BitArray(new bool[] { true });

                e.IsFinalized = new BitArray(new bool[] { true });
                _context.SaveChanges();

            }
        }

        public void providerConcludeCarePost(int requestId, string notes, string email)
        {
            var res = _context.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            RequestStatusLog rs = new RequestStatusLog();
            RequestNote rn = _context.RequestNotes.Where(x=>x.RequestId == requestId).FirstOrDefault();
            var e = _context.EncounterForms.Where(x => x.RequestId == requestId).FirstOrDefault();
            if(res.CompletedByPhysician[0] && e.IsFinalized[0])
            {
                res.Status = 8;
                _context.SaveChanges();

                rs.RequestId = requestId;
                rs.Status = 8;
                rs.PhysicianId = res.PhysicianId;
                rs.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(rs);
                _context.SaveChanges();

                rn.PhysicianNotes = notes;
                rn.ModifiedDate = DateTime.Now;
               string name = _context.Physicians.Where(x=>x.Email == email).Select(u=>u.FirstName).First();
                rn.ModifiedBy = name;
                _context.SaveChanges();
            }
        }


        public Physician getPhysicianDetails(int physicianId)
        {
            
            var res = _context.Physicians.Where(x => x.PhysicianId == physicianId).FirstOrDefault();
            return res;
        }

        public List<Physician> GetAllPhysicians()
        {
            return _context.Physicians.Where(x => x.IsDeleted == new BitArray(new bool[] { false })).ToList();
        }
        public List<Region> getAllRegions()
        {
            return _context.Regions.ToList();
        }
        public List<ShiftDetailsModel> getshiftDetail(string email)
        {
            Physician res = _context.Physicians.Where(x => x.Email == email).FirstOrDefault();
            var data = from sd in _context.ShiftDetails
                       join
                       s in _context.Shifts on sd.ShiftId equals s.ShiftId
                       join phy in _context.Physicians on s.PhysicianId equals phy.PhysicianId
                       join reg in _context.Regions on sd.RegionId equals reg.RegionId
                       where sd.IsDeleted == new BitArray(new bool[] { false }) && phy.PhysicianId == res.PhysicianId
                       select new ShiftDetailsModel
                       {
                           PhysicianName = phy.FirstName + " " + phy.LastName,
                           Physicianid = phy.PhysicianId,
                           RegionName = reg.Name,
                           Status = sd.Status,
                           Starttime = sd.StartTime,
                           Endtime = sd.EndTime,
                           Shiftdate = DateOnly.FromDateTime(sd.ShiftDate),

                           Shiftdetailid = sd.ShiftDetailId,
                       };
            return data.ToList();

        }
        public ShiftDetail getShiftDetailByShiftDetailId(int id)
        {
            return _context.ShiftDetails.FirstOrDefault(e => e.ShiftDetailId == id && e.IsDeleted != new BitArray(1, true));
        }
        public Shift getShiftByID(int shiftid)
        {
            return _context.Shifts.FirstOrDefault(e => e.ShiftId == shiftid);
        }
        public ShiftDetailsModel getViewShiftData(int id)
        {
            ShiftDetailsModel model = new ShiftDetailsModel();
            ShiftDetail sd = getShiftDetailByShiftDetailId(id);

            Shift s = getShiftByID(sd.ShiftId);
            model.RegionId = (int)sd.RegionId;
            model.physicians = GetAllPhysicians();


            DateOnly date = DateOnly.Parse(sd.ShiftDate.ToString("yyyy-MM-dd"));
            model.regions = getAllRegions();
            model.Shiftdate = date;
            model.Physicianid = s.PhysicianId;
            model.shiftData = s;
            model.ShiftDetailData = sd;


            return model;
        }



        public void insertShift(shiftViewModel s, string checktoggle, int[] dayList, string email)
        {
            var weekdays = "";
            foreach (var i in dayList)
            {

                weekdays += i;
            }

            int phy_id =   _context.Physicians.Where(x => x.Email == email).Select(u => u.PhysicianId).FirstOrDefault();
            Shift shift = new Shift();
            shift.PhysicianId = phy_id;

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



            if (shift.IsRepeat.Get(0) == true)
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



                    if (d1 == d2)
                    {
                        continue;
                    }
                    shiftDetails.Add(shiftdetail1);
                    _context.ShiftDetails.Add(shiftdetail1);
                    _context.SaveChanges();
                }
            }




        }


    }
}
﻿using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Repository.IRepository
{
    public interface IProviderRepository
    {

        public IEnumerable<RequestandRequestClient> getRequestStateData(int type, string email);
        public AspNetUser GetUserByEmail(string email);
        public AspNetUser GetUserById(string id);
        public List<RequestandRequestClient> getFilterByName(IEnumerable<RequestandRequestClient> r, string patient_name);
        public List<RequestandRequestClient> getByRequesttypeId(IEnumerable<RequestandRequestClient> r, int requesttypeId);
        public Physician getProviderInfo(string email);
        public List<RequestandRequestClient> getFilterByrequestTypeAndName(IEnumerable<RequestandRequestClient> r, int requesttypeId, string patient_name);
        public void providerAccept(int requestId, string email);
        public RequestClient getPatientInfo(int requestId);
        public string getConfirmationNumber(int requestId);

        public viewNotes getNotes(int requestId, string email);
        public void providerNotes(int requestId, viewNotes v, string email);
        public void providerTransferCase(string requestId, string additionalNotesTransfer, string email);
        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId);
        public string getName(string requestId);

        public void UploadFiles(int requestId, List<IFormFile> files, string email);
        public RequestWiseFile GetFileById(int fileId);
        public void DeleteFile(int fileId);
        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds);
        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId);
        public void GetFilesByIdsDelete(List<int> fileIds);
        public void GetFilesByRequestIdDelete(int requestId);
        public string GetPatientEmail(int requestId);
        public List<string> GetSelectedFiles(List<int> ids);
        public List<string> GetAllFiles(int requestId);
        public void insertEmailLog(string? emailTemplate, string? subjectName, string? emailId, int? requestId, string sessionemail, string? filePath);
        public List<string> adminSendAgreementGet(string requestId);
        public List<HealthProfessionalType> GetAllHealthProfessionalType();
        public List<HealthProfessional> GetAllHealthProfessional();
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId);
        public HealthProfessional GetProfessionInfo(int vendorId);
        public void sendOrderDetails(int requestId, sendOrder s, string email);
        public void providerEncounterCase(int requestId, string calltype, string email);
        public encounterModel providerEncounterForm(int requestId);
        public void providerEncounterFormPost(int requestId, encounterModel em);
        public void transferToConcludeState(int requestId);

        public void providerIsFinal(int requestId);
        public void providerConcludeCarePost(int requestId, string notes, string email);
        public Physician getPhysicianDetails(int physicianId);
        public List<Region> getPhysicianRegions(string email);
        public void physicianUpdatePassword(string email, string password);
        public List<Physician> GetAllPhysicians();
        public List<Region> getAllRegions();
        public List<ShiftDetailsModel> getshiftDetail(string email);
        public ShiftDetail getShiftDetailByShiftDetailId(int id);
        public Shift getShiftByID(int shiftid);
        public ShiftDetailsModel getViewShiftData(int id);
    }
}

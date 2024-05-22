using HalloDoc.DataAccessLayer.DataModels;
using HalloDoc.DataAccessLayer.DataModels.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepository
{
    public interface IPatientRepository
    {
        AspNetUser ValidateUser(string email, string password);
        AspNetUser GetUserByEmail(string email);
        //public void CreateRequest(createPatientRequest RequestData);
        //public string CreateFamilyRequest(familyCreateRequest RequestData);
        //public string CreateConciergeRequest(conciergeCreateRequest RequestData);
        //public string CreateBusinessRequest(businessCreateRequest RequestData);
        public string getRoleName(AspNetUser asp);
        public void InsertPatientInfo(patientInfo RequestData, AspNetUser asp, User data, Request req, RequestClient rc);
        public void CreateRequest(patientInfo RequestData);
        public string CreateFamilyRequest(patientInfo RequestData);
        public string CreateConciergeRequest(patientInfo RequestData);
        public string CreateBusinessRequest(patientInfo RequestData);
        public List<Request> GetbyEmail(string email);
        public void agreementApproved(int requestId, int? adminId, int? physicianId);
        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId);
        public void UploadFiles(int requestId, List<IFormFile> files);

        public string getName(string requestId);
        public string getConfirmationNumber(string requestId);
        public RequestWiseFile GetFileById(int fileId);
        public User GetPatientData(string email);
        public IEnumerable<RequestWiseFile> GetAllFiles();
        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds);
        public void createPatientRequestMe(createPatientRequest RequestData);

        public void createPatientRequestSomeoneElse(string email,requestSomeoneElse r);
        public void updateProfile(string email,User u);

        public Physician getPhysicianDetails(int physicianId);
        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId);
        public GroupsMain getGroupMainDetails(int requestId);
        public void InsertGroupMains(int requestId);
        public string getCurrentUserAspId(string email);

    }
}

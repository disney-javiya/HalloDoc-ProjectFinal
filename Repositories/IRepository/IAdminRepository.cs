using HalloDoc.DataAccessLayer.DataModels;
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
    public interface IAdminRepository
    {
        AspNetUser ValidateUser(string email, string password);
        public IEnumerable<RequestandRequestClient> getRequestStateData(int type);
        public string getRoleName(AspNetUser asp);
        public AspNetUser GetUserByEmail(string email);
        public RequestClient getPatientInfo(int requestId);
        public int GetUserByRequestId(string Id);
        public string getConfirmationNumber(int requestId);

        public List<RequestandRequestClient> getFilterByRegions(IEnumerable<RequestandRequestClient> r, int regionId);

        public List<RequestandRequestClient> getByRequesttypeId(IEnumerable<RequestandRequestClient> r, int requesttypeId);
        public List<RequestandRequestClient> getFilterByName(IEnumerable<RequestandRequestClient> r, string patient_name);

        public List<RequestandRequestClient> getFilterByRegionAndName(IEnumerable<RequestandRequestClient> r, string patient_name, int regionId);

        public List<RequestandRequestClient> getByRequesttypeIdRegionAndName(IEnumerable<RequestandRequestClient> r, int requesttypeId, int? regionId, string? patient_name);
        public void adminNotes(int requestId, viewNotes v, string email);
        //public void adminCancelNote([FromBody] viewNotes viewNoteData, string email);
        public void adminCancelNote(string requestId, string reason, string additionalNotes, string email);

        public string getName(string requestId);

        public string getConfirmationNumber(string requestId);
        public viewNotes getNotes(int requestId, string email);
        public List<Physician> GetPhysicians(int? regionId);
        public void adminAssignNote(string requestId, string region, string physician, string additionalNotesAssign, string email);

        public void adminBlockNote(string requestId, string additionalNotesBlock, string email);

        public void adminTransferCase(string requestId, string physician, string additionalNotesTransfer, string email);

        public void patientCancelNote(string requestId, string additionalNotesPatient);
        public List<RequestWiseFile> GetDocumentsByRequestId(int requestId);

        public List<Region> getAllRegions();
      
        public void UploadFiles(int requestId, List<IFormFile> files, string email);
        public RequestWiseFile GetFileById(int fileId);
        public void DeleteFile(int fileId);
        public IEnumerable<RequestWiseFile> GetFilesByRequestId(int requestId);
        public IEnumerable<RequestWiseFile> GetFilesByIds(List<int> fileIds);

        public void GetFilesByIdsDelete(List<int> fileIds);
        public void GetFilesByRequestIdDelete(int requestId);
        public string GetPatientEmail(int requestId);
        public List<string> GetAllFiles(int requestId);

        public List<string> GetSelectedFiles(List<int> ids);
        public void sendOrderDetails(int requestId, sendOrder s, string email);
        public List<HealthProfessionalType> GetAllHealthProfessionalType();
        public List<HealthProfessional> GetAllHealthProfessional();
        public List<HealthProfessional> GetHealthProfessionals();
        public List<string> GetNameConfirmation(int requestId);
        public List<HealthProfessional> GetHealthProfessional(int healthprofessionalId);
        public HealthProfessional GetProfessionInfo(int vendorId);

        public void adminClearCase(string requestId, string email);
        public List<string> adminSendAgreementGet(string requestId);

        public void closeCaseAdmin(int requestId, string email);

        public string adminTransferNotes(string requestId, string email);
        public Admin getAdminInfo(string email);
        public string adminCreateRequest(createAdminRequest RequestData, string email);
        public void passwordresetInsert(string Email, string id);

        public Passwordreset getPasswordReset(string token);
        public List<Region> getAdminRegions(string email);
        public void ResetPassword(ResetPasswordVM obj);


        public void adminProfileUpdatePassword(string email, string password);  
        public void adminProfileUpdateStatus(string email, Admin a);
        public void adminUpdateProfile(string email, Admin a, string uncheckedCheckboxes);
        public void adminUpdateProfileBilling(string email, Admin a);
        public void createPhysicianAccount(Physician p, IFormFile photo, string password, string role, List<int> region, string email, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc);
        public List<Physician> GetAllPhysicians();
        //public List<Physician> getPhysiciansbyRegion(int? regionId);
        public List<Region> getPhysicianRegions(int physicianId);
        public Physician getPhysicianDetails(int physicianId);
        public void physicianUpdateStatus(string email, int physicianId, Physician p);
        public void physicianUpdatePassword(string email, int physicianId, string password);
        public void physicianUpdateAccount(string email, int physicianId, Physician p, string uncheckedCheckboxes);
        public void physicianUpdateBilling(string email, int physicianId, Physician p);

        public void physicianUpdateBusiness(string email, int physicianId, Physician p, IFormFile[] files, IFormFile? photo, IFormFile? signature);

        public void physicianUpdateUpload(string email, int physicianId, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc);
        public List<Role> GetPhysiciansRoles();
        public void deletePhysicianAccount(string email, int physicianId);
        public List<Menu> menuByAccountType(int accountType);
        //public void physicianUpdateBusiness(string email, int physicianId, Physician p, IFormFile? profilePhoto, IFormFile? signaturePhoto, IFormFile? agreementDoc, IFormFile? backgroundDoc, IFormFile? hippaDoc, IFormFile? disclosureDoc, IFormFile? licenseDoc);

        public void createRole(createRole r, List<string> menu, string email);

        public List<Role> GetAllRoles();
        public void adminDeleteRole(int roleId, string email);

        public Role getRoleData(int roleId);
        public List<int> menuByAccountTypeRoleId(int accountType, int roleId);
        public void updateRole(Role r, List<int> menu, int roleId, string email);

        public void createAdmin(Admin a, string password, List<int> region, string role, string email);

        public List<Role> GetAdminsRoles();

        public void insertShift(shiftViewModel s, string checktoggle, int[] dayList, string email);
        public List<PhysicianLocation> getAllPhysicianLocation();
        public void addHealthProfessional(HealthProfessional h, string Profession);
        public void editBusinessPost(int VendorId ,HealthProfessional h);
        public List<HealthProfessional> filterPartnersPage(int? healthprofessionId, string? vendor_name);
        public void adminDeletePartner(int VendorId, string email);
        public List<BlockRequest> GetAllBlockRequests();
        public List<BlockRequest> filterBlockedHistory(string? patientName, DateOnly? date, string? email, string? phone);
        public void unblockPatient(int RequestId, string email);
        public List<User> patientHistory();
        public List<User> filterPatientHistory(string? patientFirstName, string? patientLastName, string? email, string? phone);
        public List<Request> explorePatientHistory(int UserId);
        public List<searchRecords> searchRecords(string email);
        public List<searchRecords> filterSearchRecords(string sessionemail, int? requestStatus, string? patientName, int? requestType, DateOnly? fromDate, DateOnly? toDate, string? providerName, string? email, string? phone);
        public void insertEmailLog(string? emailTemplate, string? subjectName, string? emailId, int? requestId, string sessionemail, string? filePath);
        public void insertSMSLog(string? SMSTemplate, string? mobile, int? requestId, string sessionemail);

        public List<EmailLog> emailLogs();


        public List<EmailLog> filterEmailLogs(int? role, string? recieverName, string? email, DateOnly? createdDate, DateOnly? sentDate);
        public List<Smslog> SMSLogs();

        public List<Smslog> filterSMSLogs(int? role, string? recieverName, string? mobile, DateOnly? createdDate, DateOnly? sentDate);

        //public List<ShiftDetailsModel> getshiftDetail();
        public List<ShiftDetailsModel> getshiftDetail(int? regionId);
        public ShiftDetailsModel getViewShiftData(int id);
        public Shift getShiftByID(int shiftid);
        public List<Physician> getPhysicianListByregion(int regid);
        public ShiftDetail getShiftDetailByShiftDetailId(int id);
        public void UpdateShiftDetailData(ShiftDetailsModel model, string email);
        public void DeleteShiftDetails(int id, string email);


        public void UpdateShiftDetailTable(ShiftDetail sd);
        public void UpdateShiftDetailsStatus(int id);
        public Admin getAdminTableDataByEmail(string? email);
        public ShiftDetailsModel getReviewShiftData(int reg);
        public void ApproveShift(string[] selectedShifts);
        public void DeleteShift(string[] selectedShifts);
        public ShiftDetailsModel SchedulingMonth(int monthNum);

        public ShiftDetailsModel getProviderOnCall(int reg);

        public List<Physician> getPhysicianOnCallList(int reg);

        public List<userAccessModel> userAccess();
        public List<userAccessModel> userAccessSearch(string? accounttype);
        public List<int> getPhysicianNotification();
        public void deleteRequest(int requestId);

        //public void updatePhysicianNotification(List<int> phy_ids);
        public encounterModel adminEncounterForm(int requestId);
        public void adminEncounterFormPost(int requestId, encounterModel em);
        public encounterModel getEncounterDetails(int requestId);
        public byte[] GeneratePDF(encounterModel encounter);
        public bool IsUserExists(string email);
        public string GetRequestName(int RequestId);
        public List<RequestClient> GetAllRequestClient();
    }
}

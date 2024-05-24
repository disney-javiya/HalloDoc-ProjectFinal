using Microsoft.AspNetCore.SignalR;
using Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HalloDoc.DataAccessLayer.DataContext;
using DocumentFormat.OpenXml.InkML;
using HalloDoc.DataAccessLayer.DataModels;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Commons.Actions.Contexts;
using NuGet.Protocol.Plugins;

namespace HalloDoc.Hubs{
    public class ChatHub: Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatRepository _chatRepository;
        private readonly IAuthenticateRepository _authenticate;
        private readonly ApplicationDbContext _context;
        public ChatHub(ILogger<ChatHub> logger, IChatRepository chatRepository, IAuthenticateRepository authenticate, ApplicationDbContext context)
        {
            _logger = logger;
            _chatRepository = chatRepository;
            _authenticate = authenticate;
            _context = context;
        }

        public void SendMessage(string Sender, string SenderType, string Receiver, string ReceiverType, string Receiver2)
        {
            string senderId = "0",receiverId = "0", receiver2Id = "0";
            List<Chat> data = new();
            switch (SenderType)
            {
                case "Admin":
                    senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
                    break;
                case "Patient":
                    senderId = Sender;
                    break;
                case "Provider":
                    senderId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Sender)).AspNetUserId;
                    break;

            }
            switch (ReceiverType)
            {
                case "Admin":
                    receiverId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Receiver)).AspNetUserId;
                    break;
                case "Patient":
                    receiverId = Receiver;
                    break;
                case "Provider":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    break;
                case "AdminGroup":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    receiver2Id = Receiver2;
                    break;
                case "ProviderGroup":
                    receiverId = Receiver;
                    receiver2Id = Receiver2;
                    break;
                case "PatientGroup":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    receiver2Id = Receiver2;
                    break;
            }
            if (ReceiverType == "PatientGroup" || ReceiverType == "ProviderGroup" || ReceiverType == "AdminGroup")
            {
                data = _context.Chats.Where(e =>
                (e.Senderid == senderId || e.Receiverid == senderId || e.Receiver2id == senderId)
                &&
                (e.Senderid == receiver2Id || e.Receiverid == receiver2Id || e.Receiver2id == receiver2Id)
                &&
                (e.Senderid == receiverId || e.Receiverid == receiverId || e.Receiver2id == receiverId) && e.IsGroup == true).OrderBy(e => e.Sentdate).ToList();
            }
            else
            {
                data = _context.Chats.Where(e => (e.Senderid == senderId || e.Receiverid == senderId) && (e.Senderid == receiverId || e.Receiverid == receiverId) && e.IsGroup == false).ToList();
            }
            // to All clients
            //Clients.All.SendAsync("ReceiveMessage", data);
            //to single client
            Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", data);

        }
        public void SaveData(string Sender, string SenderType, string Receiver, string ReceiverType, string message, string Receiver2)
        {
            string senderId = "0", receiverId = "0", receiver2Id = "0";
            switch (SenderType)
            {
                case "Admin":
                    senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
                    break;
                case "Patient":
                    senderId = Sender;
                    break;
                case "Provider":
                    senderId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Sender)).AspNetUserId;
                    break;
            }
            switch (ReceiverType)
            {
                case "Admin":
                    receiverId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Receiver)).AspNetUserId;
                    break;
                case "Patient":
                    receiverId = Receiver;
                    break;
                case "Provider":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    break;
                case "AdminGroup":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    receiver2Id = Receiver2;
                    break;
                case "ProviderGroup":
                    receiverId = Receiver;
                    receiver2Id = Receiver2;
                    break;
                case "PatientGroup":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    receiver2Id = Receiver2;
                    break;
            }
            Chat chat = new Chat
            {
                Senderid = senderId,
                Receiverid = receiverId,
                Message = message,
                Sentdate = DateTime.Now,
                Senttime = TimeOnly.FromDateTime(DateTime.Now),
                IsGroup = false
            };
            if (ReceiverType == "PatientGroup" || ReceiverType == "ProviderGroup" || ReceiverType == "AdminGroup")
            {
                chat.Receiver2id = Receiver2;
                chat.IsGroup = true;
            }
            _context.Chats.Add(chat);
            _context.SaveChanges();
        }


    }
}

//public void SendMessage(string Sender, string SenderType, string Receiver, string ReceiverType)
//{
//    string senderId = "", receiverId = "";
//    switch (SenderType)
//    {
//        case "Admin":
//            senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
//            break;
//        case "Patient":
//            //senderId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Sender)).User.AspNetUserId; 
//            senderId = Sender;
//            break;
//        case "Provider":
//            senderId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Sender)).AspNetUserId;
//            break;
//    }
//    switch (ReceiverType)
//    {
//        case "Admin":
//            receiverId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Receiver)).AspNetUserId;
//            break;
//        case "Patient":
//            //receiverId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Receiver)).User.AspNetUserId;
//            receiverId = Receiver;
//            break;
//        case "Provider":
//            receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
//            break;
//    }
//    List<Chat> data = _context.Chats.Where(e => (e.Senderid == senderId || e.Receiverid == senderId) && (e.Senderid == receiverId || e.Receiverid == receiverId)).ToList();
//    //var data =_context.Chats.Select(e=> e.Message).ToList();
//    Clients.All.SendAsync("ReceiveMessage", data);
//    //await Clients.All.SendAsync("ReceiveMessage", user, message);
//}
//public void SaveData(string Sender, string SenderType, string Receiver, string ReceiverType, string message)
//{
//    string senderId = "", receiverId = "";
//    switch (SenderType)
//    {
//        case "Admin":
//            senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
//            break;
//        case "Patient":
//            //senderId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Sender)).User.AspNetUserId;
//            senderId = Sender;
//            break;
//        case "Provider":
//            senderId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Sender)).AspNetUserId;
//            break;
//    }
//    switch (ReceiverType)
//    {
//        case "Admin":
//            receiverId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Receiver)).AspNetUserId;
//            break;
//        case "Patient":
//            //receiverId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Receiver)).User.AspNetUserId;
//            receiverId = Receiver;
//            break;
//        case "Provider":
//            receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
//            break;
//    }

//    Chat chat = new Chat
//    {
//        Senderid = senderId,
//        Receiverid = receiverId,
//        Message = message,
//        Sentdate = DateTime.Now,
//        Senttime = TimeOnly.FromDateTime(DateTime.Now)
//    };
//    _context.Chats.Add(chat);
//    _context.SaveChanges();
//}

//public void GroupSendMessage(string GroupId)
//{
//    GroupsMain groupsMain = _context.GroupsMains.FirstOrDefault(g => g.GroupId == int.Parse(GroupId));
//    List<GroupChat> data = _context.GroupChats.Where(e => e.GroupId == int.Parse(GroupId)).ToList();
//    Clients.All.SendAsync("ReceiveMessage", data);
//}

//public void GroupSaveData(string GroupId, string message, string RequestId, string PhysicianId, string AdminId, string Senderid)
//{


//    GroupChat chat = new GroupChat
//    {
//        GroupId = int.Parse(GroupId),
//        RequestId = int.Parse(RequestId),
//        PhysicianId = int.Parse(PhysicianId),
//        AdminId = int.Parse(AdminId),
//        Message = message,
//        Senderid = Senderid,
//        SentDate = DateTime.Now,
//        SentTime = TimeOnly.FromDateTime(DateTime.Now)
//    };
//    _context.GroupChats.Add(chat);
//    _context.SaveChanges();
//}











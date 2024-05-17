using Microsoft.AspNetCore.SignalR;
using Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HalloDoc.DataAccessLayer.DataContext;
using DocumentFormat.OpenXml.InkML;
using HalloDoc.DataAccessLayer.DataModels;

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


        public void SendMessage(string Sender, string SenderType, string Receiver, string ReceiverType)
        {
            string senderId = "", receiverId = "";
            switch (SenderType)
            {
                case "Admin":
                    senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
                    break;
                case "Patient":
                    //senderId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Sender)).User.AspNetUserId; 
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
                    //receiverId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Receiver)).User.AspNetUserId;
                    receiverId= Receiver;
                    break;
                case "Provider":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    break;
            }
            List<Chat> data = _context.Chats.Where(e => (e.Senderid == senderId || e.Receiverid == senderId) && (e.Senderid == receiverId || e.Receiverid == receiverId)).ToList();
            //var data =_context.Chats.Select(e=> e.Message).ToList();
            Clients.All.SendAsync("ReceiveMessage", data);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public void SaveData(string Sender, string SenderType, string Receiver, string ReceiverType, string message)
        {
            string senderId = "", receiverId = "";
            switch (SenderType)
            {
                case "Admin":
                    senderId = _context.Admins.FirstOrDefault(e => e.AdminId == int.Parse(Sender)).AspNetUserId;
                    break;
                case "Patient":
                    //senderId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Sender)).User.AspNetUserId;
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
                    //receiverId = _context.Requests.FirstOrDefault(e => e.RequestId == int.Parse(Receiver)).User.AspNetUserId;
                    receiverId = Receiver;
                    break;
                case "Provider":
                    receiverId = _context.Physicians.FirstOrDefault(e => e.PhysicianId == int.Parse(Receiver)).AspNetUserId;
                    break;
            }

            Chat chat = new Chat
            {
                Senderid = senderId,
                Receiverid = receiverId,
                Message = message,
                Sentdate = DateTime.Now,
                Senttime = TimeOnly.FromDateTime(DateTime.Now)
            };
            _context.Chats.Add(chat);
            _context.SaveChanges();
        }
       
    }
}


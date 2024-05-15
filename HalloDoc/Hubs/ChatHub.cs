using Microsoft.AspNetCore.SignalR;
using Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HalloDoc.Hubs{
    public class ChatHub: Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatRepository _chatRepository;
        private readonly IAuthenticateRepository _authenticate;
        public ChatHub(ILogger<ChatHub> logger, IChatRepository chatRepository, IAuthenticateRepository authenticate)
        {
            _logger = logger;
            _chatRepository = chatRepository;
            _authenticate = authenticate;
        }
        public string GetAspId(string phyId)
        {
            string Id;
           
            Id = _chatRepository.GetAspId( int.Parse(phyId));
            return Id;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.Users(user).SendAsync("ReceiveMessage", user, message);
        }
    }
}


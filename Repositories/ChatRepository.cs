using HalloDoc.DataAccessLayer.DataContext;
using HalloDoc.DataAccessLayer.DataModels;
using Repository.IRepository;
namespace Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string GetAspId(int phyId)
        {
           return _context.Physicians.Where(x=>x.PhysicianId == phyId).Select(x=>x.AspNetUserId).FirstOrDefault();
        }

       
        
    }
}



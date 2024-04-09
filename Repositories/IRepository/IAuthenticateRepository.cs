using HalloDoc.DataAccessLayer.DataModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepository
{
    public interface IAuthenticateRepository
    {
        public string GenerateJwtToken(AspNetUser user, string role);
        public bool ValidateToken(string token, out JwtSecurityToken validatedToken);
    }
}

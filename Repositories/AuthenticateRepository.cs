﻿using DocumentFormat.OpenXml.Spreadsheet;
using HalloDoc.DataAccessLayer.DataContext;
using HalloDoc.DataAccessLayer.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Repository
{
    public class AuthenticateRepository : IAuthenticateRepository
    {
        private readonly string _secret;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly AdminRepository _adminRepository;

        public AuthenticateRepository(IConfiguration configuration, ApplicationDbContext context, IAdminRepository adminRepository)
        {
            _configuration = configuration;
            _context = context;
            adminRepository = adminRepository;
        }
        public string GenerateJwtToken(AspNetUser user, string role)
        {

           
            int roleId = 0;
            if (role == "Admin")
            {
                Admin admin = _context.Admins.Where(x => x.AspNetUserId == user.Id).FirstOrDefault();
                roleId = (int)admin.RoleId;
            }
            else if (role == "Physician")
            {
                Physician physician = _context.Physicians.Where(x => x.AspNetUserId == user.Id).FirstOrDefault();
                roleId = (int)physician.RoleId;
            }

            List<string> menu = _context.RoleMenus.Where(x => x.RoleId == roleId).Select(x => x.Menu.Name).ToList();
            var claims = new List<Claim> {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserId", user.Id.ToString()),
            new Claim("UserName", user.UserName),
             new Claim("Menu",JsonSerializer.Serialize(menu))
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var expires = DateTime.UtcNow.AddMinutes(20);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
             );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public bool ValidateToken(string token, out JwtSecurityToken validatedToken)
        {
            validatedToken = null;

            if (token == null)
            {
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                validatedToken = securityToken as JwtSecurityToken;

                

                return true;
            }
            catch
            {
                return false;
            }
        }

       
    }
}

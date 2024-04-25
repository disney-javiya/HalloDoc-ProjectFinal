using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Repository.IRepository;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HalloDoc.AuthMiddleware
{
    public class CustomeAuthorize : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public CustomeAuthorize(string role)
        {
            this._role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            var jwtTokenService = context.HttpContext.RequestServices.GetService<IAuthenticateRepository>();

            if (jwtTokenService == null)
            {
                context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                {
                    Controller = "Home",
                    Action = "Index",
                }));
                return;
            }

            var request = context.HttpContext.Request;
            var token = request.Cookies["jwt"];

            if (token == null || !jwtTokenService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                string returnURL = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                //context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                //{
                //    Controller = "Home",
                //    Action = "Index",
                //}));
                if (_role == "Admin" || _role == "Physician")
                {
                    context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                    {
                        Controller = "Admin",
                        Action = "Index",
                        returnUrl = returnURL
                    }));
                }
                else
                {
                    context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                    {
                        Controller = "Home",
                        Action = "Index",
                        returnUrl = returnURL
                    }));
                }
               
                return;
            }

            var roleClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var Id = jwtToken?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (roleClaim == null)
            {
                context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                {
                    Controller = "Home",
                    Action = "Index",
                }));
                return;
            }

            if (roleClaim != _role || string.IsNullOrWhiteSpace(_role))
            {
                
                context.Result = new RedirectToRouteResult(new Microsoft.AspNetCore.Routing.RouteValueDictionary(new
                {
                    Controller = "Home",
                    Action = "Index",
                }));
                return;
            }
        }
    }
}



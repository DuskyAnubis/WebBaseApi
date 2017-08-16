using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebBaseApi.Data;
using WebBaseApi.Common;
using WebBaseApi.Models;
using WebBaseApi.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;


namespace WebBaseApi.Controllers
{

    [Route("api/v1/[Controller]")]
    public class TokenController : Controller
    {
        private readonly JWTTokenOptions tokenOptions;
        private readonly ApiContext dbContext;

        public TokenController(JWTTokenOptions tokenOptions, ApiContext dbContext)
        {
            this.tokenOptions = tokenOptions;
            this.dbContext = dbContext;
        }

        private string CreatToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();

            string jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim("userId",user.Id.ToString()),
                new Claim("role",user.Role.Name),
                new Claim(JwtRegisteredClaimNames.Jti,jti)
            };
            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(user.Name, "TokenAuth"), claims);

            var token = handler.CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Issuer = tokenOptions.Issuer,
                Audience = tokenOptions.Audience,
                SigningCredentials = new SigningCredentials(tokenOptions.SecretKey, SecurityAlgorithms.HmacSha256),
                Expires = DateTime.Now.Add(tokenOptions.Expiration),
                Subject = identity
            });
            return token;
        }
        /// <summary>
        /// 用户登录，返回一个Token
        /// </summary>
        /// <param name="loginUser">登录信息</param>
        /// <returns>Token</returns>
        // POST: api/Token
        [HttpPost]
        public IActionResult Post([FromBody]LoginInputDto loginUser)
        {
            var user = dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Organazition)
                .FirstOrDefault(u => u.Name == loginUser.UserName && u.PassWord == Encrypt.Md5Encrypt(loginUser.PassWord));

            if (user == null)
            {
                return Json(new { Error = "用户名或密码错误！" });
            }
            return Json(new { Token = CreatToken(user) });
        }


    }
}

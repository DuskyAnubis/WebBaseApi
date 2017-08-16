using Microsoft.IdentityModel.Tokens;
using System;

namespace WebBaseApi.Common
{
    public class JWTTokenOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public TimeSpan Expiration { get; set; }
        public SymmetricSecurityKey SecretKey { get; set; }
    }
}

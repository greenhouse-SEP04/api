using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.Helpers
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration _cfg;
        public JwtTokenHelper(IConfiguration cfg) => _cfg = cfg;

        public string Generate(IEnumerable<Claim> claims, DateTime? exp = null)
        {
            var jwt = _cfg.GetSection("JWT");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));
            var token = new JwtSecurityToken(
                issuer: jwt["ValidIssuer"],
                audience: jwt["ValidAudience"],
                claims: claims,
                expires: exp ?? DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

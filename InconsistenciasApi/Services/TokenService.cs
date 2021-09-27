using InconsistenciasApi.Models.Inconsistencias.Responses;
using InconsistenciasApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InconsistenciasApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public ClaimsIdentity CrearClaimsUsuario(string usuario, string email)
        {
            // CREAMOS LOS CLAIMS //
            return new ClaimsIdentity(new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserName", usuario),
                new Claim("NameIdentifier", usuario),
                new Claim("Email", email)
            });
        }

        // GENERAMOS EL TOKEN CON LA INFORMACIÓN DEL USUARIO
        public TokenResponse GenerarTokenJWT(ClaimsIdentity claimsUsuario)
        {
            DateTime createdDate = DateTime.UtcNow.ToLocalTime();
            DateTime expirationDate = createdDate.AddMinutes(int.Parse(_configuration["JWT:ExpireTime"]));

            JwtHeader _Header = CrearHeader();
            JwtPayload _Payload = CrearContenido(claimsUsuario, expirationDate, createdDate);

            // GENERAMOS EL TOKEN //
            var _Token = new JwtSecurityToken(_Header, _Payload);
            var _jwtTokenString = new JwtSecurityTokenHandler().WriteToken(_Token);

            return new TokenResponse
            {
                AccessToken = _jwtTokenString,
                ExpirationDate = expirationDate,
                Error = null
            };
        }

        JwtHeader CrearHeader()
        {
            // CREAMOS EL HEADER //
            var _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var _signingCredentials = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var _Header = new JwtHeader(_signingCredentials);
            return _Header;
        }

        JwtPayload CrearContenido(ClaimsIdentity claimsUsuario, DateTime expirationDate, DateTime createdDate)
        {
            // CREAMOS EL PAYLOAD //
            return new JwtPayload(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claimsUsuario.Claims,
                    notBefore: createdDate,
                    expires: expirationDate
                );
        } 
    }
}

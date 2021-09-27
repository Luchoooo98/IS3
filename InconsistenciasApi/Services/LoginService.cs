using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InconsistenciasApi.Models.Inconsistencias.Requests;
using InconsistenciasApi.Models.Inconsistencias.Responses;
using InconsistenciasApi.Models.Enums.Errors;
using InconsistenciasApi.Services.Interfaces;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using InconsistenciasApi.Models.Inconsistencias.Classes;
using InconsistenciasApi.Models.Entities;

namespace InconsistenciasApi.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly MyContextDatabase _context;

        public LoginService(ILogger<LoginService> logger, IConfiguration configuration, ITokenService tokenService, MyContextDatabase context)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
            _context = context;
        }

        public ResponseGenericApi AutenticarUsuario(LoginRequest loginRequest)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                var userDb = _context.User.Where(x => x.Usuario == loginRequest.Usuario && x.Contraseña == loginRequest.Contraseña).FirstOrDefault();

                if(userDb != null)
                {
                    UsuarioInfo usuarioInfo = new() { Mail = userDb.Mail, Usuario = userDb.Usuario, Validado = true };
                    responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: usuarioInfo);
                }
                else
                {
                    responseGenericApi = TratarRespuesta(caso: Casos.ERROR_NO_DEVUELVE_NADA_DB.ToString());
                }
            }
            catch (Exception ex)
            {
                responseGenericApi = TratarRespuesta(caso: Casos.EXCEPCION.ToString());
                _logger.LogError($"Error: Message: {ex.Message} StackTrace: {ex.StackTrace} InnerException: {ex.InnerException}");
            }
            return responseGenericApi;
        }

        public ResponseGenericApi GenerarToken(UsuarioInfo usuarioInfo)
        {
            if (string.IsNullOrEmpty(usuarioInfo.Usuario) || string.IsNullOrEmpty(usuarioInfo.Mail))
                return TratarRespuesta(caso: Casos.ERROR_DATA.ToString());

            //creo claims
            var claims = _tokenService.CrearClaimsUsuario(usuarioInfo.Usuario, usuarioInfo.Mail);

            //genero token
            var tokenResponse = _tokenService.GenerarTokenJWT(claims);

            return TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: tokenResponse);
        }

        public ResponseGenericApi AutenticarToken(RefreshAccessTokenRequest refreshAccessTokenRequest)
        {
            return TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: ValidarTokenJWT(refreshAccessTokenRequest));
        }

        TokenResponse ValidarTokenJWT(RefreshAccessTokenRequest login)
        {
            DateTimeOffset expire = new();
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                var tokenHandler = new JwtSecurityTokenHandler();
                var date = tokenHandler.ReadJwtToken(login.Token).Claims.FirstOrDefault(x => x.Type == "exp").Value;
                expire = DateTimeOffset.FromUnixTimeSeconds(long.Parse(date)).ToLocalTime();

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    LifetimeValidator = ValidacionDeExpiracion
                };

                var principal = tokenHandler.ValidateToken(login.Token, tokenValidationParameters, out SecurityToken securityToken);
                if (principal == null)
                    return new TokenResponse() { Error = "Credenciales Invalidas del Usuario" };

                return _tokenService.GenerarTokenJWT(new ClaimsIdentity(principal.Identity));
            }
            catch (SecurityTokenValidationException)
            {
                return new TokenResponse() { Error = "Token No Expirado", AccessToken = login.Token, ExpirationDate = expire.LocalDateTime };
            }
            catch (Exception)
            {
                return new TokenResponse() { Error = "Token Invalido", AccessToken = login.Token, ExpirationDate = expire.LocalDateTime };
            }
        }
        static bool ValidacionDeExpiracion(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires != null)
            {
                if (DateTime.UtcNow > expires) return true;
            }
            return false;
        }

        #region OTROS
        private ResponseGenericApi TratarRespuesta(string caso, string codeMessage = null, object datosDevolucion = null)
        {
            ResponseGenericApi responseGenericApi = new();
            switch (caso)
            {
                case "EXITOSO":
                    responseGenericApi.CodeError = "0";
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{Messages.APP001}"];
                    responseGenericApi.Response = null;
                    break;
                case "EXITOSO_CODIGO":
                    responseGenericApi.CodeError = "0";
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{codeMessage}"];
                    responseGenericApi.Response = null;
                    break;
                case "EXITOSO_CON_DEVOLUCION_DATOS":
                    responseGenericApi.CodeError = "0";
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{Messages.APP001}"];
                    responseGenericApi.Response = datosDevolucion;
                    break;
                case "ERROR_CODE_MESSAGE":
                    responseGenericApi.CodeError = codeMessage;
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{codeMessage}"];
                    responseGenericApi.Response = null;
                    break;
                case "ERROR_DATA":
                    responseGenericApi.CodeError = Messages.EAPP003.ToString();
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{Messages.EAPP003}"];
                    responseGenericApi.Response = null;
                    break;
                case "EXCEPCION":
                    responseGenericApi.CodeError = Messages.EAPP001.ToString();
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{Messages.EAPP001}"];
                    responseGenericApi.Response = null;
                    break;
                case "ERROR_NO_DEVUELVE_NADA_DB":
                    responseGenericApi.CodeError = Messages.EAPP002.ToString();
                    responseGenericApi.ErrorMessage = _configuration[$"MENSAJES:{Messages.EAPP002}"];
                    responseGenericApi.Response = null;
                    break;
            }
            return responseGenericApi;
        }
        #endregion
    }
}

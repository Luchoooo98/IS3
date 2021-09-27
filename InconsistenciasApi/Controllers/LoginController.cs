using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using InconsistenciasApi.Models.Inconsistencias.Responses;
using InconsistenciasApi.Models.Inconsistencias.Requests;
using InconsistenciasApi.Services.Interfaces;
using InconsistenciasApi.Models.Inconsistencias.Classes;

namespace InconsistenciasApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ILoginService _loginService;

        public LoginController(ILogger<LoginController> logger, ILoginService loginService)
        {
            _logger = logger;
            _loginService = loginService;
        }

        #region Comprobacion del servicio
        [HttpGet]
        [AllowAnonymous]
        [Route("healtcheck")]
        public IActionResult Healtcheck()
        {
            _logger.LogInformation("Api llamada => Healtcheck");
            return Ok(true);
        }

        [HttpGet]
        [Route("autorizado")]
        public IActionResult Autorizado()
        {
            _logger.LogInformation("Api llamada => Autorizado");
            return Ok("Autorizado");
        }
        #endregion

        #region APIs Auth 
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var jsonRequest = JsonConvert.SerializeObject(loginRequest);
            _logger.LogInformation("Api llamada => Login");
            _logger.LogInformation("Api Input => " + jsonRequest);

            if (string.IsNullOrEmpty(loginRequest.Usuario) || string.IsNullOrEmpty(loginRequest.Contraseña))
                return BadRequest(loginRequest);

            var responseGenericApiAuntenticar = _loginService.AutenticarUsuario(loginRequest);

            if (responseGenericApiAuntenticar.CodeError != "0" && responseGenericApiAuntenticar.Response == null)
            {
                return Unauthorized(responseGenericApiAuntenticar);
            }
            else
            {
                var usuarioAutenticado = (UsuarioInfo)responseGenericApiAuntenticar.Response;

                if (usuarioAutenticado.Validado)
                {
                    ResponseGenericApi responseGenericApi = _loginService.GenerarToken(usuarioAutenticado);
                    var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
                    _logger.LogInformation("Api Output => " + jsonOutput);
                    return Ok(responseGenericApi);
                }
                else
                {
                    return Unauthorized();
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("validarToken")]
        public IActionResult ValidarToken(RefreshAccessTokenRequest refreshAccessTokenRequest)
        {
            var jsonRequest = JsonConvert.SerializeObject(refreshAccessTokenRequest);
            _logger.LogInformation("Api llamada => ValidarToken");
            _logger.LogInformation("Api Input => " + jsonRequest);

            if (string.IsNullOrEmpty(refreshAccessTokenRequest.Token))
                return BadRequest(refreshAccessTokenRequest);

            var responseGenericApi = _loginService.AutenticarToken(refreshAccessTokenRequest);

            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }
        #endregion
    }
}

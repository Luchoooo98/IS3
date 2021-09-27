using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using InconsistenciasApi.Models.Inconsistencias.Responses;
using InconsistenciasApi.Models.Inconsistencias.Requests;
using InconsistenciasApi.Services.Interfaces;
using InconsistenciasApi.Models.Inconsistencias.Classes;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using InconsistenciasApi.Models.Enums.Errors;
using System.IO;
using System;

namespace InconsistenciasApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ArchivoController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IArchivoService _archivoService;

        public ArchivoController(ILogger<LoginController> logger, IArchivoService archivoService)
        {
            _logger = logger;
            _archivoService = archivoService;
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

        #region APIs Archivo 
        [HttpPost]
        [Route("insertarArchivo")]
        public async Task<IActionResult> InsertarArchivo(IFormFile file)
        {
            var jsonRequest = JsonConvert.SerializeObject(file);
            _logger.LogInformation("Api llamada => InsertarArchivo");
            _logger.LogInformation("Api Input => " + jsonRequest);

            var usuario = ControllerContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserName").Value;

            ResponseGenericApi responseGenericApi = await _archivoService.InsertarArchivo(file, usuario);
            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }

        [HttpGet]
        [Route("traerArchivo")]
        public IActionResult TraerArchivo(int id)
        {
            var jsonRequest = JsonConvert.SerializeObject(id);
            _logger.LogInformation("Api llamada => TraerArchivo");
            _logger.LogInformation("Api Input => " + jsonRequest);

            ResponseGenericApi responseGenericApi = _archivoService.TraerArchivo(id);
            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }

        [HttpGet]
        [Route("traerArchivos")]
        public IActionResult TraerArchivos(string usuario)
        {
            var jsonRequest = JsonConvert.SerializeObject(usuario);
            _logger.LogInformation("Api llamada => TraerArchivos");
            _logger.LogInformation("Api Input => " + jsonRequest);

            ResponseGenericApi responseGenericApi = _archivoService.TraerArchivos(usuario);
            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }

        [HttpGet]
        [Route("traerArchivoTipoFile")]
        public IActionResult TraerArchivoTipoFile(int id)
        {
            var jsonRequest = JsonConvert.SerializeObject(id);
            _logger.LogInformation("Api llamada => TraerArchivoTipoFile");
            _logger.LogInformation("Api Input => " + jsonRequest);

            ResponseGenericApi responseGenericApi = _archivoService.TraerArchivoTipoFile(id);
            if (responseGenericApi.CodeError == "0" && responseGenericApi.Response != null)
            {
                var response = (Tuple<MemoryStream,string>)responseGenericApi.Response;
                _logger.LogInformation("Api Output => " + "OK");
                return File(response.Item1, "application/.txt", response.Item2);
            }
            else
            {
                var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
                _logger.LogInformation("Api Output => " + jsonOutput);
                return Ok(responseGenericApi);
            }
        }

        [HttpPost]
        [Route("procesarArchivoPorId")]
        public IActionResult ProcesarArchivoPorId(int id, Procesamiento opcion)
        {
            var jsonRequest = JsonConvert.SerializeObject(id);
            _logger.LogInformation("Api llamada => ProcesarArchivoPorId");
            _logger.LogInformation("Api Input => " + jsonRequest);

            ResponseGenericApi responseGenericApi = _archivoService.ProcesarArchivo(id, opcion);
            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("procesarArchivoPorArchivo")]
        public IActionResult ProcesarArchivoPorArchivo(IFormFile file, Procesamiento opcion)
        {
            var jsonRequest = JsonConvert.SerializeObject(file);
            _logger.LogInformation("Api llamada => ProcesarArchivoPorArchivo");
            _logger.LogInformation("Api Input => " + jsonRequest);

            ResponseGenericApi responseGenericApi = _archivoService.ProcesarArchivo(file, opcion);
            var jsonOutput = JsonConvert.SerializeObject(responseGenericApi);
            _logger.LogInformation("Api Output => " + jsonOutput);
            return Ok(responseGenericApi);
        }
        #endregion
    }
}

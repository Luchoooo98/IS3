using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InconsistenciasApi.Models.Inconsistencias.Responses;
using InconsistenciasApi.Models.Enums.Errors;
using InconsistenciasApi.Services.Interfaces;
using InconsistenciasApi.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Linq;
using InconsistenciasApi.Models.Inconsistencias.Classes;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace InconsistenciasApi.Services
{
    public class ArchivoService : IArchivoService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly MyContextDatabase _context;

        public ArchivoService(ILogger<ArchivoService> logger, IConfiguration configuration, MyContextDatabase context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        #region Insercion de Archivo
        public async Task<ResponseGenericApi> InsertarArchivo(IFormFile file, string usuario)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                if (file == null || file.Length == 0)
                {
                    return responseGenericApi = TratarRespuesta(caso: Casos.ERROR_DATA.ToString());
                }

                var bytes = await file.GetBytes();
                string hash = MD5Hash(bytes);

                var lines = file.ReadAsList().ToList();
                List<ReglasArchivo> reglasArchivo = new();
                lines.ForEach(l =>
                {
                    reglasArchivo.Add(new ReglasArchivo { Regla = l });
                });

                var rules = FileHelper.GetRuleList(file);

                var tamanio = (double)file.Length;
                tamanio /= 1000000;
                tamanio = Math.Round(tamanio, 2);


                List<ResultadoArchivo> resultadoArchivo = new();
                List<ArchivoProcesadoResponse> procesadoLogicaInsercion = ProcesarLogicaInsercion(rules);
                procesadoLogicaInsercion.ForEach(a =>
                {
                    resultadoArchivo.Add(new ResultadoArchivo {Reglas = JsonConvert.SerializeObject(a.Reglas), Resultado = JsonConvert.SerializeObject(a.Resultado), TipoProcesamiento = a.TipoProcesamiento });
                });

                Archivo archivo = new()
                {
                    Alta = DateTime.UtcNow,
                    HashArchivo = hash,
                    NombreArchivo = file.FileName,
                    Usuario = usuario,
                    ContenidoArchivo = JsonConvert.SerializeObject(rules),
                    Tamanio = tamanio,
                    ReglasArchivo = reglasArchivo,
                    ResultadoArchivo = resultadoArchivo,
                    Bytes = bytes
                };

                _context.Archivo.Add(archivo);
                _context.SaveChanges();

                var id_registro = archivo.Id;

                responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(),
                    datosDevolucion: new ArchivoGuardadoResponse { IdInsercion = id_registro, NombreArchivo = file.FileName, Tamanio = tamanio });
                responseGenericApi.ErrorMessage = $"El archivo {file.FileName} fue guardado con un peso aproximado de: {tamanio} MB. El id de insercion es: {id_registro}";
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("UQ_HashArchivo"))
                    return TratarRespuesta(caso: Casos.ERROR_CODE_MESSAGE.ToString(), codeMessage: Messages.EAPP004.ToString());
                responseGenericApi = TratarRespuesta(caso: Casos.EXCEPCION.ToString());
                _logger.LogError($"Error: Message: {ex.Message} StackTrace: {ex.StackTrace} InnerException: {ex.InnerException}");
            }

            return responseGenericApi;
        }

        private static List<ArchivoProcesadoResponse> ProcesarLogicaInsercion(List<Rule> rules)
        {
            List<ArchivoProcesadoResponse> resultadoResponse = new();
            if (rules?.Count > 0)
            {
                var equalAntecedentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, true);
                var equalConsequentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, false);
                var reglas = FunctionHelper.PrintRuleList(rules);
                List<int> recorridoProcesado = new() { 1, 2, 3, 4, 5 };
                List<string> resultado = new();

                foreach (int i in recorridoProcesado)
                {
                    ArchivoProcesadoResponse archivoProcesado = new();
                    archivoProcesado.Reglas = reglas;

                    switch (i)
                    {
                        case 1:
                            archivoProcesado.Resultado = FunctionHelper.FindReglasRedundates(equalAntecedentRules);
                            archivoProcesado.TipoProcesamiento = Procesamiento.ReglasRedundates.ToString();
                            break;
                        case 2:
                            archivoProcesado.Resultado = FunctionHelper.FindReglasConflictivas(equalAntecedentRules);
                            archivoProcesado.TipoProcesamiento = Procesamiento.ReglasConflictivas.ToString();
                            break;
                        case 3:
                            archivoProcesado.Resultado = FunctionHelper.FindReglasIncluidasEnOtras(equalConsequentRules);
                            archivoProcesado.TipoProcesamiento = Procesamiento.ReglasIncluidasEnOtras.ToString();
                            break;
                        case 4:
                            archivoProcesado.Resultado = FunctionHelper.FindCondicionesSiInnecesarias(equalConsequentRules);
                            archivoProcesado.TipoProcesamiento = Procesamiento.CondicionesSiInnecesarias.ToString();
                            break;
                        case 5:
                            resultado.AddRange(FunctionHelper.FindReglasRedundates(equalAntecedentRules));
                            resultado.AddRange(FunctionHelper.FindReglasConflictivas(equalAntecedentRules));
                            resultado.AddRange(FunctionHelper.FindReglasIncluidasEnOtras(equalConsequentRules));
                            resultado.AddRange(FunctionHelper.FindCondicionesSiInnecesarias(equalConsequentRules));
                            archivoProcesado.Resultado = resultado;
                            archivoProcesado.TipoProcesamiento = Procesamiento.Todas.ToString();
                            break;
                    }
                    resultadoResponse.Add(archivoProcesado);
                }
            }
            return resultadoResponse;
        }
        #endregion

        #region Traer Archivos
        public ResponseGenericApi TraerArchivo(int id)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                var archivoDb = _context.Archivo.Include(x=>x.ReglasArchivo).Include(x=>x.ResultadoArchivo)
                    .Where(x => x.Id == id).FirstOrDefault();

                if (archivoDb != null)
                {
                    var reglas = JsonConvert.DeserializeObject<List<Rule>>(archivoDb.ContenidoArchivo);
                    var reglasArchivo = archivoDb.ReglasArchivo;
                    var resultadoArchivo = archivoDb.ResultadoArchivo;

                    if (reglas?.Count > 0 && reglasArchivo?.Count > 0 && resultadoArchivo?.Count > 0)
                    {
                        List<ArchivoProcesadoResponse> archivoProcesadoResultado = new();
                        ReglasProcesadoResponse reglasProcesadoResponse = new();

                        reglasArchivo.ForEach(r =>
                        {
                            reglasProcesadoResponse.Reglas.Add(r.Regla);
                        });

                        resultadoArchivo.ForEach(a =>
                        {
                            archivoProcesadoResultado.Add(new ArchivoProcesadoResponse { Reglas = JsonConvert.DeserializeObject<List<string>>(a.Reglas), Resultado = JsonConvert.DeserializeObject<List<string>>(a.Resultado), TipoProcesamiento = a.TipoProcesamiento});
                        });

                        ArchivoDetalleResponse archivoDetalleResponse = new()
                        {
                            FechaAlta = archivoDb.Alta,
                            IdInsercion = archivoDb.Id,
                            NombreArchivo = archivoDb.NombreArchivo,
                            Usuario = archivoDb.Usuario,
                            Tamanio = archivoDb.Tamanio,
                            Reglas = reglas,
                            ReglasArchivo = reglasProcesadoResponse,
                            ResultadoArchivo = archivoProcesadoResultado
                        };

                        responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: archivoDetalleResponse);
                    }
                    else
                    {
                        responseGenericApi = TratarRespuesta(caso: Casos.ERROR_DATA.ToString());
                    }
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

        public ResponseGenericApi TraerArchivos(string usuario)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                var archivoDb = _context.Archivo.Include(x => x.ReglasArchivo).Include(x => x.ResultadoArchivo)
                    .Where(x => x.Usuario == usuario).ToList();

                if (archivoDb?.Count > 0)
                {
                    List<ArchivoDetalleResponse> archivoDetalleResponseList = new();

                    archivoDb.ForEach(archivo =>
                    {
                        var reglas = JsonConvert.DeserializeObject<List<Rule>>(archivo.ContenidoArchivo);
                        var reglasArchivo = archivo.ReglasArchivo;
                        var resultadoArchivo = archivo.ResultadoArchivo;

                        if (reglas?.Count > 0 && reglasArchivo?.Count > 0 && resultadoArchivo?.Count > 0)
                        {
                            List<ArchivoProcesadoResponse> archivoProcesadoResultado = new();
                            ReglasProcesadoResponse reglasProcesadoResponse = new();

                            reglasArchivo.ForEach(r =>
                            {
                                reglasProcesadoResponse.Reglas.Add(r.Regla);
                            });

                            resultadoArchivo.ForEach(a =>
                            {
                                archivoProcesadoResultado.Add(new ArchivoProcesadoResponse { Reglas = JsonConvert.DeserializeObject<List<string>>(a.Reglas), Resultado = JsonConvert.DeserializeObject<List<string>>(a.Resultado), TipoProcesamiento = a.TipoProcesamiento });
                            });

                            ArchivoDetalleResponse archivoDetalleResponse = new()
                            {
                                FechaAlta = archivo.Alta,
                                IdInsercion = archivo.Id,
                                NombreArchivo = archivo.NombreArchivo,
                                Usuario = archivo.Usuario,
                                Tamanio = archivo.Tamanio,
                                Reglas = reglas,
                                ReglasArchivo = reglasProcesadoResponse,
                                ResultadoArchivo = archivoProcesadoResultado
                            };

                            archivoDetalleResponseList.Add(archivoDetalleResponse);
                        }
                    });

                    if (archivoDetalleResponseList?.Count > 0)
                        responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: archivoDetalleResponseList);
                    else
                        responseGenericApi = TratarRespuesta(caso: Casos.ERROR_DATA.ToString());
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

        public ResponseGenericApi TraerArchivoTipoFile(int id)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                var archivoDb = _context.Archivo.Where(x => x.Id == id).FirstOrDefault();
                MemoryStream memoryStream = new(archivoDb.Bytes);
                responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(), datosDevolucion: Tuple.Create(memoryStream, archivoDb.NombreArchivo));
            }
            catch (Exception ex)
            {
                responseGenericApi = TratarRespuesta(caso: Casos.EXCEPCION.ToString());
                _logger.LogError($"Error: Message: {ex.Message} StackTrace: {ex.StackTrace} InnerException: {ex.InnerException}");
            }
            return responseGenericApi;
        }
        #endregion

        #region Procesar Archivos por ID y File
        public ResponseGenericApi ProcesarArchivo(int id, Procesamiento opcion)
        {
            ResponseGenericApi responseGenericApi = new();
            try
            {
                var archivoDb = _context.Archivo.Where(x => x.Id == id).FirstOrDefault();
                if (archivoDb != null)
                {
                    var rules = JsonConvert.DeserializeObject<List<Rule>>(archivoDb.ContenidoArchivo);
                    responseGenericApi = ProcesarLogica(opcion, rules);
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
        public ResponseGenericApi ProcesarArchivo(IFormFile file, Procesamiento opcion)
        {
            ResponseGenericApi responseGenericApi;
            try
            {
                if (file == null || file.Length == 0)
                {
                    return responseGenericApi = TratarRespuesta(caso: Casos.ERROR_DATA.ToString());
                }

                var rules = FileHelper.GetRuleList(file);
                responseGenericApi = ProcesarLogica(opcion, rules);
            }
            catch (Exception ex)
            {
                responseGenericApi = TratarRespuesta(caso: Casos.EXCEPCION.ToString());
                _logger.LogError($"Error: Message: {ex.Message} StackTrace: {ex.StackTrace} InnerException: {ex.InnerException}");
            }

            return responseGenericApi;
        }
        private ResponseGenericApi ProcesarLogica(Procesamiento opcion, List<Rule> rules)
        {
            ResponseGenericApi responseGenericApi;

            if (rules?.Count > 0)
            {
                var equalAntecedentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, true);
                var equalConsequentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, false);
                var reglas = FunctionHelper.PrintRuleList(rules);
                var resultado = new List<string>();
                switch (opcion)
                {
                    case Procesamiento.ReglasRedundates:
                        resultado = FunctionHelper.FindReglasRedundates(equalAntecedentRules);
                        break;
                    case Procesamiento.ReglasConflictivas:
                        resultado = FunctionHelper.FindReglasConflictivas(equalAntecedentRules);
                        break;
                    case Procesamiento.ReglasIncluidasEnOtras:
                        resultado = FunctionHelper.FindReglasIncluidasEnOtras(equalConsequentRules);
                        break;
                    case Procesamiento.CondicionesSiInnecesarias:
                        resultado = FunctionHelper.FindCondicionesSiInnecesarias(equalConsequentRules);
                        break;
                    case Procesamiento.Todas:
                        resultado.AddRange(FunctionHelper.FindReglasRedundates(equalAntecedentRules));
                        resultado.AddRange(FunctionHelper.FindReglasConflictivas(equalAntecedentRules));
                        resultado.AddRange(FunctionHelper.FindReglasIncluidasEnOtras(equalConsequentRules));
                        resultado.AddRange(FunctionHelper.FindCondicionesSiInnecesarias(equalConsequentRules));
                        break;
                }
                responseGenericApi = TratarRespuesta(caso: Casos.EXITOSO_CON_DEVOLUCION_DATOS.ToString(),
                    datosDevolucion: new ArchivoProcesadoResponse
                    {
                        Reglas = reglas,
                        Resultado = resultado,
                        TipoProcesamiento = opcion.ToString()
                    });
            }
            else
            {
                responseGenericApi = TratarRespuesta(caso: Casos.ERROR_DATA.ToString());
            }

            return responseGenericApi;
        }
        #endregion

        #region OTROS
        static string MD5Hash(byte[] bytes)
        {
            using var md5 = MD5.Create();
            var result = md5.ComputeHash(bytes);
            return Encoding.ASCII.GetString(result);
        }
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

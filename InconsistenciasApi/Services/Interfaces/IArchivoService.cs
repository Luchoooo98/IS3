using InconsistenciasApi.Models.Enums.Errors;
using InconsistenciasApi.Models.Inconsistencias.Responses;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace InconsistenciasApi.Services.Interfaces
{
    public interface IArchivoService
    {
        Task<ResponseGenericApi> InsertarArchivo(IFormFile file, string usuario);
        ResponseGenericApi TraerArchivo(int id);
        ResponseGenericApi TraerArchivos(string usuario);
        ResponseGenericApi TraerArchivoTipoFile(int id);
        ResponseGenericApi ProcesarArchivo(int id, Procesamiento opcion);
        ResponseGenericApi ProcesarArchivo(IFormFile file, Procesamiento opcion);
    }
}

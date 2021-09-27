using InconsistenciasApi.Models.Inconsistencias.Responses;
using System.Security.Claims;

namespace InconsistenciasApi.Services.Interfaces
{
    public interface ITokenService
    {
        TokenResponse GenerarTokenJWT(ClaimsIdentity claimsUsuario);
        ClaimsIdentity CrearClaimsUsuario(string usuario, string email);
    }
}
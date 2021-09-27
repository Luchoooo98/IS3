using InconsistenciasApi.Models.Inconsistencias.Classes;
using InconsistenciasApi.Models.Inconsistencias.Requests;
using InconsistenciasApi.Models.Inconsistencias.Responses;

namespace InconsistenciasApi.Services.Interfaces
{
    public interface ILoginService
    {
        ResponseGenericApi AutenticarUsuario(LoginRequest loginRequest);
        ResponseGenericApi GenerarToken(UsuarioInfo usuarioInfo);
        ResponseGenericApi AutenticarToken(RefreshAccessTokenRequest refreshAccessTokenRequest);
    }
}
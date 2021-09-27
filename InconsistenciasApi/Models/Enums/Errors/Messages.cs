namespace InconsistenciasApi.Models.Enums.Errors
{
    public enum Messages
    {
        APP001 = 1,
        EAPP001,
        EAPP002,
        EAPP003,
        EAPP004
    }
    public enum Casos
    {
        EXITOSO = 1,
        EXITOSO_CODIGO,
        EXITOSO_CON_DEVOLUCION_DATOS,
        ERROR_CODE_MESSAGE,
        ERROR_NO_DEVUELVE_NADA_DB,
        EXCEPCION,
        ERROR_DATA
    }

    public enum Procesamiento
    {
        ReglasRedundates = 1,
        ReglasConflictivas = 2,
        ReglasIncluidasEnOtras = 3,
        CondicionesSiInnecesarias = 4,
        Todas = 5
    }
}

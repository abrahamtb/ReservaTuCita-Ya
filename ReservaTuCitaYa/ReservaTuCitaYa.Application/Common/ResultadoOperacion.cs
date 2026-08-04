namespace ReservaTuCitaYa.Application.Common
{
    public enum TipoErrorOperacion
    {
        Ninguno = 0,
        Validacion = 1,
        NoEncontrado = 2,
        Conflicto = 3
    }

    public sealed record ResultadoOperacion(
        bool EsExitoso,
        string? Error,
        TipoErrorOperacion TipoError)
    {
        public static ResultadoOperacion Exito() =>
            new(true, null, TipoErrorOperacion.Ninguno);

        public static ResultadoOperacion Fallo(
            string error,
            TipoErrorOperacion tipoError = TipoErrorOperacion.Validacion) =>
            new(false, error, tipoError);
    }

    public sealed record ResultadoOperacion<T>(
        bool EsExitoso,
        T? Valor,
        string? Error,
        TipoErrorOperacion TipoError)
    {
        public static ResultadoOperacion<T> Exito(T valor) =>
            new(true, valor, null, TipoErrorOperacion.Ninguno);

        public static ResultadoOperacion<T> Fallo(
            string error,
            TipoErrorOperacion tipoError = TipoErrorOperacion.Validacion) =>
            new(false, default, error, tipoError);
    }

    public sealed class ConflictoPersistenciaException : Exception
    {
        public ConflictoPersistenciaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

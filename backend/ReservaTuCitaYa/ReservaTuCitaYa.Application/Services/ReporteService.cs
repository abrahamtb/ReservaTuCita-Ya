using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Reportes;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Enums;
using System.Globalization;
using System.Text;

namespace ReservaTuCitaYa.Application.Services;

public sealed class ReporteService(
    IReporteRepository reporteRepository)
    : IReporteService
{
    public const string OrganizacionInvalida =
        "No se pudo determinar la organización del usuario.";

    public const string RangoFechasInvalido =
        "La fecha hasta no puede ser menor que la fecha desde.";

    public const string RangoDemasiadoAmplio =
        "El rango máximo permitido para reportes es de 366 días.";

    public const string PaginaInvalida =
        "La página debe ser mayor o igual a 1.";

    public const string TamanoPaginaInvalido =
        "El tamaño de página debe estar entre 1 y 100.";

    public async Task<ResultadoOperacion<ReporteReservasRespuestaDto>>
        ObtenerReservasAsync(
            Guid organizacionId,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            Guid? sedeId,
            Guid? profesionalId,
            Guid? servicioId,
            EstadoReserva? estado,
            Guid? clienteId,
            int pagina,
            int tamanoPagina,
            CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            pagina,
            tamanoPagina);

        if (validacion is not null)
        {
            return ResultadoOperacion<ReporteReservasRespuestaDto>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteReservasFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            clienteId,
            pagina,
            tamanoPagina);

        var resultado =
            await reporteRepository.ObtenerReservasAsync(
                filtro,
                ct);

        return ResultadoOperacion<ReporteReservasRespuestaDto>
            .Exito(resultado);
    }

    public async Task<ResultadoOperacion<ReporteIngresosRespuestaDto>>
        ObtenerIngresosAsync(
            Guid organizacionId,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            Guid? sedeId,
            Guid? metodoPagoId,
            int pagina,
            int tamanoPagina,
            CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            pagina,
            tamanoPagina);

        if (validacion is not null)
        {
            return ResultadoOperacion<ReporteIngresosRespuestaDto>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteIngresosFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            metodoPagoId,
            pagina,
            tamanoPagina);

        var resultado =
            await reporteRepository.ObtenerIngresosAsync(
                filtro,
                ct);

        return ResultadoOperacion<ReporteIngresosRespuestaDto>
            .Exito(resultado);
    }

    public async Task<ResultadoOperacion<ReporteAtencionesRespuestaDto>>
        ObtenerAtencionesAsync(
            Guid organizacionId,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            Guid? sedeId,
            Guid? profesionalId,
            Guid? servicioId,
            EstadoReserva? estado,
            ResultadoAtencion? resultado,
            int pagina,
            int tamanoPagina,
            CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            pagina,
            tamanoPagina);

        if (validacion is not null)
        {
            return ResultadoOperacion<ReporteAtencionesRespuestaDto>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteAtencionesFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            resultado,
            pagina,
            tamanoPagina);

        var reporte =
            await reporteRepository.ObtenerAtencionesAsync(
                filtro,
                ct);

        return ResultadoOperacion<ReporteAtencionesRespuestaDto>
            .Exito(reporte);
    }

    public async Task<ResultadoOperacion<byte[]>> ExportarReservasCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        Guid? clienteId,
        CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            1,
            100);

        if (validacion is not null)
        {
            return ResultadoOperacion<byte[]>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteReservasFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            clienteId,
            1,
            100);

        var filas =
            await reporteRepository.ExportarReservasAsync(
                filtro,
                ct);

        var csv = new StringBuilder();

        csv.AppendLine(
            "Codigo;Fecha;Hora;Cliente;Servicio;Sede;Profesional;Estado;CantidadParticipantes;PrecioTotal");

        foreach (var fila in filas)
        {
            csv.AppendLine(string.Join(";",
                EscaparCsv(fila.Codigo),
                fila.Fecha.ToString("yyyy-MM-dd"),
                fila.Hora.ToString("HH:mm:ss"),
                EscaparCsv(fila.Cliente),
                EscaparCsv(fila.Servicio),
                EscaparCsv(fila.Sede),
                EscaparCsv(fila.Profesional),
                EscaparCsv(fila.Estado),
                fila.CantidadParticipantes.ToString(
                    CultureInfo.InvariantCulture),
                fila.PrecioTotal.ToString(
                    CultureInfo.InvariantCulture)));
        }

        return ResultadoOperacion<byte[]>
            .Exito(
                Encoding.UTF8.GetBytes(
                    csv.ToString()));
    }

    public async Task<ResultadoOperacion<byte[]>> ExportarIngresosCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? metodoPagoId,
        CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            1,
            100);

        if (validacion is not null)
        {
            return ResultadoOperacion<byte[]>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteIngresosFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            metodoPagoId,
            1,
            100);

        var filas =
            await reporteRepository.ExportarIngresosAsync(
                filtro,
                ct);

        var csv = new StringBuilder();

        csv.AppendLine(
            "Fecha;CodigoMovimiento;CodigoReserva;Cliente;Sede;Tipo;Metodo;NumeroOperacion;Monto");

        foreach (var fila in filas)
        {
            csv.AppendLine(string.Join(";",
                fila.Fecha.ToString("yyyy-MM-dd"),
                EscaparCsv(fila.CodigoMovimiento),
                EscaparCsv(fila.CodigoReserva),
                EscaparCsv(fila.Cliente),
                EscaparCsv(fila.Sede),
                EscaparCsv(fila.Tipo),
                EscaparCsv(fila.Metodo),
                EscaparCsv(fila.NumeroOperacion),
                fila.Monto.ToString(
                    CultureInfo.InvariantCulture)));
        }

        return ResultadoOperacion<byte[]>
            .Exito(
                Encoding.UTF8.GetBytes(
                    csv.ToString()));
    }

    public async Task<ResultadoOperacion<byte[]>> ExportarAtencionesCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        ResultadoAtencion? resultado,
        CancellationToken ct = default)
    {
        var validacion = Validar(
            organizacionId,
            fechaDesde,
            fechaHasta,
            1,
            100);

        if (validacion is not null)
        {
            return ResultadoOperacion<byte[]>
                .Fallo(
                    validacion,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new ReporteAtencionesFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            resultado,
            1,
            100);

        var filas =
            await reporteRepository.ExportarAtencionesAsync(
                filtro,
                ct);

        var csv = new StringBuilder();

        csv.AppendLine(
            "CodigoReserva;Fecha;HoraProgramada;Cliente;Servicio;Profesional;HoraLlegada;HoraInicioReal;HoraFinReal;DuracionRealMinutos;Resultado;Estado");

        foreach (var fila in filas)
        {
            csv.AppendLine(string.Join(";",
                EscaparCsv(fila.CodigoReserva),
                fila.Fecha.ToString("yyyy-MM-dd"),
                fila.HoraProgramada.ToString("HH:mm:ss"),
                EscaparCsv(fila.Cliente),
                EscaparCsv(fila.Servicio),
                EscaparCsv(fila.Profesional),
                fila.HoraLlegada?.ToString("O") ?? "",
                fila.HoraInicioReal?.ToString("O") ?? "",
                fila.HoraFinReal?.ToString("O") ?? "",
                fila.DuracionRealMinutos?.ToString(
                    CultureInfo.InvariantCulture) ?? "",
                EscaparCsv(fila.Resultado),
                EscaparCsv(fila.Estado)));
        }

        return ResultadoOperacion<byte[]>
            .Exito(
                Encoding.UTF8.GetBytes(
                    csv.ToString()));
    }

    private static string? Validar(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        int pagina,
        int tamanoPagina)
    {
        if (organizacionId == Guid.Empty)
            return OrganizacionInvalida;

        if (fechaHasta < fechaDesde)
            return RangoFechasInvalido;

        var dias =
            fechaHasta.DayNumber -
            fechaDesde.DayNumber + 1;

        if (dias > 366)
            return RangoDemasiadoAmplio;

        if (pagina < 1)
            return PaginaInvalida;

        if (tamanoPagina is < 1 or > 100)
            return TamanoPaginaInvalido;

        return null;
    }

    private static string EscaparCsv(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        var necesitaComillas =
            valor.Contains(';') ||
            valor.Contains('"') ||
            valor.Contains('\n') ||
            valor.Contains('\r');

        if (!necesitaComillas)
            return valor;

        return $"\"{valor.Replace("\"", "\"\"")}\"";
    }
}
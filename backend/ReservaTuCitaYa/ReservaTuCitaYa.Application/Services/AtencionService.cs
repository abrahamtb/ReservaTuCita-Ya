using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Atenciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services;

public sealed class AtencionService(
    IAtencionRepository atencionRepository) : IAtencionService
{
    public const string ReservaInvalida =
        "La reserva no existe o no pertenece a la organización.";

    public const string EstadoNoPermitido =
        "El estado actual de la reserva no permite marcar al cliente como presente.";

    public const string ReservaFutura =
        "No se puede marcar presente una reserva futura.";

    public const string PresenciaYaRegistrada =
        "La presencia del cliente ya fue registrada.";

    public const string EstadoNoPermitidoIniciar =
        "El estado actual de la reserva no permite iniciar la atención.";

    public const string AtencionNoEncontrada =
        "No se encontró la atención asociada a la reserva.";

    public const string AtencionYaIniciada =
        "La atención ya fue iniciada.";

    public const string EstadoNoPermitidoFinalizar =
    "El estado actual de la reserva no permite finalizar la atención.";

    public const string AtencionNoIniciada =
        "La atención todavía no ha sido iniciada.";

    public const string ResultadoInvalido =
        "El resultado de atención no es válido.";

    public const string EstadoNoPermitidoNoAsistio =
    "El estado actual de la reserva no permite marcarla como no asistida.";

    public const string ReservaFuturaNoAsistio =
        "No se puede marcar como no asistida una reserva futura.";

    public async Task<ResultadoOperacion<MarcarPresenteRespuesta>>
        MarcarPresenteAsync(
            Guid organizacionId,
            Guid reservaId,
            string? usuarioId,
            CancellationToken ct = default)
    {
        return await atencionRepository.EjecutarEnTransaccionAsync(
            async innerCt =>
            {
                var reserva =
                    await atencionRepository.ObtenerReservaParaModificarAsync(
                        reservaId,
                        innerCt);

                if (reserva is null ||
                    reserva.EstaEliminado ||
                    reserva.OrganizacionId != organizacionId)
                {
                    return ResultadoOperacion<MarcarPresenteRespuesta>
                        .Fallo(
                            ReservaInvalida,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (reserva.EstadoReserva is not
                    (EstadoReserva.Confirmada or EstadoReserva.Reprogramada))
                {
                    return ResultadoOperacion<MarcarPresenteRespuesta>
                        .Fallo(
                            EstadoNoPermitido,
                            TipoErrorOperacion.Conflicto);
                }

                var hoy = DateOnly.FromDateTime(DateTime.Now);

                if (reserva.Fecha > hoy)
                {
                    return ResultadoOperacion<MarcarPresenteRespuesta>
                        .Fallo(
                            ReservaFutura,
                            TipoErrorOperacion.Conflicto);
                }

                var atencion =
                    await atencionRepository.ObtenerPorReservaIdAsync(
                        reserva.Id,
                        innerCt);

                if (atencion?.FechaHoraPresencia is not null)
                {
                    return ResultadoOperacion<MarcarPresenteRespuesta>
                        .Fallo(
                            PresenciaYaRegistrada,
                            TipoErrorOperacion.Conflicto);
                }

                var fechaHoraPresencia = DateTime.UtcNow;

                if (atencion is null)
                {
                    atencion = new Atencion
                    {
                        ReservaId = reserva.Id,
                        FechaHoraPresencia = fechaHoraPresencia
                    };

                    atencionRepository.Agregar(atencion);
                }
                else
                {
                    atencion.FechaHoraPresencia = fechaHoraPresencia;
                    atencion.FechaModificacion = fechaHoraPresencia;
                }

                var estadoAnterior = reserva.EstadoReserva;

                reserva.EstadoReserva = EstadoReserva.Presente;
                reserva.FechaModificacion = fechaHoraPresencia;

                atencionRepository.AgregarHistorial(
                    new HistorialReserva
                    {
                        ReservaId = reserva.Id,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = EstadoReserva.Presente,
                        TipoAccion = TipoAccionReserva.MarcadaPresente,
                        FechaAccion = fechaHoraPresencia,
                        UsuarioId = usuarioId
                    });

                await atencionRepository.GuardarAsync(innerCt);

                return ResultadoOperacion<MarcarPresenteRespuesta>
                    .Exito(
                        new MarcarPresenteRespuesta(
                            reserva.Id,
                            atencion.Id,
                            reserva.Codigo,
                            reserva.EstadoReserva.ToString(),
                            fechaHoraPresencia));
            },
            ct);
    }

    public async Task<ResultadoOperacion<IniciarAtencionRespuesta>>
        IniciarAtencionAsync(
            Guid organizacionId,
            Guid reservaId,
            string? usuarioId,
            CancellationToken ct = default)
    {
        return await atencionRepository.EjecutarEnTransaccionAsync(
            async innerCt =>
            {
                var reserva =
                    await atencionRepository.ObtenerReservaParaModificarAsync(
                        reservaId,
                        innerCt);

                if (reserva is null ||
                    reserva.EstaEliminado ||
                    reserva.OrganizacionId != organizacionId)
                {
                    return ResultadoOperacion<IniciarAtencionRespuesta>
                        .Fallo(
                            ReservaInvalida,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (reserva.EstadoReserva != EstadoReserva.Presente)
                {
                    return ResultadoOperacion<IniciarAtencionRespuesta>
                        .Fallo(
                            EstadoNoPermitidoIniciar,
                            TipoErrorOperacion.Conflicto);
                }

                var atencion =
                    await atencionRepository.ObtenerPorReservaIdAsync(
                        reserva.Id,
                        innerCt);

                if (atencion is null)
                {
                    return ResultadoOperacion<IniciarAtencionRespuesta>
                        .Fallo(
                            AtencionNoEncontrada,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (atencion.FechaHoraInicioReal is not null)
                {
                    return ResultadoOperacion<IniciarAtencionRespuesta>
                        .Fallo(
                            AtencionYaIniciada,
                            TipoErrorOperacion.Conflicto);
                }

                var fechaHoraInicio = DateTime.UtcNow;
                var estadoAnterior = reserva.EstadoReserva;

                atencion.FechaHoraInicioReal = fechaHoraInicio;
                atencion.FechaModificacion = fechaHoraInicio;

                reserva.EstadoReserva = EstadoReserva.EnAtencion;
                reserva.FechaModificacion = fechaHoraInicio;

                atencionRepository.AgregarHistorial(
                    new HistorialReserva
                    {
                        ReservaId = reserva.Id,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = EstadoReserva.EnAtencion,
                        TipoAccion = TipoAccionReserva.AtencionIniciada,
                        FechaAccion = fechaHoraInicio,
                        UsuarioId = usuarioId
                    });

                await atencionRepository.GuardarAsync(innerCt);

                return ResultadoOperacion<IniciarAtencionRespuesta>
                    .Exito(
                        new IniciarAtencionRespuesta(
                            reserva.Id,
                            atencion.Id,
                            reserva.Codigo,
                            reserva.EstadoReserva.ToString(),
                            fechaHoraInicio));
            },
            ct);
    }
    public async Task<ResultadoOperacion<FinalizarAtencionRespuesta>>
    FinalizarAtencionAsync(
        Guid organizacionId,
        Guid reservaId,
        FinalizarAtencionSolicitud solicitud,
        string? usuarioId,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(solicitud.Resultado) ||
            solicitud.Resultado == ResultadoAtencion.NoDefinido)
        {
            return ResultadoOperacion<FinalizarAtencionRespuesta>
                .Fallo(
                    ResultadoInvalido,
                    TipoErrorOperacion.Validacion);
        }

        if (solicitud.Observaciones?.Trim().Length > 1000)
        {
            return ResultadoOperacion<FinalizarAtencionRespuesta>
                .Fallo(
                    "Las observaciones no pueden superar 1000 caracteres.",
                    TipoErrorOperacion.Validacion);
        }

        if (solicitud.Recomendaciones?.Trim().Length > 1000)
        {
            return ResultadoOperacion<FinalizarAtencionRespuesta>
                .Fallo(
                    "Las recomendaciones no pueden superar 1000 caracteres.",
                    TipoErrorOperacion.Validacion);
        }

        return await atencionRepository.EjecutarEnTransaccionAsync(
            async innerCt =>
            {
                var reserva =
                    await atencionRepository.ObtenerReservaParaModificarAsync(
                        reservaId,
                        innerCt);

                if (reserva is null ||
                    reserva.EstaEliminado ||
                    reserva.OrganizacionId != organizacionId)
                {
                    return ResultadoOperacion<FinalizarAtencionRespuesta>
                        .Fallo(
                            ReservaInvalida,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (reserva.EstadoReserva != EstadoReserva.EnAtencion)
                {
                    return ResultadoOperacion<FinalizarAtencionRespuesta>
                        .Fallo(
                            EstadoNoPermitidoFinalizar,
                            TipoErrorOperacion.Conflicto);
                }

                var atencion =
                    await atencionRepository.ObtenerPorReservaIdAsync(
                        reserva.Id,
                        innerCt);

                if (atencion is null)
                {
                    return ResultadoOperacion<FinalizarAtencionRespuesta>
                        .Fallo(
                            AtencionNoEncontrada,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (atencion.FechaHoraInicioReal is null)
                {
                    return ResultadoOperacion<FinalizarAtencionRespuesta>
                        .Fallo(
                            AtencionNoIniciada,
                            TipoErrorOperacion.Conflicto);
                }

                if (atencion.FechaHoraFinReal is not null)
                {
                    return ResultadoOperacion<FinalizarAtencionRespuesta>
                        .Fallo(
                            "La atención ya fue finalizada.",
                            TipoErrorOperacion.Conflicto);
                }

                var fechaHoraFin = DateTime.UtcNow;
                var estadoAnterior = reserva.EstadoReserva;

                atencion.FechaHoraFinReal = fechaHoraFin;
                atencion.ResultadoAtencion = solicitud.Resultado;

                atencion.Observaciones =
                    string.IsNullOrWhiteSpace(solicitud.Observaciones)
                        ? null
                        : solicitud.Observaciones.Trim();

                atencion.Recomendaciones =
                    string.IsNullOrWhiteSpace(solicitud.Recomendaciones)
                        ? null
                        : solicitud.Recomendaciones.Trim();

                atencion.ProximoServicioId =
                    solicitud.ProximoServicioId;

                atencion.ProximaFechaSugerida =
                    solicitud.ProximaFechaSugerida;

                atencion.FechaModificacion = fechaHoraFin;

                reserva.EstadoReserva = EstadoReserva.Atendida;
                reserva.FechaModificacion = fechaHoraFin;

                atencionRepository.AgregarHistorial(
                    new HistorialReserva
                    {
                        ReservaId = reserva.Id,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = EstadoReserva.Atendida,
                        TipoAccion =
                            TipoAccionReserva.AtencionFinalizada,
                        FechaAccion = fechaHoraFin,
                        UsuarioId = usuarioId
                    });

                await atencionRepository.GuardarAsync(innerCt);

                return ResultadoOperacion<FinalizarAtencionRespuesta>
                    .Exito(
                        new FinalizarAtencionRespuesta(
                            reserva.Id,
                            atencion.Id,
                            reserva.Codigo,
                            reserva.EstadoReserva.ToString(),
                            solicitud.Resultado.ToString(),
                            fechaHoraFin));
            },
            ct);
    }
    public async Task<ResultadoOperacion<MarcarNoAsistioRespuesta>>
    MarcarNoAsistioAsync(
        Guid organizacionId,
        Guid reservaId,
        string? usuarioId,
        CancellationToken ct = default)
    {
        return await atencionRepository.EjecutarEnTransaccionAsync(
            async innerCt =>
            {
                var reserva =
                    await atencionRepository.ObtenerReservaParaModificarAsync(
                        reservaId,
                        innerCt);

                if (reserva is null ||
                    reserva.EstaEliminado ||
                    reserva.OrganizacionId != organizacionId)
                {
                    return ResultadoOperacion<MarcarNoAsistioRespuesta>
                        .Fallo(
                            ReservaInvalida,
                            TipoErrorOperacion.NoEncontrado);
                }

                if (reserva.EstadoReserva is not
                    (EstadoReserva.Confirmada or EstadoReserva.Reprogramada))
                {
                    return ResultadoOperacion<MarcarNoAsistioRespuesta>
                        .Fallo(
                            EstadoNoPermitidoNoAsistio,
                            TipoErrorOperacion.Conflicto);
                }

                var hoy = DateOnly.FromDateTime(DateTime.Now);

                if (reserva.Fecha > hoy)
                {
                    return ResultadoOperacion<MarcarNoAsistioRespuesta>
                        .Fallo(
                            ReservaFuturaNoAsistio,
                            TipoErrorOperacion.Conflicto);
                }

                // No debe existir atención iniciada/presencia registrada.
                var atencion =
                    await atencionRepository.ObtenerPorReservaIdAsync(
                        reserva.Id,
                        innerCt);

                if (atencion is not null)
                {
                    return ResultadoOperacion<MarcarNoAsistioRespuesta>
                        .Fallo(
                            "La reserva ya tiene una atención asociada.",
                            TipoErrorOperacion.Conflicto);
                }

                var fechaRegistro = DateTime.UtcNow;
                var estadoAnterior = reserva.EstadoReserva;

                reserva.EstadoReserva = EstadoReserva.NoAsistio;
                reserva.FechaModificacion = fechaRegistro;

                atencionRepository.AgregarHistorial(
                    new HistorialReserva
                    {
                        ReservaId = reserva.Id,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = EstadoReserva.NoAsistio,
                        TipoAccion = TipoAccionReserva.NoAsistio,
                        FechaAccion = fechaRegistro,
                        UsuarioId = usuarioId
                    });

                await atencionRepository.GuardarAsync(innerCt);

                return ResultadoOperacion<MarcarNoAsistioRespuesta>
                    .Exito(
                        new MarcarNoAsistioRespuesta(
                            reserva.Id,
                            reserva.Codigo,
                            reserva.EstadoReserva.ToString(),
                            fechaRegistro));
            },
            ct);
    }
    public async Task<ResultadoOperacion<AtencionDetalleDto>>
    ObtenerDetalleAsync(
        Guid organizacionId,
        Guid reservaId,
        CancellationToken ct = default)
    {
        var detalle =
            await atencionRepository.ObtenerDetalleAsync(
                reservaId,
                ct);

        if (detalle is null ||
            detalle.OrganizacionId != organizacionId)
        {
            return ResultadoOperacion<AtencionDetalleDto>
                .Fallo(
                    AtencionNoEncontrada,
                    TipoErrorOperacion.NoEncontrado);
        }

        return ResultadoOperacion<AtencionDetalleDto>
            .Exito(detalle);
    }
    public async Task<ResultadoOperacion<AgendaProfesionalDto>>
    ObtenerAgendaProfesionalAsync(
        Guid organizacionId,
        Guid profesionalId,
        DateOnly fecha,
        CancellationToken ct = default)
    {
        var agenda =
            await atencionRepository.ObtenerAgendaProfesionalAsync(
                organizacionId,
                profesionalId,
                fecha,
                ct);

        if (agenda is null)
        {
            return ResultadoOperacion<AgendaProfesionalDto>
                .Fallo(
                    "El profesional no existe o no pertenece a la organización.",
                    TipoErrorOperacion.NoEncontrado);
        }

        return ResultadoOperacion<AgendaProfesionalDto>
            .Exito(agenda);
    }
}
using System.Net.Mail;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Services
{
    public class SedeService : ISedeService
    {
        private readonly ISedeRepository _sedeRepository;
        private readonly IOrganizacionRepository _organizacionRepository;

        public SedeService(
            ISedeRepository sedeRepository,
            IOrganizacionRepository organizacionRepository)
        {
            _sedeRepository = sedeRepository;
            _organizacionRepository = organizacionRepository;
        }

        public async Task<ResultadoOperacion<IReadOnlyList<SedeListaDto>>> ListarPorOrganizacionAsync(
            SedeFiltroDto filtro,
            CancellationToken cancellationToken = default)
        {
            var organizacion = await _organizacionRepository.ObtenerParaModificarAsync(
                filtro.OrganizacionId,
                cancellationToken);

            if (organizacion is null || organizacion.EstaEliminado)
            {
                return ResultadoOperacion<IReadOnlyList<SedeListaDto>>.Fallo(
                    "La organización no existe o fue eliminada.",
                    TipoErrorOperacion.NoEncontrado);
            }

            var sedes = await _sedeRepository.ListarAsync(filtro, cancellationToken);
            return ResultadoOperacion<IReadOnlyList<SedeListaDto>>.Exito(sedes);
        }

        public async Task<ResultadoOperacion<SedeDetalleDto>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detalle = await _sedeRepository.ObtenerDetalleAsync(id, cancellationToken);

            return detalle is null
                ? ResultadoOperacion<SedeDetalleDto>.Fallo(
                    "La sede no existe o fue eliminada.",
                    TipoErrorOperacion.NoEncontrado)
                : ResultadoOperacion<SedeDetalleDto>.Exito(detalle);
        }

        public async Task<ResultadoOperacion<Guid>> CrearAsync(
            CrearSedeSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            var error = Validar(
                solicitud.Nombre,
                solicitud.Direccion,
                solicitud.Telefono,
                solicitud.Correo,
                solicitud.Referencia);

            if (error is not null)
                return ResultadoOperacion<Guid>.Fallo(error);

            var organizacion = await _organizacionRepository.ObtenerParaModificarAsync(
                solicitud.OrganizacionId,
                cancellationToken);

            var errorOrganizacion = ValidarOrganizacionParaNuevaOperacion(organizacion);
            if (errorOrganizacion is not null)
                return ResultadoOperacion<Guid>.Fallo(errorOrganizacion);

            var nombre = solicitud.Nombre.Trim();
            if (await _sedeRepository.ExisteNombreActivoAsync(
                    solicitud.OrganizacionId,
                    nombre,
                    cancellationToken: cancellationToken))
            {
                return ResultadoOperacion<Guid>.Fallo(
                    "Ya existe una sede activa con ese nombre en la organización.",
                    TipoErrorOperacion.Conflicto);
            }

            var sede = new Sede
            {
                OrganizacionId = solicitud.OrganizacionId,
                Nombre = nombre,
                Direccion = solicitud.Direccion.Trim(),
                Telefono = LimpiarOpcional(solicitud.Telefono),
                Correo = LimpiarOpcional(solicitud.Correo),
                Referencia = LimpiarOpcional(solicitud.Referencia)
            };

            _sedeRepository.Agregar(sede);

            try
            {
                await _sedeRepository.GuardarAsync(cancellationToken);
            }
            catch (ConflictoPersistenciaException)
            {
                return ResultadoOperacion<Guid>.Fallo(
                    "Ya existe una sede activa con ese nombre en la organización.",
                    TipoErrorOperacion.Conflicto);
            }

            return ResultadoOperacion<Guid>.Exito(sede.Id);
        }

        public async Task<ResultadoOperacion> ActualizarAsync(
            ActualizarSedeSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            var sede = await _sedeRepository.ObtenerParaModificarAsync(
                solicitud.Id,
                cancellationToken);

            var estado = ValidarRegistro(sede);
            if (estado is not null)
                return estado;

            var error = Validar(
                solicitud.Nombre,
                solicitud.Direccion,
                solicitud.Telefono,
                solicitud.Correo,
                solicitud.Referencia);

            if (error is not null)
                return ResultadoOperacion.Fallo(error);

            var nombre = solicitud.Nombre.Trim();
            if (sede!.EstaActivo && await _sedeRepository.ExisteNombreActivoAsync(
                    sede.OrganizacionId,
                    nombre,
                    sede.Id,
                    cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "Ya existe una sede activa con ese nombre en la organización.",
                    TipoErrorOperacion.Conflicto);
            }

            sede.Nombre = nombre;
            sede.Direccion = solicitud.Direccion.Trim();
            sede.Telefono = LimpiarOpcional(solicitud.Telefono);
            sede.Correo = LimpiarOpcional(solicitud.Correo);
            sede.Referencia = LimpiarOpcional(solicitud.Referencia);
            sede.FechaModificacion = DateTime.UtcNow;

            try
            {
                await _sedeRepository.GuardarAsync(cancellationToken);
            }
            catch (ConflictoPersistenciaException)
            {
                return ResultadoOperacion.Fallo(
                    "Ya existe una sede activa con ese nombre en la organización.",
                    TipoErrorOperacion.Conflicto);
            }

            return ResultadoOperacion.Exito();
        }

        public async Task<ResultadoOperacion> CambiarEstadoAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var sede = await _sedeRepository.ObtenerParaModificarAsync(id, cancellationToken);
            var estado = ValidarRegistro(sede);
            if (estado is not null)
                return estado;

            if (!sede!.EstaActivo)
            {
                var organizacion = await _organizacionRepository.ObtenerParaModificarAsync(
                    sede.OrganizacionId,
                    cancellationToken);

                var errorOrganizacion = ValidarOrganizacionParaNuevaOperacion(organizacion);
                if (errorOrganizacion is not null)
                    return ResultadoOperacion.Fallo(errorOrganizacion);

                if (await _sedeRepository.ExisteNombreActivoAsync(
                        sede.OrganizacionId,
                        sede.Nombre,
                        sede.Id,
                        cancellationToken))
                {
                    return ResultadoOperacion.Fallo(
                        "No se puede activar la sede porque ya existe otra sede activa con ese nombre.",
                        TipoErrorOperacion.Conflicto);
                }
            }

            sede.EstaActivo = !sede.EstaActivo;
            sede.FechaModificacion = DateTime.UtcNow;
            await _sedeRepository.GuardarAsync(cancellationToken);

            return ResultadoOperacion.Exito();
        }

        public async Task<ResultadoOperacion> EliminarAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var sede = await _sedeRepository.ObtenerParaModificarAsync(id, cancellationToken);
            var estado = ValidarRegistro(sede);
            if (estado is not null)
                return estado;

            sede!.EstaActivo = false;
            sede.EstaEliminado = true;
            sede.FechaModificacion = DateTime.UtcNow;
            await _sedeRepository.GuardarAsync(cancellationToken);

            return ResultadoOperacion.Exito();
        }

        private static ResultadoOperacion? ValidarRegistro(Sede? sede)
        {
            if (sede is null)
            {
                return ResultadoOperacion.Fallo(
                    "La sede no existe.",
                    TipoErrorOperacion.NoEncontrado);
            }

            return sede.EstaEliminado
                ? ResultadoOperacion.Fallo(
                    "La sede fue eliminada y no admite operaciones.",
                    TipoErrorOperacion.NoEncontrado)
                : null;
        }

        private static string? ValidarOrganizacionParaNuevaOperacion(Organizacion? organizacion)
        {
            if (organizacion is null || organizacion.EstaEliminado)
                return "La organización no existe o fue eliminada.";
            if (!organizacion.EstaActivo)
                return "La organización está inactiva y no admite nuevas sedes.";

            return null;
        }

        private static string? Validar(
            string nombre,
            string direccion,
            string? telefono,
            string? correo,
            string? referencia)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "El nombre de la sede es obligatorio.";
            if (nombre.Trim().Length > 150)
                return "El nombre no puede superar 150 caracteres.";
            if (string.IsNullOrWhiteSpace(direccion))
                return "La dirección es obligatoria.";
            if (direccion.Trim().Length > 250)
                return "La dirección no puede superar 250 caracteres.";
            if (telefono?.Trim().Length > 30)
                return "El teléfono no puede superar 30 caracteres.";
            if (correo?.Trim().Length > 256)
                return "El correo no puede superar 256 caracteres.";
            if (!string.IsNullOrWhiteSpace(correo) && !MailAddress.TryCreate(correo.Trim(), out _))
                return "El correo no tiene un formato válido.";
            if (referencia?.Trim().Length > 500)
                return "La referencia no puede superar 500 caracteres.";

            return null;
        }

        private static string? LimpiarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}

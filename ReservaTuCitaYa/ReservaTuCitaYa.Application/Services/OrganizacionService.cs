using System.Net.Mail;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Services
{
    public class OrganizacionService : IOrganizacionService
    {
        private readonly IOrganizacionRepository _repository;

        public OrganizacionService(IOrganizacionRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default) =>
            _repository.ListarAsync(filtro, cancellationToken);

        public Task<PaginaResultado<OrganizacionListaDto>> ListarPaginadoAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default) =>
            _repository.ListarPaginadoAsync(filtro, cancellationToken);

        public async Task<ResultadoOperacion<OrganizacionDetalleDto>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detalle = await _repository.ObtenerDetalleAsync(id, cancellationToken);

            return detalle is null
                ? ResultadoOperacion<OrganizacionDetalleDto>.Fallo(
                    "La organización no existe o fue eliminada.",
                    TipoErrorOperacion.NoEncontrado)
                : ResultadoOperacion<OrganizacionDetalleDto>.Exito(detalle);
        }

        public Task<IReadOnlyList<TipoOrganizacionOpcionDto>> ListarTiposActivosAsync(
            CancellationToken cancellationToken = default) =>
            _repository.ListarTiposActivosAsync(cancellationToken);

        public async Task<ResultadoOperacion<Guid>> CrearAsync(
            CrearOrganizacionSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            var error = Validar(
                solicitud.TipoOrganizacionId,
                solicitud.NombreComercial,
                solicitud.RazonSocial,
                solicitud.NumeroDocumento,
                solicitud.Telefono,
                solicitud.Correo,
                solicitud.DireccionPrincipal,
                solicitud.LogoUrl);

            if (error is not null)
            {
                return ResultadoOperacion<Guid>.Fallo(error);
            }

            if (!await _repository.TipoValidoAsync(
                    solicitud.TipoOrganizacionId,
                    cancellationToken))
            {
                return ResultadoOperacion<Guid>.Fallo(
                    "El tipo de organización seleccionado no existe o está inactivo.");
            }

            var numeroDocumento = solicitud.NumeroDocumento.Trim();

            if (await _repository.ExisteDocumentoAsync(
                    numeroDocumento,
                    cancellationToken: cancellationToken))
            {
                return ResultadoOperacion<Guid>.Fallo(
                    "El número de documento ya está registrado.",
                    TipoErrorOperacion.Conflicto);
            }

            var organizacion = new Organizacion
            {
                TipoOrganizacionId = solicitud.TipoOrganizacionId,
                NombreComercial = solicitud.NombreComercial.Trim(),
                RazonSocial = LimpiarOpcional(solicitud.RazonSocial),
                NumeroDocumento = numeroDocumento,
                Telefono = LimpiarOpcional(solicitud.Telefono),
                Correo = LimpiarOpcional(solicitud.Correo),
                DireccionPrincipal = LimpiarOpcional(solicitud.DireccionPrincipal),
                LogoUrl = LimpiarOpcional(solicitud.LogoUrl)
            };

            _repository.Agregar(organizacion);

            try
            {
                await _repository.GuardarAsync(cancellationToken);
            }
            catch (ConflictoPersistenciaException)
            {
                return ResultadoOperacion<Guid>.Fallo(
                    "El número de documento ya está registrado.",
                    TipoErrorOperacion.Conflicto);
            }

            return ResultadoOperacion<Guid>.Exito(organizacion.Id);
        }

        public async Task<ResultadoOperacion> ActualizarAsync(
            ActualizarOrganizacionSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            var organizacion = await _repository.ObtenerParaModificarAsync(
                solicitud.Id,
                cancellationToken);

            var estado = ValidarRegistro(organizacion);
            if (estado is not null)
            {
                return estado;
            }

            var error = Validar(
                solicitud.TipoOrganizacionId,
                solicitud.NombreComercial,
                solicitud.RazonSocial,
                solicitud.NumeroDocumento,
                solicitud.Telefono,
                solicitud.Correo,
                solicitud.DireccionPrincipal,
                solicitud.LogoUrl);

            if (error is not null)
            {
                return ResultadoOperacion.Fallo(error);
            }

            if (!await _repository.TipoValidoAsync(
                    solicitud.TipoOrganizacionId,
                    cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "El tipo de organización seleccionado no existe o está inactivo.");
            }

            var numeroDocumento = solicitud.NumeroDocumento.Trim();

            if (await _repository.ExisteDocumentoAsync(
                    numeroDocumento,
                    solicitud.Id,
                    cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "El número de documento ya está registrado.",
                    TipoErrorOperacion.Conflicto);
            }

            organizacion!.TipoOrganizacionId = solicitud.TipoOrganizacionId;
            organizacion.NombreComercial = solicitud.NombreComercial.Trim();
            organizacion.RazonSocial = LimpiarOpcional(solicitud.RazonSocial);
            organizacion.NumeroDocumento = numeroDocumento;
            organizacion.Telefono = LimpiarOpcional(solicitud.Telefono);
            organizacion.Correo = LimpiarOpcional(solicitud.Correo);
            organizacion.DireccionPrincipal = LimpiarOpcional(solicitud.DireccionPrincipal);
            organizacion.LogoUrl = LimpiarOpcional(solicitud.LogoUrl);
            organizacion.FechaModificacion = DateTime.UtcNow;

            try
            {
                await _repository.GuardarAsync(cancellationToken);
            }
            catch (ConflictoPersistenciaException)
            {
                return ResultadoOperacion.Fallo(
                    "El número de documento ya está registrado.",
                    TipoErrorOperacion.Conflicto);
            }

            return ResultadoOperacion.Exito();
        }

        public async Task<ResultadoOperacion> CambiarEstadoAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var organizacion = await _repository.ObtenerParaModificarAsync(id, cancellationToken);
            var estado = ValidarRegistro(organizacion);

            if (estado is not null)
            {
                return estado;
            }

            if (!organizacion!.EstaActivo &&
                !await _repository.TipoValidoAsync(
                    organizacion.TipoOrganizacionId,
                    cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "No se puede activar una organización con un tipo inexistente o inactivo.");
            }

            organizacion.EstaActivo = !organizacion.EstaActivo;
            organizacion.FechaModificacion = DateTime.UtcNow;
            await _repository.GuardarAsync(cancellationToken);

            return ResultadoOperacion.Exito();
        }

        public async Task<ResultadoOperacion> EliminarAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var organizacion = await _repository.ObtenerParaModificarAsync(id, cancellationToken);
            var estado = ValidarRegistro(organizacion);

            if (estado is not null)
            {
                return estado;
            }

            organizacion!.EstaActivo = false;
            organizacion.EstaEliminado = true;
            organizacion.FechaModificacion = DateTime.UtcNow;
            await _repository.GuardarAsync(cancellationToken);

            return ResultadoOperacion.Exito();
        }

        private static ResultadoOperacion? ValidarRegistro(Organizacion? organizacion)
        {
            if (organizacion is null)
            {
                return ResultadoOperacion.Fallo(
                    "La organización no existe.",
                    TipoErrorOperacion.NoEncontrado);
            }

            return organizacion.EstaEliminado
                ? ResultadoOperacion.Fallo(
                    "La organización fue eliminada y no admite operaciones.",
                    TipoErrorOperacion.NoEncontrado)
                : null;
        }

        private static string? Validar(
            Guid tipoOrganizacionId,
            string nombreComercial,
            string? razonSocial,
            string numeroDocumento,
            string? telefono,
            string? correo,
            string? direccionPrincipal,
            string? logoUrl)
        {
            if (tipoOrganizacionId == Guid.Empty)
                return "El tipo de organización es obligatorio.";
            if (string.IsNullOrWhiteSpace(nombreComercial))
                return "El nombre comercial es obligatorio.";
            if (nombreComercial.Trim().Length > 150)
                return "El nombre comercial no puede superar 150 caracteres.";
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return "El número de documento es obligatorio.";
            if (numeroDocumento.Trim().Length > 20)
                return "El número de documento no puede superar 20 caracteres.";
            if (razonSocial?.Trim().Length > 200)
                return "La razón social no puede superar 200 caracteres.";
            if (telefono?.Trim().Length > 30)
                return "El teléfono no puede superar 30 caracteres.";
            if (correo?.Trim().Length > 256)
                return "El correo no puede superar 256 caracteres.";
            if (!string.IsNullOrWhiteSpace(correo) && !MailAddress.TryCreate(correo.Trim(), out _))
                return "El correo no tiene un formato válido.";
            if (direccionPrincipal?.Trim().Length > 250)
                return "La dirección principal no puede superar 250 caracteres.";
            if (logoUrl?.Trim().Length > 500)
                return "La URL del logo no puede superar 500 caracteres.";

            return null;
        }

        private static string? LimpiarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}

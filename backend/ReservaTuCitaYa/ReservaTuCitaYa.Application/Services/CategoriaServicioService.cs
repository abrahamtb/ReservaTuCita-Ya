using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Services;

public sealed class CategoriaServicioService(
    ICategoriaServicioRepository categoriaRepository,
    IOrganizacionRepository organizacionRepository) : ICategoriaServicioService
{
    public async Task<ResultadoOperacion<PaginaResultado<CategoriaServicioListaDto>>> ListarAsync(
        CategoriaServicioFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(
            filtro.OrganizacionId, cancellationToken);
        if (organizacion is null || organizacion.EstaEliminado)
            return ResultadoOperacion<PaginaResultado<CategoriaServicioListaDto>>.Fallo(
                "La organización no existe o fue eliminada.", TipoErrorOperacion.NoEncontrado);

        return ResultadoOperacion<PaginaResultado<CategoriaServicioListaDto>>.Exito(
            await categoriaRepository.ListarAsync(filtro, cancellationToken));
    }

    public async Task<ResultadoOperacion<CategoriaServicioDetalleDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var detalle = await categoriaRepository.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null
            ? ResultadoOperacion<CategoriaServicioDetalleDto>.Fallo(
                "La categoría no existe o fue eliminada.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<CategoriaServicioDetalleDto>.Exito(detalle);
    }

    public Task<IReadOnlyList<CategoriaServicioOpcionDto>> ListarActivasAsync(
        Guid organizacionId,
        CancellationToken cancellationToken = default) =>
        categoriaRepository.ListarActivasAsync(organizacionId, cancellationToken);

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearCategoriaServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var error = Validar(solicitud.Nombre, solicitud.Descripcion);
        if (error is not null)
            return ResultadoOperacion<Guid>.Fallo(error);

        var errorOrganizacion = await ValidarOrganizacionActivaAsync(
            solicitud.OrganizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion<Guid>.Fallo(errorOrganizacion);

        var nombre = solicitud.Nombre.Trim();
        if (await categoriaRepository.ExisteNombreActivoAsync(
                solicitud.OrganizacionId, nombre, cancellationToken: cancellationToken))
        {
            return ResultadoOperacion<Guid>.Fallo(
                "Ya existe una categoría activa con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        var categoria = new CategoriaServicio
        {
            OrganizacionId = solicitud.OrganizacionId,
            Nombre = nombre,
            Descripcion = LimpiarOpcional(solicitud.Descripcion)
        };
        categoriaRepository.Agregar(categoria);

        try
        {
            await categoriaRepository.GuardarAsync(cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<Guid>.Fallo(
                "Ya existe una categoría activa con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion<Guid>.Exito(categoria.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarCategoriaServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var categoria = await categoriaRepository.ObtenerParaModificarAsync(
            solicitud.Id, cancellationToken);
        var estado = ValidarRegistro(categoria);
        if (estado is not null)
            return estado;

        var error = Validar(solicitud.Nombre, solicitud.Descripcion);
        if (error is not null)
            return ResultadoOperacion.Fallo(error);

        var errorOrganizacion = await ValidarOrganizacionActivaAsync(
            categoria!.OrganizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion.Fallo(errorOrganizacion);

        var nombre = solicitud.Nombre.Trim();
        if (categoria.EstaActivo && await categoriaRepository.ExisteNombreActivoAsync(
                categoria.OrganizacionId, nombre, categoria.Id, cancellationToken))
        {
            return ResultadoOperacion.Fallo(
                "Ya existe una categoría activa con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        categoria.Nombre = nombre;
        categoria.Descripcion = LimpiarOpcional(solicitud.Descripcion);
        categoria.FechaModificacion = DateTime.UtcNow;

        try
        {
            await categoriaRepository.GuardarAsync(cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion.Fallo(
                "Ya existe una categoría activa con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id,
        bool confirmarServiciosActivos,
        CancellationToken cancellationToken = default)
    {
        var categoria = await categoriaRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(categoria);
        if (estado is not null)
            return estado;

        if (categoria!.EstaActivo)
        {
            if (!confirmarServiciosActivos &&
                await categoriaRepository.TieneServiciosActivosAsync(id, cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "La categoría tiene servicios activos. Confirme que desea desactivarla sin modificar esos servicios.");
            }
        }
        else
        {
            var errorOrganizacion = await ValidarOrganizacionActivaAsync(
                categoria.OrganizacionId, cancellationToken);
            if (errorOrganizacion is not null)
                return ResultadoOperacion.Fallo(errorOrganizacion);

            if (await categoriaRepository.ExisteNombreActivoAsync(
                    categoria.OrganizacionId, categoria.Nombre, categoria.Id, cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "No se puede activar la categoría porque ya existe otra activa con el mismo nombre.",
                    TipoErrorOperacion.Conflicto);
            }
        }

        categoria.EstaActivo = !categoria.EstaActivo;
        categoria.FechaModificacion = DateTime.UtcNow;
        await categoriaRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var categoria = await categoriaRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(categoria);
        if (estado is not null)
            return estado;

        if (await categoriaRepository.TieneServiciosAsync(id, cancellationToken))
        {
            return ResultadoOperacion.Fallo(
                "No se puede eliminar la categoría mientras tenga servicios asociados. Reasígnelos primero.");
        }

        categoria!.EstaActivo = false;
        categoria.EstaEliminado = true;
        categoria.FechaModificacion = DateTime.UtcNow;
        await categoriaRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private async Task<string?> ValidarOrganizacionActivaAsync(
        Guid organizacionId,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(
            organizacionId, cancellationToken);
        if (organizacion is null || organizacion.EstaEliminado)
            return "La organización no existe o fue eliminada.";
        return organizacion.EstaActivo
            ? null
            : "La organización está inactiva y no admite categorías.";
    }

    private static ResultadoOperacion? ValidarRegistro(CategoriaServicio? categoria)
    {
        if (categoria is null)
            return ResultadoOperacion.Fallo("La categoría no existe.", TipoErrorOperacion.NoEncontrado);
        return categoria.EstaEliminado
            ? ResultadoOperacion.Fallo(
                "La categoría fue eliminada y no admite operaciones.", TipoErrorOperacion.NoEncontrado)
            : null;
    }

    private static string? Validar(string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre de la categoría es obligatorio.";
        if (nombre.Trim().Length > 150)
            return "El nombre de la categoría no puede superar 150 caracteres.";
        if (descripcion?.Trim().Length > 500)
            return "La descripción no puede superar 500 caracteres.";
        return null;
    }

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

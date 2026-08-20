using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Api.Contracts.Usuarios;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = Permissions.Usuarios.Ver)]
public sealed class UsuariosController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICurrentUser _currentUser;
    private readonly ApplicationDbContext _db;

    public UsuariosController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICurrentUser currentUser,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUser = currentUser;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        // Aislamiento por organización: si no es Superadmin, solo ve usuarios de su organización
        var query = _db.Users.AsQueryable();

        if (!_currentUser.IsInRole(RoleNames.Superadministrador) && _currentUser.OrganizacionId is Guid orgId)
        {
            var usuarioIdsDeLaOrg = _db.UsuariosOrganizaciones
                .Where(uo => uo.OrganizacionId == orgId)
                .Select(uo => uo.UsuarioId);

            query = query.Where(u => usuarioIdsDeLaOrg.Contains(u.Id));
        }

        var usuarios = await query.ToListAsync(cancellationToken);
        var respuesta = new List<UsuarioResponse>();

        foreach (var u in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var org = await _db.UsuariosOrganizaciones
                .Where(uo => uo.UsuarioId == u.Id)
                .Select(uo => (Guid?)uo.OrganizacionId)
                .FirstOrDefaultAsync(cancellationToken);

            respuesta.Add(new UsuarioResponse(
                Guid.Parse(u.Id), u.Email ?? "", u.Nombres, u.Apellidos,
                u.EstaActivo, roles.ToArray(), org));
        }

        return Ok(respuesta);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Usuarios.Gestionar)]
    public async Task<ActionResult<UsuarioResponse>> Crear(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        // Regla: un Administrador no puede crear un Superadministrador
        if (request.Rol == RoleNames.Superadministrador &&
            !_currentUser.IsInRole(RoleNames.Superadministrador))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acción no permitida",
                detail: "No puedes asignar el rol Superadministrador.");
        }

        if (!RoleNames.Todos.Contains(request.Rol))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Rol inválido",
                detail: $"El rol '{request.Rol}' no existe.");
        }

        var organizacionId = request.OrganizacionId ?? _currentUser.OrganizacionId;
        Cliente? clienteVinculado = null;
        Empleado? empleadoVinculado = null;
        if (request.Rol == RoleNames.Cliente)
        {
            if (request.ClienteId is null || organizacionId is null)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Cliente requerido", detail: "Selecciona el cliente que tendrá acceso a su cuenta.");

            clienteVinculado = await _db.Clientes.FirstOrDefaultAsync(cliente =>
                cliente.Id == request.ClienteId && cliente.OrganizacionId == organizacionId &&
                cliente.EstaActivo && !cliente.EstaEliminado, cancellationToken);
            if (clienteVinculado is null)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Cliente inválido", detail: "El cliente seleccionado no pertenece a la organización activa.");

            var yaVinculado = await _db.UsuariosClientes.AnyAsync(link =>
                link.ClienteId == clienteVinculado.Id && link.EstaActivo && !link.EstaEliminado, cancellationToken);
            if (yaVinculado)
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Cliente ya vinculado", detail: "Este cliente ya tiene una cuenta de acceso.");
        }

        if (request.Rol == RoleNames.Profesional)
        {
            if (request.EmpleadoId is null || organizacionId is null)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Profesional requerido", detail: "Selecciona el profesional que tendrá acceso a su cuenta.");

            empleadoVinculado = await _db.Empleados.FirstOrDefaultAsync(empleado =>
                empleado.Id == request.EmpleadoId && empleado.OrganizacionId == organizacionId &&
                empleado.EsProfesional && empleado.EstaActivo && !empleado.EstaEliminado, cancellationToken);
            if (empleadoVinculado is null)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Profesional inválido", detail: "El profesional seleccionado no pertenece a la organización activa o no está disponible.");

            var yaVinculado = await _db.UsuariosEmpleados.AnyAsync(link =>
                link.EmpleadoId == empleadoVinculado.Id && link.EstaActivo && !link.EstaEliminado, cancellationToken);
            if (yaVinculado)
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Profesional ya vinculado", detail: "Este profesional ya tiene una cuenta de acceso.");
        }

        var nuevoUsuario = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            NumeroDocumento = request.NumeroDocumento,
            Telefono = request.Telefono,
            EstaActivo = true,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, request.Password);
        if (!resultado.Succeeded)
        {
            var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo crear el usuario",
                detail: errores);
        }

        await _userManager.AddToRoleAsync(nuevoUsuario, request.Rol);

        // Vincular organización (obligatorio salvo Superadministrador)
        if (organizacionId is Guid orgId)
        {
            _db.UsuariosOrganizaciones.Add(new UsuarioOrganizacion
            {
                UsuarioId = nuevoUsuario.Id,
                OrganizacionId = orgId,
                EsPrincipal = true
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (clienteVinculado is not null)
        {
            _db.UsuariosClientes.Add(new UsuarioCliente { UsuarioId = nuevoUsuario.Id, ClienteId = clienteVinculado.Id });
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (empleadoVinculado is not null)
        {
            _db.UsuariosEmpleados.Add(new UsuarioEmpleado { UsuarioId = nuevoUsuario.Id, EmpleadoId = empleadoVinculado.Id });
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (request.Rol != RoleNames.Superadministrador)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Organización requerida",
                detail: "Debes especificar una organización para este usuario.");
        }

        var roles = await _userManager.GetRolesAsync(nuevoUsuario);
        return Ok(new UsuarioResponse(
            Guid.Parse(nuevoUsuario.Id), nuevoUsuario.Email!, nuevoUsuario.Nombres,
            nuevoUsuario.Apellidos, nuevoUsuario.EstaActivo, roles.ToArray(), organizacionId));
    }

    [HttpPatch("{id}/estado")]
    [Authorize(Policy = Permissions.Usuarios.Gestionar)]
    public async Task<IActionResult> CambiarEstado(string id, CancellationToken cancellationToken)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        usuario.EstaActivo = !usuario.EstaActivo;
        await _userManager.UpdateAsync(usuario);

        return NoContent();
    }

    [HttpPut("{id}/roles")]
    [Authorize(Policy = Permissions.Usuarios.Gestionar)]
    public async Task<IActionResult> AsignarRol(
        string id, AsignarRolRequest request, CancellationToken cancellationToken)
    {
        if (request.Rol == RoleNames.Superadministrador &&
            !_currentUser.IsInRole(RoleNames.Superadministrador))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acción no permitida",
                detail: "No puedes asignar el rol Superadministrador.");
        }

        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
        await _userManager.AddToRoleAsync(usuario, request.Rol);

        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = Permissions.Roles.Ver)]
public sealed class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }

    // GET: /api/roles
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> ListarRoles(
        CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .Where(r => r.Name != null)
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    // GET: /api/roles/permisos
    [HttpGet("permisos")]
    public async Task<ActionResult<IReadOnlyList<PermisoResponse>>> ListarPermisos(
        CancellationToken cancellationToken)
    {
        var permisos = await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .Select(p => new PermisoResponse(
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Descripcion))
            .ToListAsync(cancellationToken);

        return Ok(permisos);
    }

    // GET: /api/roles/{rol}/permisos
    [HttpGet("{rol}/permisos")]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerPermisosRol(
        string rol,
        CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByNameAsync(rol);

        if (role is null)
        {
            return NotFound(new
            {
                message = $"El rol '{rol}' no existe."
            });
        }

        var permisos = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.Permission.Codigo)
            .OrderBy(codigo => codigo)
            .ToListAsync(cancellationToken);

        return Ok(permisos);
    }

    // PUT: /api/roles/{rol}/permisos
    [HttpPut("{rol}/permisos")]
    [Authorize(Policy = Permissions.Roles.Gestionar)]
    public async Task<IActionResult> GuardarPermisosRol(
        string rol,
        GuardarPermisosRolRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByNameAsync(rol);

        if (role is null)
        {
            return NotFound(new
            {
                message = $"El rol '{rol}' no existe."
            });
        }

        var codigosSolicitados = request.Permisos
            .Where(codigo => !string.IsNullOrWhiteSpace(codigo))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var permisosExistentes = await _db.Permissions
            .Where(p => codigosSolicitados.Contains(p.Codigo))
            .ToListAsync(cancellationToken);

        if (permisosExistentes.Count != codigosSolicitados.Count)
        {
            var encontrados = permisosExistentes
                .Select(p => p.Codigo)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inexistentes = codigosSolicitados
                .Where(codigo => !encontrados.Contains(codigo))
                .ToArray();

            return BadRequest(new
            {
                message = "Uno o más permisos no existen.",
                permisos = inexistentes
            });
        }

        var permisosActuales = await _db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        _db.RolePermissions.RemoveRange(permisosActuales);

        var nuevosPermisos = permisosExistentes
            .Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            })
            .ToList();

        _db.RolePermissions.AddRange(nuevosPermisos);

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record PermisoResponse(
        Guid Id,
        string Codigo,
        string Nombre,
        string? Descripcion);

    public sealed record GuardarPermisosRolRequest(
        IReadOnlyList<string> Permisos);
}
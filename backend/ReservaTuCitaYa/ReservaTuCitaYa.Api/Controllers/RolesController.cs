using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions;
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
    private readonly ICurrentUser _currentUser;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _roleManager = roleManager;
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<string>>> Listar(CancellationToken ct)
    {
        var roles = await _roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToArrayAsync(ct);
        return Ok(roles);
    }

    [HttpGet("permisos")]
    public async Task<ActionResult<IReadOnlyCollection<PermisoResponse>>> ListarPermisos(CancellationToken ct)
    {
        var permisos = await _db.Permissions
            .OrderBy(p => p.Codigo)
            .Select(p => new PermisoResponse(p.Id, p.Codigo, p.Nombre, p.Descripcion))
            .ToArrayAsync(ct);
        return Ok(permisos);
    }

    [HttpGet("{rol}/permisos")]
    public async Task<ActionResult<IReadOnlyCollection<string>>> ObtenerPermisos(string rol, CancellationToken ct)
    {
        var role = await _roleManager.FindByNameAsync(rol);
        if (role is null) return NotFound();

        var permisos = await _db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p.Codigo)
            .OrderBy(c => c)
            .ToArrayAsync(ct);
        return Ok(permisos);
    }

    [HttpPut("{rol}/permisos")]
    [Authorize(Policy = Permissions.Roles.Gestionar)]
    public async Task<IActionResult> GuardarPermisos(
        string rol,
        GuardarPermisosRolRequest request,
        CancellationToken ct)
    {
        var role = await _roleManager.FindByNameAsync(rol);
        if (role is null) return NotFound();

        if (rol == RoleNames.Superadministrador && !_currentUser.IsInRole(RoleNames.Superadministrador))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acción no permitida",
                detail: "Solo un Superadministrador puede modificar sus permisos.");
        }

        var codigos = request.Permisos
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permisosValidos = await _db.Permissions
            .Where(p => codigos.Contains(p.Codigo))
            .ToDictionaryAsync(p => p.Codigo, p => p.Id, ct);

        var invalidos = codigos.Where(c => !permisosValidos.ContainsKey(c)).ToArray();
        if (invalidos.Length > 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Permisos inválidos",
                detail: $"No existen: {string.Join(", ", invalidos)}");
        }

        var actuales = await _db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(actuales);
        _db.RolePermissions.AddRange(permisosValidos.Values.Select(permissionId => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permissionId
        }));
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}

public sealed record PermisoResponse(Guid Id, string Codigo, string Nombre, string? Descripcion);
public sealed record GuardarPermisosRolRequest(IReadOnlyCollection<string> Permisos);

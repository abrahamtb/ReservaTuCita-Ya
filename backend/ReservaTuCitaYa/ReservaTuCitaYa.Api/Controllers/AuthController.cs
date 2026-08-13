using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Api.Contracts.Auth;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ApplicationDbContext _db;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICurrentUser currentUser,
        ApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _currentUser = currentUser;
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.EstaActivo)
        {
            return UnauthorizedProblem();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            request.Password,
            request.Recordarme,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return UnauthorizedProblem();
        }

        user.FechaUltimoAcceso = DateTime.UtcNow;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await _signInManager.SignOutAsync();
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "No se pudo iniciar la sesión",
                detail: "La sesión no pudo completarse. Inténtalo nuevamente.");
        }

        return Ok(await BuildResponseAsync(user, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null || !user.EstaActivo)
        {
            await _signInManager.SignOutAsync();
            return UnauthorizedProblem();
        }

        return Ok(await BuildResponseAsync(user, cancellationToken));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    private async Task<AuthUserResponse> BuildResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        OrganizacionResumenDto? organizacion = null;

        if (_currentUser.OrganizacionId is Guid organizacionId)
        {
            var org = await _db.Organizaciones
                .Where(o => o.Id == organizacionId)
                .Select(o => new { o.Id, o.NombreComercial })
                .FirstOrDefaultAsync(cancellationToken);

            if (org is not null)
            {
                organizacion = new OrganizacionResumenDto(org.Id, org.NombreComercial);
            }
        }

        return new AuthUserResponse(
            Guid.Parse(user.Id),
            user.Email ?? string.Empty,
            _currentUser.Roles.ToArray(),
            _currentUser.Permissions.ToArray(),
            organizacion,
            _currentUser.ClienteId,
            _currentUser.EmpleadoId);
    }

    private ObjectResult UnauthorizedProblem() => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Credenciales inválidas",
        detail: "El correo o la contraseña no son válidos.");
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Auth;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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

        return Ok(await BuildResponseAsync(user));
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

        return Ok(await BuildResponseAsync(user));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    private async Task<AuthUserResponse> BuildResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUserResponse(user.Id, user.Email ?? string.Empty, roles.ToArray());
    }

    private ObjectResult UnauthorizedProblem() => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Credenciales inválidas",
        detail: "El correo o la contraseña no son válidos.");
}

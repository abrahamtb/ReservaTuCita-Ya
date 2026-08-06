using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Auth;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Route("api/antiforgery")]
public sealed class AntiforgeryController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("token")]
    [ProducesResponseType<AntiforgeryTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<AntiforgeryTokenResponse> GetToken([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new AntiforgeryTokenResponse(
            tokens.RequestToken!,
            tokens.HeaderName ?? "X-XSRF-TOKEN"));
    }
}

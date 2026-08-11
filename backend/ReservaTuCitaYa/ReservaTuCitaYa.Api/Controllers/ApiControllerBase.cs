using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;

namespace ReservaTuCitaYa.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ObjectResult OperationProblem(
        string? detail,
        TipoErrorOperacion errorType,
        string? type = null,
        string? titleOverride = null)
    {
        var (status, title) = errorType switch
        {
            TipoErrorOperacion.NoEncontrado =>
                (StatusCodes.Status404NotFound, "Registro no encontrado"),
            TipoErrorOperacion.Conflicto =>
                (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            _ => (StatusCodes.Status400BadRequest, "No se pudo completar la operación")
        };

        return Problem(
            type: type,
            statusCode: status,
            title: titleOverride ?? title,
            detail: detail ?? "La operación no pudo completarse.");
    }
}

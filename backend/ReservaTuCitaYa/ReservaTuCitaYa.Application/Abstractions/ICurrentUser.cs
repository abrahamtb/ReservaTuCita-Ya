using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Abstractions
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
        IReadOnlyCollection<string> Roles { get; }
        IReadOnlyCollection<string> Permissions { get; }
        Guid? OrganizacionId { get; }
        Guid? ClienteId { get; }
        Guid? EmpleadoId { get; }
        bool HasPermission(string permiso);
        bool IsInRole(string rol);
    }
}

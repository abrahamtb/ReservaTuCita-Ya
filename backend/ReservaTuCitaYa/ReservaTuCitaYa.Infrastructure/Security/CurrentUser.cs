using Microsoft.AspNetCore.Http;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        private bool _cargado;
        private Guid _userId;
        private List<string> _roles = new();
        private List<string> _permissions = new();
        private Guid? _organizacionId;
        private Guid? _clienteId;
        private Guid? _empleadoId;

        public CurrentUser(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public Guid UserId
        {
            get
            {
                CargarSiNecesario();
                return _userId;
            }
        }

        public IReadOnlyCollection<string> Roles
        {
            get
            {
                CargarSiNecesario();
                return _roles;
            }
        }

        public IReadOnlyCollection<string> Permissions
        {
            get
            {
                CargarSiNecesario();
                return _permissions;
            }
        }

        public Guid? OrganizacionId
        {
            get
            {
                CargarSiNecesario();
                return _organizacionId;
            }
        }

        public Guid? ClienteId
        {
            get
            {
                CargarSiNecesario();
                return _clienteId;
            }
        }

        public Guid? EmpleadoId
        {
            get
            {
                CargarSiNecesario();
                return _empleadoId;
            }
        }

        public bool HasPermission(string permiso) => Permissions.Contains(permiso);

        public bool IsInRole(string rol) => Roles.Contains(rol);

        private void CargarSiNecesario()
        {
            if (_cargado || !IsAuthenticated)
            {
                return;
            }

            var principal = _httpContextAccessor.HttpContext!.User;
            var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (idClaim is null || !Guid.TryParse(idClaim, out _userId))
            {
                _cargado = true;
                return;
            }

            _roles = principal.FindAll(ClaimTypes.Role)
    .Select(c => c.Value)
    .ToList();

            _permissions = _db.RolePermissions
                .Where(rp => _db.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                    .Any(ur => ur.UserId == idClaim && ur.RoleId == rp.RoleId))
                .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Codigo)
                .Distinct()
                .ToList();

            var usuarioOrganizacion = _db.UsuariosOrganizaciones
                .Where(uo => uo.UsuarioId == idClaim && uo.EstaActivo && !uo.EstaEliminado)
                .OrderByDescending(uo => uo.EsPrincipal)
                .FirstOrDefault();
            _organizacionId = usuarioOrganizacion?.OrganizacionId;

            var usuarioEmpleado = _db.UsuariosEmpleados
                .Where(ue => ue.UsuarioId == idClaim && ue.EstaActivo && !ue.EstaEliminado)
                .FirstOrDefault();
            _empleadoId = usuarioEmpleado?.EmpleadoId;

            var usuarioCliente = _db.UsuariosClientes
                .Where(uc => uc.UsuarioId == idClaim && uc.EstaActivo && !uc.EstaEliminado)
                .FirstOrDefault();
            _clienteId = usuarioCliente?.ClienteId;
        }
    }
}

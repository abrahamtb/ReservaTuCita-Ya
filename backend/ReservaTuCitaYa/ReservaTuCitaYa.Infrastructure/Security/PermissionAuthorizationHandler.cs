using Microsoft.AspNetCore.Authorization;
using ReservaTuCitaYa.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permiso { get; }

        public PermissionRequirement(string permiso)
        {
            Permiso = permiso;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ICurrentUser _currentUser;

        public PermissionAuthorizationHandler(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (_currentUser.HasPermission(requirement.Permiso))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

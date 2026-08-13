using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}

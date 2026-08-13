using ReservaTuCitaYa.Domain.Common;
using Xunit;

namespace ReservaTuCitaYa.UnitTests.Security
{
    public class PermissionMatrixTests
    {
        [Fact]
        public void Permissions_Todos_NoDebeTenerDuplicados()
        {
            var duplicados = Permissions.Todos
                .GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .ToList();

            Assert.Empty(duplicados);
        }

        [Fact]
        public void Permissions_Todos_DebeTener37Permisos()
        {
            Assert.Equal(37, Permissions.Todos.Count);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IHorarioSedeRepository
{
    Task<IReadOnlyList<HorarioSede>> ListarAsync(Guid sedeId, CancellationToken ct = default);
    void Agregar(HorarioSede horario);
    Task GuardarAsync(CancellationToken ct = default);
    Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken ct = default);
}
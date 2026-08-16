using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.Common.Disponibilidad;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class Rg020Rg024BusinessRulesTests
{
    [Theory]
    [InlineData("10:00", "11:00", "10:30", "11:30", true)]
    [InlineData("10:00", "11:00", "10:15", "10:45", true)]
    [InlineData("10:15", "10:45", "10:00", "11:00", true)]
    [InlineData("10:00", "11:00", "11:00", "12:00", false)]
    public void Solapamiento_UsaIntervalosSemiabiertos(
        string inicioA, string finA, string inicioB, string finB, bool esperado)
    {
        Assert.Equal(esperado, ValidadorIntervalos.SeSuperponen(
            TimeOnly.Parse(inicioA), TimeOnly.Parse(finA),
            TimeOnly.Parse(inicioB), TimeOnly.Parse(finB)));
    }

    [Fact]
    public void HorariosAdyacentes_SonValidos()
    {
        var intervalos = new[]
        {
            new IntervaloHorarioRequest(DiaSemana.Lunes, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            new IntervaloHorarioRequest(DiaSemana.Lunes, new TimeOnly(13, 0), new TimeOnly(18, 0))
        };

        Assert.Null(ValidadorIntervalos.ValidarColeccionSemana(
            intervalos, x => x.DiaSemana, x => x.HoraInicio, x => x.HoraFin));
    }

    [Fact]
    public void HorariosSuperpuestos_SonInvalidos()
    {
        var intervalos = new[]
        {
            new IntervaloHorarioRequest(DiaSemana.Lunes, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            new IntervaloHorarioRequest(DiaSemana.Lunes, new TimeOnly(12, 0), new TimeOnly(15, 0))
        };

        Assert.NotNull(ValidadorIntervalos.ValidarColeccionSemana(
            intervalos, x => x.DiaSemana, x => x.HoraInicio, x => x.HoraFin));
    }

    [Fact]
    public void ExcepcionCerrada_EliminaTodoElHorario()
    {
        var resultado = CalculadorHorarioEfectivo.Calcular(DiaSemana.Sabado,
            [new(DiaSemana.Sabado, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [new(TipoExcepcionHorario.CerradoTodoElDia, null, null)]);

        Assert.Empty(resultado);
    }

    [Fact]
    public void HorarioEspecial_SustituyeElHorarioSemanal()
    {
        var resultado = CalculadorHorarioEfectivo.Calcular(DiaSemana.Sabado,
            [new(DiaSemana.Sabado, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [new(TipoExcepcionHorario.HorarioEspecial, new TimeOnly(10, 0), new TimeOnly(14, 0))]);

        Assert.Equal([new Intervalo(new TimeOnly(10, 0), new TimeOnly(14, 0))], resultado);
    }

    [Fact]
    public void Bloqueo_ParteElIntervaloDisponible()
    {
        var resultado = CalculadorIntervalos.Restar(
            [new(new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [new(new TimeOnly(14, 0), new TimeOnly(15, 0))]);

        Assert.Equal([
            new Intervalo(new TimeOnly(9, 0), new TimeOnly(14, 0)),
            new Intervalo(new TimeOnly(15, 0), new TimeOnly(18, 0))], resultado);
    }

    [Fact]
    public void Slot_ExigePreparacionDuracionYTiempoPosteriorCompletos()
    {
        var slots = GeneradorSlots.Generar(
            [new(new TimeOnly(9, 0), new TimeOnly(18, 0))],
            new TiemposServicio(15, 60, 15), 15);

        Assert.Equal(new TimeOnly(9, 15), slots.First());
        Assert.Equal(new TimeOnly(16, 45), slots.Last());
        Assert.DoesNotContain(new TimeOnly(17, 0), slots);
    }

    [Fact]
    public void Slot_NoCruzaLaMedianoche()
    {
        var slots = GeneradorSlots.Generar(
            [new(new TimeOnly(23, 0), TimeOnly.MaxValue)],
            new TiemposServicio(0, 90, 0), 15);

        Assert.Empty(slots);
    }

    [Fact]
    public void ReservaReprogramada_SigueOcupandoHorario_YCanceladaNo()
    {
        Assert.Contains(EstadoReserva.Reprogramada, EstadosReserva.OcupanHorario);
        Assert.DoesNotContain(EstadoReserva.Cancelada, EstadosReserva.OcupanHorario);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class UsuarioClienteConfiguration : IEntityTypeConfiguration<UsuarioCliente>
    {
        public void Configure(EntityTypeBuilder<UsuarioCliente> builder)
        {
            builder.HasIndex(uc => new { uc.UsuarioId, uc.ClienteId }).IsUnique();

            // FK hacia Cliente se agrega cuando la entidad Cliente exista
        }
    }
}

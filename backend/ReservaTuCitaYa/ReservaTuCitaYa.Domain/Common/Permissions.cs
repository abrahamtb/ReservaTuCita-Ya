using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Common
{
    public static class Permissions
    {
        public static class Organizaciones
        {
            public const string Ver = "organizaciones.ver";
            public const string Gestionar = "organizaciones.gestionar";
        }

        public static class Sedes
        {
            public const string Ver = "sedes.ver";
            public const string Gestionar = "sedes.gestionar";
        }

        public static class Clientes
        {
            public const string Ver = "clientes.ver";
            public const string Crear = "clientes.crear";
            public const string Editar = "clientes.editar";
            public const string Eliminar = "clientes.eliminar";
        }

        public static class Empleados
        {
            public const string Ver = "empleados.ver";
            public const string Gestionar = "empleados.gestionar";
        }

        public static class Servicios
        {
            public const string Ver = "servicios.ver";
            public const string Gestionar = "servicios.gestionar";
        }

        public static class Recursos
        {
            public const string Ver = "recursos.ver";
            public const string Gestionar = "recursos.gestionar";
        }

        public static class Horarios
        {
            public const string Ver = "horarios.ver";
            public const string Gestionar = "horarios.gestionar";
        }

        public static class Reservas
        {
            public const string Ver = "reservas.ver";
            public const string Crear = "reservas.crear";
            public const string Reprogramar = "reservas.reprogramar";
            public const string Cancelar = "reservas.cancelar";
        }

        public static class Atenciones
        {
            public const string Ver = "atenciones.ver";
            public const string MarcarPresente = "atenciones.marcarPresente";
            public const string Iniciar = "atenciones.iniciar";
            public const string Finalizar = "atenciones.finalizar";
        }

        public static class Pagos
        {
            public const string Ver = "pagos.ver";
            public const string Registrar = "pagos.registrar";
            public const string Anular = "pagos.anular";
            public const string Reembolsar = "pagos.reembolsar";
        }

        public static class Dashboard
        {
            public const string Ver = "dashboard.ver";
        }

        public static class Reportes
        {
            public const string Ver = "reportes.ver";
            public const string Exportar = "reportes.exportar";
        }

        public static class Calificaciones
        {
            public const string Ver = "calificaciones.ver";
            public const string Crear = "calificaciones.crear";
        }

        public static class Usuarios
        {
            public const string Ver = "usuarios.ver";
            public const string Gestionar = "usuarios.gestionar";
        }

        public static class Roles
        {
            public const string Ver = "roles.ver";
            public const string Gestionar = "roles.gestionar";
        }

        // Lista completa — útil para el seed, para no tener que enumerar a mano
        public static IReadOnlyList<string> Todos { get; } = new[]
        {
            Organizaciones.Ver, Organizaciones.Gestionar,
            Sedes.Ver, Sedes.Gestionar,
            Clientes.Ver, Clientes.Crear, Clientes.Editar, Clientes.Eliminar,
            Empleados.Ver, Empleados.Gestionar,
            Servicios.Ver, Servicios.Gestionar,
            Recursos.Ver, Recursos.Gestionar,
            Horarios.Ver, Horarios.Gestionar,
            Reservas.Ver, Reservas.Crear, Reservas.Reprogramar, Reservas.Cancelar,
            Atenciones.Ver, Atenciones.MarcarPresente, Atenciones.Iniciar, Atenciones.Finalizar,
            Pagos.Ver, Pagos.Registrar, Pagos.Anular, Pagos.Reembolsar,
            Dashboard.Ver,
            Reportes.Ver, Reportes.Exportar,
            Calificaciones.Ver, Calificaciones.Crear,
            Usuarios.Ver, Usuarios.Gestionar,
            Roles.Ver, Roles.Gestionar,
        };
    }
}

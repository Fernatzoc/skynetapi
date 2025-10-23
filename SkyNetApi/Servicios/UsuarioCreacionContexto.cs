namespace SkyNetApi.Servicios
{
    public class UsuarioCreacionContexto
    {
        private static readonly AsyncLocal<UsuarioCreacionDatos?> _datos = new AsyncLocal<UsuarioCreacionDatos?>();

        public static void Establecer(string firstName, string? middleName, string lastName, string? secondSurname, string phone)
        {
            _datos.Value = new UsuarioCreacionDatos
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                SecondSurname = secondSurname,
                Phone = phone
            };
        }

        public static UsuarioCreacionDatos? Obtener()
        {
            return _datos.Value;
        }

        public static void Limpiar()
        {
            _datos.Value = null;
        }
    }

    public class UsuarioCreacionDatos
    {
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? SecondSurname { get; set; }
        public string Phone { get; set; } = null!;
    }
}


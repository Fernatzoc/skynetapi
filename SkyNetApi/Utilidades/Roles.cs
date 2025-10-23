namespace SkyNetApi.Utilidades
{
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Supervisor = "Supervisor";
        public const string Tecnico = "Tecnico";

        public static readonly string[] TodosLosRoles = 
        {
            Administrador,
            Supervisor,
            Tecnico
        };
    }
}


namespace SkyNetApi.DTOs
{
    public class TecnicoAsignadoDTO
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? SecondSurname { get; set; }
        public string Phone { get; set; } = null!;
    }
}


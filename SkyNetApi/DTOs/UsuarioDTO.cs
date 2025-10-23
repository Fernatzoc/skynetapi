namespace SkyNetApi.DTOs
{
    public class UsuarioDTO
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? SecondSurname { get; set; }
        public string? Phone { get; set; }
        public bool Status { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}


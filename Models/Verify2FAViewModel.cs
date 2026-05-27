namespace SDSP1.Models
{
    public class Verify2FAViewModel
    {
        public int UsuarioId { get; set; }
        public string Codigo { get; set; }
        public required string Celular { get; set; }
    }
}
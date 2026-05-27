namespace SDSP1.Models
{
    public class Verify2FAViewModel
    {
        public int UsuarioId { get; set; }
        public required string Codigo { get; set; }
    }
}
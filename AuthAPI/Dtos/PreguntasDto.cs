namespace AuthAPI.Dtos
{
    public class RespuestaFAQDto
    {
        public int Id { get; set; }
        public int PreguntaId { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string? MensajeRespuesta { get; set; }
        public string? RespondidoPor { get; set; }
        public PreguntaFAQDto? Pregunta { get; set; }
    }

    public class PreguntaFAQDto
    {
        public int Id { get; set; }
        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? CorreoUsuario { get; set; }
        public string? Pregunta { get; set; }
        public DateTime FechaPregunta { get; set; }
        public bool EsDestacada { get; set; }
        public int? RespuestaId { get; set; }
        public RespuestaFAQDto? Respuesta { get; set; }
    }
}
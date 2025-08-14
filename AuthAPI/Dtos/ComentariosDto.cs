namespace AuthAPI.Dtos
{
    public class RespuestaComentarioDto
    {
        public int Id { get; set; }
        public int ComentarioId { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string? MensajeRespuesta { get; set; }
        public string? RespondidoPor { get; set; }
        public ComentariosDto? Comentario { get; set; }

    }

    public class ComentariosDto
    {
        public int Id { get; set; }
        public string? ClienteId { get; set; }
        public string? NombreCliente { get; set; }
        public string? CorreoCliente { get; set; }
        public string? Mensaje { get; set; }
        public DateTime FechaComentario { get; set; }

        public int? RespuestaId { get; set; }
        public RespuestaComentarioDto? Respuesta { get; set; }
    }
}

using AuthAPI.Data;
using AuthAPI.Dtos;
using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComentariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentariosDto>>> GetAll()
        {
            var comentarios = await _context.Comentarios
                .Include(c => c.Respuesta)
                .ToListAsync();

            // Mapeo de los modelos a los DTOs
            var comentariosDto = comentarios.Select(c => new ComentariosDto
            {
                Id = c.Id,
                ClienteId = c.ClienteId,
                NombreCliente = c.NombreCliente,
                CorreoCliente = c.CorreoCliente,
                Mensaje = c.Mensaje,
                FechaComentario = c.FechaComentario,
                Respuesta = c.Respuesta != null ? new RespuestaComentarioDto
                {
                    Id = c.Respuesta.Id,
                    ComentarioId = c.Respuesta.ComentarioId,
                    FechaRespuesta = c.Respuesta.FechaRespuesta,
                    MensajeRespuesta = c.Respuesta.MensajeRespuesta,
                    RespondidoPor = c.Respuesta.RespondidoPor
                } : null
            }).ToList();

            return Ok(comentariosDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ComentariosDto>> GetById(int id)
        {
            var comentario = await _context.Comentarios
                .Include(c => c.Respuesta)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario == null)
                return NotFound();

            // Mapeo del modelo a un DTO
            var comentarioDto = new ComentariosDto
            {
                Id = comentario.Id,
                ClienteId = comentario.ClienteId,
                NombreCliente = comentario.NombreCliente,
                CorreoCliente = comentario.CorreoCliente,
                Mensaje = comentario.Mensaje,
                FechaComentario = comentario.FechaComentario,
                Respuesta = comentario.Respuesta != null ? new RespuestaComentarioDto
                {
                    Id = comentario.Respuesta.Id,
                    ComentarioId = comentario.Respuesta.ComentarioId,
                    FechaRespuesta = comentario.Respuesta.FechaRespuesta,
                    MensajeRespuesta = comentario.Respuesta.MensajeRespuesta,
                    RespondidoPor = comentario.Respuesta.RespondidoPor
                } : null
            };

            return Ok(comentarioDto);
        }

        [HttpPost]
        public async Task<ActionResult<ComentariosDto>> Create(ComentariosDto nuevoComentarioDto)
        {
            // Mapeo del DTO al modelo
            var comentario = new ComentariosDto
            {
                ClienteId = nuevoComentarioDto.ClienteId,
                NombreCliente = nuevoComentarioDto.NombreCliente,
                CorreoCliente = nuevoComentarioDto.CorreoCliente,
                Mensaje = nuevoComentarioDto.Mensaje,
                FechaComentario = DateTime.Now
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            // Mapeo de vuelta a DTO para la respuesta
            nuevoComentarioDto.Id = comentario.Id;
            nuevoComentarioDto.FechaComentario = comentario.FechaComentario;

            return CreatedAtAction(nameof(GetById), new { id = comentario.Id }, nuevoComentarioDto);
        }

        [HttpPost("responder/{id}")]
        public async Task<IActionResult> ResponderComentario(int id, [FromBody] string respuesta)
        {
            var comentario = await _context.Comentarios
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario == null)
                return NotFound();

            var nuevaRespuesta = new RespuestaComentarioDto
            {
                ComentarioId = comentario.Id,
                FechaRespuesta = DateTime.Now,
                MensajeRespuesta = respuesta,
                RespondidoPor = "Administrador" // O el usuario autenticado
            };

            _context.RespuestasComentario.Add(nuevaRespuesta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
                return NotFound();

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
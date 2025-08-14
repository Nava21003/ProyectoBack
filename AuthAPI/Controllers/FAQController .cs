using AuthAPI.Data;
using AuthAPI.Dtos;
using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FAQController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FAQController(AppDbContext context)
        {
            _context = context;
        }

        // Obtener todas las preguntas
        [HttpGet]
        public async Task<ActionResult<List<PreguntaFAQDto>>> GetAll()
        {
            var preguntas = await _context.PreguntasFAQ
                .Include(p => p.Respuesta)
                .ToListAsync();

            var preguntasDto = preguntas.Select(p => new PreguntaFAQDto
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.NombreUsuario,
                CorreoUsuario = p.CorreoUsuario,
                Pregunta = p.Pregunta,
                FechaPregunta = p.FechaPregunta,
                EsDestacada = p.EsDestacada,
                Respuesta = p.Respuesta != null ? new RespuestaFAQDto
                {
                    Id = p.Respuesta.Id,
                    PreguntaId = p.Respuesta.PreguntaId,
                    FechaRespuesta = p.Respuesta.FechaRespuesta,
                    MensajeRespuesta = p.Respuesta.MensajeRespuesta,
                    RespondidoPor = p.Respuesta.RespondidoPor
                } : null
            }).ToList();

            return Ok(preguntasDto);
        }

        // Obtener preguntas destacadas
        [HttpGet("destacadas")]
        public async Task<ActionResult<List<PreguntaFAQDto>>> GetDestacadas()
        {
            var destacadas = await _context.PreguntasFAQ
                .Include(p => p.Respuesta)
                .Where(p => p.EsDestacada)
                .ToListAsync();

            var destacadasDto = destacadas.Select(p => new PreguntaFAQDto
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.NombreUsuario,
                CorreoUsuario = p.CorreoUsuario,
                Pregunta = p.Pregunta,
                FechaPregunta = p.FechaPregunta,
                EsDestacada = p.EsDestacada,
                Respuesta = p.Respuesta != null ? new RespuestaFAQDto
                {
                    Id = p.Respuesta.Id,
                    PreguntaId = p.Respuesta.PreguntaId,
                    FechaRespuesta = p.Respuesta.FechaRespuesta,
                    MensajeRespuesta = p.Respuesta.MensajeRespuesta,
                    RespondidoPor = p.Respuesta.RespondidoPor
                } : null
            }).ToList();

            return Ok(destacadasDto);
        }

        // Obtener pregunta por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PreguntaFAQDto>> GetById(int id)
        {
            var pregunta = await _context.PreguntasFAQ
                .Include(p => p.Respuesta)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            var preguntaDto = new PreguntaFAQDto
            {
                Id = pregunta.Id,
                UsuarioId = pregunta.UsuarioId,
                NombreUsuario = pregunta.NombreUsuario,
                CorreoUsuario = pregunta.CorreoUsuario,
                Pregunta = pregunta.Pregunta,
                FechaPregunta = pregunta.FechaPregunta,
                EsDestacada = pregunta.EsDestacada,
                Respuesta = pregunta.Respuesta != null ? new RespuestaFAQDto
                {
                    Id = pregunta.Respuesta.Id,
                    PreguntaId = pregunta.Respuesta.PreguntaId,
                    FechaRespuesta = pregunta.Respuesta.FechaRespuesta,
                    MensajeRespuesta = pregunta.Respuesta.MensajeRespuesta,
                    RespondidoPor = pregunta.Respuesta.RespondidoPor
                } : null
            };

            return Ok(preguntaDto);
        }

        // Crear nueva pregunta
        [HttpPost]
        public async Task<ActionResult<PreguntaFAQDto>> Create(PreguntaFAQDto nuevaPreguntaDto)
        {
            var pregunta = new PreguntaFAQDto
            {
                UsuarioId = nuevaPreguntaDto.UsuarioId,
                NombreUsuario = nuevaPreguntaDto.NombreUsuario,
                CorreoUsuario = nuevaPreguntaDto.CorreoUsuario,
                Pregunta = nuevaPreguntaDto.Pregunta,
                FechaPregunta = DateTime.Now,
                EsDestacada = false
            };

            _context.PreguntasFAQ.Add(pregunta);
            await _context.SaveChangesAsync();

            nuevaPreguntaDto.Id = pregunta.Id;
            nuevaPreguntaDto.FechaPregunta = pregunta.FechaPregunta;

            return CreatedAtAction(nameof(GetById), new { id = pregunta.Id }, nuevaPreguntaDto);
        }

        // Responder pregunta
        [HttpPost("responder/{id}")]
        public async Task<IActionResult> ResponderPregunta(int id, [FromBody] string respuestaMensaje)
        {
            // Buscar la pregunta e incluir la respuesta si ya existe
            var pregunta = await _context.PreguntasFAQ
                                         .Include(p => p.Respuesta)
                                         .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
            {
                return NotFound();
            }

            // Si la pregunta no tiene una respuesta, crear una nueva.
            if (pregunta.Respuesta == null)
            {
                var nuevaRespuesta = new RespuestaFAQDto
                {
                    PreguntaId = pregunta.Id,
                    FechaRespuesta = DateTime.Now,
                    MensajeRespuesta = respuestaMensaje,
                    RespondidoPor = "Administrador" // O el usuario autenticado
                };

                _context.RespuestasFAQ.Add(nuevaRespuesta);
            }
            else
            {
                // Si ya tiene una respuesta, actualizarla.
                pregunta.Respuesta.MensajeRespuesta = respuestaMensaje;
                pregunta.Respuesta.FechaRespuesta = DateTime.Now;
                pregunta.Respuesta.RespondidoPor = "Administrador"; // O el usuario autenticado
                _context.RespuestasFAQ.Update(pregunta.Respuesta);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Destacar/Quitar destacado de pregunta
        [HttpPost("destacar/{id}")]
        public async Task<IActionResult> ToggleDestacarPregunta(int id)
        {
            var pregunta = await _context.PreguntasFAQ.FindAsync(id);
            if (pregunta == null)
                return NotFound();

            pregunta.EsDestacada = !pregunta.EsDestacada;
            _context.PreguntasFAQ.Update(pregunta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Eliminar pregunta
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pregunta = await _context.PreguntasFAQ.FindAsync(id);
            if (pregunta == null)
                return NotFound();

            _context.PreguntasFAQ.Remove(pregunta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

using AuthAPI.Dtos;
using AuthAPI.Data;
using AuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IVentaService _ventaService;

        public VentaController(AppDbContext context, IVentaService ventaService)
        {
            _context = context;
            _ventaService = ventaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VentaDto>>> GetAll()
        {
            return Ok(await _context.Ventas.Include(v => v.Detalles).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetById(int id)
        {
            var venta = await _context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null)
                return NotFound();

            return Ok(venta);
        }

        [HttpGet("estatus/{estatus}")]
        public async Task<ActionResult<List<VentaDto>>> GetByEstatus(string estatus)
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.Estatus.Equals(estatus, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
            return Ok(ventas);
        }

        [HttpPost]
        public async Task<ActionResult<VentaDto>> Create([FromBody] VentaDto nuevaVenta)
        {
            try
            {
                var venta = await _ventaService.CrearVentaAsync(nuevaVenta);
                return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}/completar")]
        public async Task<ActionResult> CompletarVenta(int id)
        {
            try
            {
                await _ventaService.CompletarVentaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no encontrada"))
                {
                    return NotFound(ex.Message);
                }
                if (ex.Message.Contains("ya está completada"))
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, $"Error al completar venta: {ex.Message}");
            }
        }

        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult> CancelarVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
                return NotFound();

            if (venta.Estatus == "Completada")
                return BadRequest("No se puede cancelar una venta completada");

            venta.Estatus = "Cancelada";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/marcar-pagado")]
        public async Task<ActionResult> MarcarComoPagado(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
                return NotFound();

            venta.Pagado = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
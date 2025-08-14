using AuthAPI.Data;
using AuthAPI.Dtos;
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
    public class InventarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public InventarioController(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<ActionResult<List<InventarioDto>>> GetAll()
        {
            return Ok(await _context.Inventario.ToListAsync());
        }

        [HttpGet("producto/{productoId}")]
        public async Task<ActionResult<InventarioDto>> GetByProductoId(int productoId)
        {
            var item = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == productoId);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpGet("movimientos")]
        public async Task<ActionResult<List<MovimientoInventarioDto>>> GetMovimientosApi()
        {
            return Ok(await _context.MovimientosInventario.ToListAsync());
        }

        [HttpGet("movimientos/producto/{productoId}")]
        public async Task<ActionResult<List<MovimientoInventarioDto>>> GetMovimientosByProducto(int productoId)
        {
            var movs = await _context.MovimientosInventario
                .Where(m => m.ProductoId == productoId)
                .ToListAsync();
            return Ok(movs);
        }

        [HttpPost("entrada")]
        public async Task<ActionResult<InventarioDto>> RegistrarEntrada([FromBody] MovimientoInventarioDto movimiento)
        {
            try
            {
                var result = await _inventarioService.RegistrarEntradaAsync(movimiento);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("salida")]
        public async Task<ActionResult<InventarioDto>> RegistrarSalida([FromBody] MovimientoInventarioDto movimiento)
        {
            try
            {
                var result = await _inventarioService.RegistrarSalidaAsync(movimiento);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no encontrado"))
                {
                    return NotFound(ex.Message);
                }
                if (ex.Message.Contains("suficientes existencias"))
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("precio-promedio/{productoId}")]
        public async Task<ActionResult<decimal?>> ObtenerPrecioPromedio(int productoId)
        {
            var precio = await _inventarioService.ObtenerPrecioPromedioAsync(productoId);
            if (precio == null)
                return NotFound("Producto no encontrado en inventario");

            return Ok(precio);
        }

        [HttpGet("disponibilidad/{productoId}/{cantidad}")]
        public async Task<ActionResult<bool>> VerificarDisponibilidad(int productoId, int cantidad)
        {
            var disponible = await _inventarioService.VerificarDisponibilidadAsync(productoId, cantidad);
            return Ok(disponible);
        }

        [HttpGet("existencias/{productoId}")]
        public async Task<ActionResult<int?>> ObtenerExistencias(int productoId)
        {
            var existencias = await _inventarioService.ObtenerExistenciasAsync(productoId);
            if (existencias == null)
                return NotFound("Producto no encontrado en inventario");

            return Ok(existencias);
        }
    }
}
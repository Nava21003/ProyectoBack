using AuthAPI.Dtos;
using AuthAPI.Data;
using AuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public ComprasController(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CompraProveedorDto>>> GetAll()
        {
            return Ok(await _context.Compras.Include(c => c.Detalles).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraProveedorDto>> GetById(int id)
        {
            var compra = await _context.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
            if (compra == null)
                return NotFound();

            return Ok(compra);
        }

        [HttpPost]
        public async Task<ActionResult<CompraProveedorDto>> Create([FromBody] CompraProveedorDto nuevaCompra)
        {
            if (nuevaCompra.Detalles.Any(d => d.PrecioUnitario <= 0))
            {
                return BadRequest("El precio unitario debe ser mayor que 0");
            }

            nuevaCompra.FechaCompra = System.DateTime.Now;
            nuevaCompra.Pagado = false;

            _context.Compras.Add(nuevaCompra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevaCompra.Id }, nuevaCompra);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CompraProveedorDto>> Update(int id, [FromBody] CompraProveedorDto compraActualizada)
        {
            var compraExistente = await _context.Compras.FirstOrDefaultAsync(c => c.Id == id);
            if (compraExistente == null)
                return NotFound();

            if (compraActualizada.Detalles.Any(d => d.PrecioUnitario <= 0))
            {
                return BadRequest("El precio unitario debe ser mayor que 0");
            }

            compraExistente.ProveedorId = compraActualizada.ProveedorId;
            compraExistente.NombreProveedor = compraActualizada.NombreProveedor;
            compraExistente.Pagado = compraActualizada.Pagado;

            _context.Entry(compraExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(compraExistente);
        }

        [HttpPut("cancelar/{id}")]
        public async Task<ActionResult> Cancelar(int id)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound();

            _context.Compras.Remove(compra);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("pagar/{id}")]
        public async Task<ActionResult> Pagar(int id)
        {
            var compra = await _context.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
            if (compra == null)
                return NotFound();

            if (compra.Pagado)
            {
                return BadRequest("La compra ya ha sido pagada.");
            }

            foreach (var detalle in compra.Detalles)
            {
                var movimiento = new MovimientoInventarioDto
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    CompraId = compra.Id
                };
                await _inventarioService.RegistrarEntradaAsync(movimiento);
            }

            compra.Pagado = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
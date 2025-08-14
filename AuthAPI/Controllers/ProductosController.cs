using AuthAPI.Dtos;
using AuthAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        // Usa una inyección de dependencias para obtener una instancia de tu DbContext
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductoDto>>> GetAll()
        {
            // Obtén los productos de la base de datos de forma asíncrona
            return Ok(await _context.Productos.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetById(int id)
        {
            // Busca un producto por ID en la base de datos
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] ProductoDto nuevoProducto)
        {
            // Agrega el nuevo producto al contexto y guarda los cambios en la base de datos
            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevoProducto.Id }, nuevoProducto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ProductoDto productoActualizado)
        {
            if (id != productoActualizado.Id)
            {
                return BadRequest();
            }

            // Marca el producto como modificado y guarda los cambios
            _context.Entry(productoActualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // Busca y elimina el producto de la base de datos
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
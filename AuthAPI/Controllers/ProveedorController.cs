using AuthAPI.Dtos;
using AuthAPI.Data; // Importa el namespace del DbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProveedoresDto>>> GetAll()
        {
            // Obtén todos los proveedores e incluye sus productos
            return Ok(await _context.Proveedores.Include(p => p.Productos).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedoresDto>> GetById(int id)
        {
            // Busca un proveedor por ID e incluye sus productos
            var proveedor = await _context.Proveedores.Include(p => p.Productos).FirstOrDefaultAsync(p => p.Id == id);
            if (proveedor == null)
                return NotFound();

            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<ActionResult<ProveedoresDto>> Create(ProveedoresDto nuevoProveedor)
        {
            _context.Proveedores.Add(nuevoProveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevoProveedor.Id }, nuevoProveedor);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, ProveedoresDto proveedorActualizado)
        {
            if (id != proveedorActualizado.Id)
            {
                return BadRequest();
            }

            _context.Entry(proveedorActualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProveedorExists(id))
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
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound();

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.Id == id);
        }
    }
}
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
    public class CotizacionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;
        private readonly IVentaService _ventaService;

        public CotizacionController(
            AppDbContext context,
            IInventarioService inventarioService,
            IVentaService ventaService)
        {
            _context = context;
            _inventarioService = inventarioService;
            _ventaService = ventaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CotizacionDto>>> GetAll()
        {
            return Ok(await _context.Cotizaciones.Include(c => c.Detalles).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CotizacionDto>> GetById(int id)
        {
            var cotizacion = await _context.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
            if (cotizacion == null)
                return NotFound();

            return Ok(cotizacion);
        }

        [HttpGet("estatus/{estatus}")]
        public async Task<ActionResult<List<CotizacionDto>>> GetByEstatus(string estatus)
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Detalles)
                .Where(c => c.Estatus.Equals(estatus, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
            return Ok(cotizaciones);
        }

        [HttpPost]
        public async Task<ActionResult<CotizacionDto>> Create([FromBody] CotizacionDto nuevaCotizacion)
        {
            try
            {
                if (nuevaCotizacion == null || nuevaCotizacion.Detalles == null || !nuevaCotizacion.Detalles.Any())
                    return BadRequest("La cotización y sus detalles son requeridos");

                foreach (var item in nuevaCotizacion.Detalles)
                {
                    var precioPromedio = await _inventarioService.ObtenerPrecioPromedioAsync(item.ProductoId);

                    if (precioPromedio.HasValue)
                    {
                        item.PrecioUnitario = precioPromedio.Value;
                    }
                    else
                    {
                        return BadRequest($"No se pudo obtener el precio para el producto ID: {item.ProductoId}");
                    }
                }

                nuevaCotizacion.FechaCreacion = DateTime.Now;
                nuevaCotizacion.Estatus = "Pendiente";

                _context.Cotizaciones.Add(nuevaCotizacion);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = nuevaCotizacion.Id }, nuevaCotizacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}/aprobar")]
        public async Task<ActionResult> AprobarCotizacion(int id)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);
            if (cotizacion == null)
                return NotFound();

            cotizacion.Estatus = "Aprobada";
            cotizacion.FechaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/rechazar")]
        public async Task<ActionResult> RechazarCotizacion(int id)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);
            if (cotizacion == null)
                return NotFound();

            cotizacion.Estatus = "Rechazada";
            cotizacion.FechaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/convertir-a-venta")]
        public async Task<ActionResult<VentaDto>> ConvertirAVenta(int id)
        {
            var cotizacion = await _context.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
            if (cotizacion == null)
                return NotFound();

            if (cotizacion.Estatus != "Aprobada")
                return BadRequest("Solo se pueden convertir cotizaciones aprobadas");

            var venta = new VentaDto
            {
                ClienteNombre = cotizacion.ClienteNombre,
                ClienteEmail = cotizacion.ClienteEmail,
                ClienteTelefono = cotizacion.ClienteTelefono,
                Notas = $"Convertido de cotización #{cotizacion.Id}. " + cotizacion.Notas,
                Detalles = cotizacion.Detalles.Select(i => new VentaItemDto
                {
                    ProductoId = i.ProductoId,
                    NombreProducto = i.NombreProducto,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            };

            try
            {
                var createdVenta = await _ventaService.CrearVentaAsync(venta);

                cotizacion.Estatus = "Convertida";
                cotizacion.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(createdVenta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al convertir cotización: {ex.Message}");
            }
        }


        private string GenerarCuerpoCorreo(CotizacionDto cotizacion)
        {
            return $@"
                <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                            h1 {{ color: #2c3e50; }}
                            .cotizacion-info {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; }}
                            .detalle-item {{ margin-bottom: 10px; }}
                        </style>
                    </head>
                    <body>
                        <h1>Nueva Cotización Recibida</h1>
                        
                        <div class='cotizacion-info'>
                            <p><strong>ID:</strong> {cotizacion.Id}</p>
                            <p><strong>Cliente:</strong> {cotizacion.ClienteNombre}</p>
                            <p><strong>Email:</strong> {cotizacion.ClienteEmail}</p>
                            <p><strong>Teléfono:</strong> {cotizacion.ClienteTelefono}</p>
                            <p><strong>Fecha:</strong> {cotizacion.FechaCreacion:dd/MM/yyyy HH:mm}</p>
                            <p><strong>Notas:</strong> {cotizacion.Notas ?? "Sin notas"}</p>
                        </div>
                        
                        <h3>Detalles de la Cotización:</h3>
                        <ul>
                            {string.Join("", cotizacion.Detalles.Select(i =>
                                $"<li class='detalle-item'>{i.NombreProducto} - {i.Cantidad} x {i.PrecioUnitario:C} = {(i.Cantidad * i.PrecioUnitario):C}</li>"))}
                        </ul>
                        
                        <p>Total: {cotizacion.Detalles.Sum(i => i.Cantidad * i.PrecioUnitario):C}</p>
                        
                        <p>Por favor revise el sistema para aprobar o rechazar esta cotización.</p>
                    </body>
                </html>
            ";
        }
    }
}
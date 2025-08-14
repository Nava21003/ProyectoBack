using AuthAPI.Data;
using AuthAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AuthAPI.Services
{
    public class VentaService : IVentaService
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public VentaService(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        public async Task<VentaDto> CrearVentaAsync(VentaDto nuevaVenta)
        {
            if (nuevaVenta.Detalles == null || !nuevaVenta.Detalles.Any())
            {
                throw new ArgumentException("La venta debe contener al menos un item");
            }

            // Obtener precios promedio para cada producto
            foreach (var item in nuevaVenta.Detalles)
            {
                var precioPromedio = await _inventarioService.ObtenerPrecioPromedioAsync(item.ProductoId);
                if (precioPromedio.HasValue)
                {
                    item.PrecioUnitario = precioPromedio.Value;
                }
                else
                {
                    throw new Exception($"Producto con ID {item.ProductoId} no encontrado en inventario");
                }
            }

            // Verificar disponibilidad de productos
            foreach (var item in nuevaVenta.Detalles)
            {
                var disponibilidad = await _inventarioService.VerificarDisponibilidadAsync(item.ProductoId, item.Cantidad);
                if (!disponibilidad)
                {
                    var existencias = await _inventarioService.ObtenerExistenciasAsync(item.ProductoId);
                    throw new Exception($"No hay suficientes existencias para el producto ID {item.ProductoId}. Disponibles: {existencias}, Solicitadas: {item.Cantidad}");
                }
            }

            nuevaVenta.FechaVenta = DateTime.Now;
            nuevaVenta.Estatus = "Procesando";
            nuevaVenta.Pagado = false;

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return nuevaVenta;
        }

        public async Task<VentaDto> CompletarVentaAsync(int ventaId)
        {
            var venta = await _context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == ventaId);
            if (venta == null)
            {
                throw new Exception("Venta no encontrada");
            }

            if (venta.Estatus == "Completada")
            {
                throw new Exception("La venta ya está completada");
            }

            // Registrar salidas de inventario
            foreach (var item in venta.Detalles)
            {
                await _inventarioService.RegistrarSalidaAsync(new MovimientoInventarioDto
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    VentaId = venta.Id
                });
            }

            venta.Estatus = "Completada";
            venta.FechaVenta = DateTime.Now;
            await _context.SaveChangesAsync();

            return venta;
        }
    }
}
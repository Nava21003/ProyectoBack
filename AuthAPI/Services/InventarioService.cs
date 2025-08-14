using AuthAPI.Data;
using AuthAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AuthAPI.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly AppDbContext _context;

        public InventarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventarioDto> RegistrarEntradaAsync(MovimientoInventarioDto movimiento)
        {
            movimiento.TipoMovimiento = "ENTRADA";
            movimiento.Total = movimiento.Cantidad * movimiento.PrecioUnitario;
            movimiento.FechaMovimiento = DateTime.Now;

            var itemInventario = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == movimiento.ProductoId);

            if (itemInventario == null)
            {
                itemInventario = new InventarioDto
                {
                    ProductoId = movimiento.ProductoId,
                    Entradas = movimiento.Cantidad,
                    Salidas = 0,
                    Existencias = movimiento.Cantidad,
                    CostoUnitario = movimiento.PrecioUnitario,
                    CostoTotal = movimiento.Total,
                    PrecioPromedio = movimiento.PrecioUnitario,
                    UltimaActualizacion = DateTime.Now
                };
                _context.Inventario.Add(itemInventario);
            }
            else
            {
                itemInventario.CostoTotal += movimiento.Total;
                itemInventario.Existencias += movimiento.Cantidad;
                itemInventario.Entradas += movimiento.Cantidad;
                itemInventario.PrecioPromedio = itemInventario.CostoTotal / itemInventario.Existencias;
                itemInventario.CostoUnitario = itemInventario.PrecioPromedio;
                itemInventario.UltimaActualizacion = DateTime.Now;
            }

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            return itemInventario;
        }

        public async Task<InventarioDto> RegistrarSalidaAsync(MovimientoInventarioDto movimiento)
        {
            var itemInventario = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == movimiento.ProductoId);
            if (itemInventario == null)
            {
                throw new Exception("Producto no encontrado en inventario");
            }

            if (itemInventario.Existencias < movimiento.Cantidad)
            {
                throw new Exception("No hay suficientes existencias para esta salida");
            }

            var costoSalida = itemInventario.PrecioPromedio * movimiento.Cantidad;

            itemInventario.Salidas += movimiento.Cantidad;
            itemInventario.Existencias -= movimiento.Cantidad;
            itemInventario.CostoTotal -= costoSalida;
            itemInventario.UltimaActualizacion = DateTime.Now;

            movimiento.TipoMovimiento = "SALIDA";
            movimiento.FechaMovimiento = DateTime.Now;
            movimiento.PrecioUnitario = itemInventario.PrecioPromedio;
            movimiento.Total = costoSalida;

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            return itemInventario;
        }

        public async Task<decimal?> ObtenerPrecioPromedioAsync(int productoId)
        {
            var item = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == productoId);
            return item?.PrecioPromedio;
        }

        public async Task<bool> VerificarDisponibilidadAsync(int productoId, int cantidad)
        {
            var item = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == productoId);
            return item != null && item.Existencias >= cantidad;
        }

        public async Task<int?> ObtenerExistenciasAsync(int productoId)
        {
            var item = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == productoId);
            return item?.Existencias;
        }
    }
}
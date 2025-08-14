using System;
using System.Collections.Generic;

namespace AuthAPI.Dtos
{
    public class VentaDto
    {
        public int Id { get; set; }
        public int? CotizacionId { get; set; } // Null si es venta directa
        public string ClienteNombre { get; set; }
        public string ClienteEmail { get; set; }
        public string ClienteTelefono { get; set; }
        public string Notas { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public string Estatus { get; set; } = "Procesando"; // Procesando, Completada, Cancelada
        public List<VentaItemDto> Detalles { get; set; } = new List<VentaItemDto>();
        public decimal Subtotal => Detalles.Sum(i => i.Subtotal);
        public decimal Total => Subtotal; // Puedes agregar impuestos/descuentos aquí
        public bool Pagado { get; set; } = false;
    }

    public class VentaItemDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; } // Añade esta línea

        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } // Precio de venta
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
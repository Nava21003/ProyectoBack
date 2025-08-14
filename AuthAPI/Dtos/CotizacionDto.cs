using System;
using System.Collections.Generic;

namespace AuthAPI.Dtos
{
    public class CotizacionDto
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteEmail { get; set; }
        public string ClienteTelefono { get; set; }
        public string Notas { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string Estatus { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada, Convertida
        public List<CotizacionItemDto> Detalles { get; set; } = new List<CotizacionItemDto>();
        public decimal Subtotal => Detalles.Sum(i => i.Subtotal);
        public decimal Total => Subtotal; // Aquí podrías agregar impuestos si es necesario
    }

    public class CotizacionItemDto
    {
        public int Id { get; set; }
        public int CotizacionId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } // Precio de venta (precio promedio)
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}

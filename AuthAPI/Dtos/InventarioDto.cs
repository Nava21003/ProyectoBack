namespace AuthAPI.Dtos
{
    public class InventarioDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public string? UnidadMedida { get; set; }
        public int Entradas { get; set; }
        public int Salidas { get; set; }
        public int Existencias { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal PrecioPromedio { get; set; }
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
    }

    public class MovimientoInventarioDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? TipoMovimiento { get; set; } // "ENTRADA" o "SALIDA"
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
        public int? CompraId { get; set; } // Referencia a la compra si es entrada
        public int? VentaId { get; set; } // Referencia a la venta si es salida
    }
}
namespace AuthAPI.Dtos
{
    public class DetalleCompraDto
    {
        public int Id { get; set; }
        public int CompraId { get; set; }
        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class CompraProveedorDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime FechaCompra { get; set; }
        public bool Pagado { get; set; }
        public decimal Total => Detalles?.Sum(d => d.Subtotal) ?? 0;
        public List<DetalleCompraDto>? Detalles { get; set; }
    }
}
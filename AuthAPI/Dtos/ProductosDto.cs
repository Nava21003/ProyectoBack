namespace AuthAPI.Dtos
{
    public class ComprasProveedoresDto
    {
        public int Id { get; set; }

        public int ProveedorId { get; set; }
        public string? NombreProveedor { get; set; }

        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Total => Cantidad * PrecioUnitario;

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        public bool Pagado { get; set; } = false;
    }

    public class ProductoDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? Unidad { get; set; }
        public bool Estatus { get; set; }
    }
}

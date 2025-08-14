namespace AuthAPI.Dtos
{
    public class ProductoProveedorDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? Unidad { get; set; }
    }

    public class ProveedoresDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }

        public List<ProductoProveedorDto>? Productos { get; set; }
    }
}

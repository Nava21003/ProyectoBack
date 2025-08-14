using AuthAPI.Dtos;

public interface IInventarioService
{
    Task<InventarioDto> RegistrarEntradaAsync(MovimientoInventarioDto movimiento);
    Task<InventarioDto> RegistrarSalidaAsync(MovimientoInventarioDto movimiento);
    Task<decimal?> ObtenerPrecioPromedioAsync(int productoId);
    Task<bool> VerificarDisponibilidadAsync(int productoId, int cantidad);
    Task<int?> ObtenerExistenciasAsync(int productoId);
}
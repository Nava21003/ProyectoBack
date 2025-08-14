using AuthAPI.Dtos;
using System.Threading.Tasks;

namespace AuthAPI.Services
{
    public interface IVentaService
    {
        Task<VentaDto> CrearVentaAsync(VentaDto nuevaVenta);
        Task<VentaDto> CompletarVentaAsync(int ventaId);
    }
}
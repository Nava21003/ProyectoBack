// AuthAPI/Controllers/DashboardController.cs
using AuthAPI.Dtos;
using AuthAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public DashboardController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("resumen-negocio")]
        public async Task<ActionResult<BusinessSummaryDto>> GetBusinessSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_GetBusinessSummary", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                    var summary = new BusinessSummaryDto();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Primer resultado: Ventas
                        if (await reader.ReadAsync())
                        {
                            summary.VentasTotales = reader.GetDecimal(0);
                            summary.TotalVentas = reader.GetInt32(1);
                            summary.VentasPagadas = reader.GetDecimal(2);
                            summary.VentasNoPagadas = reader.GetDecimal(3);
                        }

                        // Segundo resultado: Compras
                        await reader.NextResultAsync();
                        if (await reader.ReadAsync())
                        {
                            summary.ComprasTotales = reader.GetDecimal(0);
                            summary.TotalCompras = reader.GetInt32(1);
                            summary.ComprasPagadas = reader.GetDecimal(2);
                            summary.ComprasNoPagadas = reader.GetDecimal(3);
                        }

                        // Tercer resultado: Cotizaciones
                        await reader.NextResultAsync();
                        if (await reader.ReadAsync())
                        {
                            summary.TotalCotizaciones = reader.GetInt32(0);
                            summary.CotizacionesAprobadas = reader.GetInt32(1);
                            summary.CotizacionesConvertidas = reader.GetInt32(2);
                            summary.CotizacionesRechazadas = reader.GetInt32(3);
                            summary.CotizacionesPendientes = reader.GetInt32(4);
                        }

                        // Cuarto resultado: Inventario bajo
                        await reader.NextResultAsync();
                        if (await reader.ReadAsync())
                        {
                            summary.ProductosBajoInventario = reader.GetInt32(0);
                        }
                    }

                    return Ok(summary);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener resumen del negocio: {ex.Message}");
            }
        }

        [HttpGet("ventas-por-periodo")]
        public async Task<ActionResult<List<SalesPeriodDto>>> GetSalesByPeriod(
            [FromQuery] string periodType = "month",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_GetSalesStatisticsByPeriod", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PeriodType", periodType);
                    command.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                    var result = new List<SalesPeriodDto>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new SalesPeriodDto();

                            if (periodType == "day")
                            {
                                item.Period = reader.GetDateTime(0);
                            }
                            else if (periodType == "week")
                            {
                                item.Year = reader.GetInt32(0);
                                item.WeekNumber = reader.GetInt32(1);
                                item.WeekStartDate = reader.GetDateTime(2);
                            }
                            else if (periodType == "month")
                            {
                                item.Year = reader.GetInt32(0);
                                item.MonthNumber = reader.GetInt32(1);
                                item.MonthName = reader.GetString(2);
                            }
                            else if (periodType == "year")
                            {
                                item.Year = reader.GetInt32(0);
                            }

                            item.TotalSales = reader.GetDecimal(reader.GetOrdinal("TotalSales"));
                            item.SalesCount = reader.GetInt32(reader.GetOrdinal("SalesCount"));
                            item.TotalItemsSold = reader.GetInt32(reader.GetOrdinal("TotalItemsSold"));

                            result.Add(item);
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener ventas por período: {ex.Message}");
            }
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<List<TopSellingProductDto>>> GetTopSellingProducts(
            [FromQuery] int topCount = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_GetTopSellingProducts", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@TopCount", topCount);
                    command.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                    var result = new List<TopSellingProductDto>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new TopSellingProductDto
                            {
                                ProductoId = reader.GetInt32(0),
                                ProductoNombre = reader.GetString(1),
                                UnidadMedida = reader.GetString(2),
                                TotalVendido = reader.GetInt32(3),
                                TotalIngresos = reader.GetDecimal(4),
                                PrecioPromedioVenta = reader.GetDecimal(5)
                            });
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener productos más vendidos: {ex.Message}");
            }
        }

        [HttpGet("rentabilidad-productos")]
        public async Task<ActionResult<List<ProductProfitabilityDto>>> GetProductProfitability(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_GetProductProfitability", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                    var result = new List<ProductProfitabilityDto>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new ProductProfitabilityDto
                            {
                                ProductoId = reader.GetInt32(0),
                                ProductoNombre = reader.GetString(1),
                                UnidadMedida = reader.GetString(2),
                                TotalVendido = reader.GetInt32(3),
                                TotalIngresosVenta = reader.GetDecimal(4),
                                PrecioPromedioVenta = reader.GetDecimal(5),
                                PrecioPromedioCompra = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                                GananciaBruta = reader.GetDecimal(7),
                                MargenGananciaPorcentaje = reader.GetDecimal(8)
                            });
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener rentabilidad de productos: {ex.Message}");
            }
        }
    }
}
// AuthAPI/Dtos/DashboardDtos.cs
namespace AuthAPI.Dtos
{
    public class BusinessSummaryDto
    {
        public decimal VentasTotales { get; set; }
        public int TotalVentas { get; set; }
        public decimal VentasPagadas { get; set; }
        public decimal VentasNoPagadas { get; set; }
        public decimal ComprasTotales { get; set; }
        public int TotalCompras { get; set; }
        public decimal ComprasPagadas { get; set; }
        public decimal ComprasNoPagadas { get; set; }
        public int TotalCotizaciones { get; set; }
        public int CotizacionesAprobadas { get; set; }
        public int CotizacionesConvertidas { get; set; }
        public int CotizacionesRechazadas { get; set; }
        public int CotizacionesPendientes { get; set; }
        public int ProductosBajoInventario { get; set; }
    }

    public class SalesPeriodDto
    {
        public DateTime? Period { get; set; } // Para tipo 'day'
        public int? Year { get; set; }
        public int? WeekNumber { get; set; } // Para tipo 'week'
        public DateTime? WeekStartDate { get; set; }
        public int? MonthNumber { get; set; } // Para tipo 'month'
        public string MonthName { get; set; }
        public decimal TotalSales { get; set; }
        public int SalesCount { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class TopSellingProductDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string UnidadMedida { get; set; }
        public int TotalVendido { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PrecioPromedioVenta { get; set; }
    }

    public class ProductProfitabilityDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string UnidadMedida { get; set; }
        public int TotalVendido { get; set; }
        public decimal TotalIngresosVenta { get; set; }
        public decimal PrecioPromedioVenta { get; set; }
        public decimal PrecioPromedioCompra { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal MargenGananciaPorcentaje { get; set; }
    }
}
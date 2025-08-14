using AuthAPI.Dtos;
using AuthAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ProductoDto> Productos { get; set; }
        public DbSet<ProveedoresDto> Proveedores { get; set; }
        public DbSet<ProductoProveedorDto> ProductoProveedores { get; set; }
        public DbSet<CompraProveedorDto> Compras { get; set; }
        public DbSet<DetalleCompraDto> DetallesCompra { get; set; }
        public DbSet<InventarioDto> Inventario { get; set; }
        public DbSet<MovimientoInventarioDto> MovimientosInventario { get; set; }
        public DbSet<CotizacionDto> Cotizaciones { get; set; }
        public DbSet<CotizacionItemDto> ItemsCotizacion { get; set; }
        public DbSet<VentaDto> Ventas { get; set; }
        public DbSet<VentaItemDto> VentaItems { get; set; }
        public DbSet<ComentariosDto> Comentarios { get; set; } = default!;
        public DbSet<RespuestaComentarioDto> RespuestasComentario { get; set; } = default!;
        public DbSet<PreguntaFAQDto> PreguntasFAQ { get; set; } = default!;
        public DbSet<RespuestaFAQDto> RespuestasFAQ { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProveedoresDto>()
                .HasMany(p => p.Productos)
                .WithOne()
                .HasForeignKey("ProveedorId");

            modelBuilder.Entity<CompraProveedorDto>()
                .HasMany(c => c.Detalles)
                .WithOne()
                .HasForeignKey("CompraId");

            modelBuilder.Entity<CotizacionDto>()
                .HasMany(c => c.Detalles)
                .WithOne()
                .HasForeignKey("CotizacionId");

            modelBuilder.Entity<VentaDto>()
                .HasMany(v => v.Detalles)
                .WithOne()
                .HasForeignKey("VentaId");

            modelBuilder.Entity<ComentariosDto>()
               .HasOne(c => c.Respuesta)
               .WithOne(r => r.Comentario)
               .HasForeignKey<RespuestaComentarioDto>(r => r.ComentarioId);

            modelBuilder.Entity<PreguntaFAQDto>()
                .HasOne(p => p.Respuesta)
                .WithOne(r => r.Pregunta)
                .HasForeignKey<RespuestaFAQDto>(r => r.PreguntaId);
        }
    }
}

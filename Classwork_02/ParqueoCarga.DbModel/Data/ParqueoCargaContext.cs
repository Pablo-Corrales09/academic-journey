using Microsoft.EntityFrameworkCore;
using ParqueoCarga.DbModel.Models;

namespace ParqueoCarga.DbModel.Data;

public partial class ParqueoCargaContext : DbContext
{
    public ParqueoCargaContext()
    {
    }

    public ParqueoCargaContext(DbContextOptions<ParqueoCargaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PrqAutomovil> PrqAutomoviles { get; set; }

    public virtual DbSet<PrqIngresoAutomovil> PrqIngresoAutomoviles { get; set; }

    public virtual DbSet<PrqParqueo> PrqParqueos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrqAutomovil>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prq_automoviles", tb =>
            {
                tb.HasComment("Catalog of automobiles that can enter the parking lots.");
            });

            entity.Property(e => e.Id)
                .HasColumnType("int unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Anio)
                .HasColumnType("year")
                .HasColumnName("anio");
            entity.Property(e => e.Color)
                .HasMaxLength(25)
                .HasColumnName("color");
            entity.Property(e => e.Fabricante)
                .HasMaxLength(30)
                .HasColumnName("fabricante");
            entity.Property(e => e.Tipo)
                .HasColumnType("enum('Sedán','4x4','moto')")
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<PrqIngresoAutomovil>(entity =>
        {
            entity.HasKey(e => e.Consecutivo).HasName("PRIMARY");

            entity.ToTable("prq_ingreso_automoviles", tb =>
            {
                tb.HasComment("Transactional log of automobile entries and exits for each parking lot.");
                tb.HasCheckConstraint("chk_prq_ingreso_automoviles_fechas", "(`fecha_salida` is null or `fecha_salida` >= `fecha_entrada`)");
            });

            entity.HasIndex(e => e.FechaEntrada, "idx_prq_ingreso_automoviles_fecha_entrada");
            entity.HasIndex(e => e.IdAutomovil, "idx_prq_ingreso_automoviles_id_automovil");
            entity.HasIndex(e => e.IdParqueo, "idx_prq_ingreso_automoviles_id_parqueo");

            entity.Property(e => e.Consecutivo)
                .HasColumnType("int unsigned")
                .HasColumnName("consecutivo");
            entity.Property(e => e.FechaEntrada)
                .HasColumnType("datetime")
                .HasColumnName("fecha_entrada");
            entity.Property(e => e.FechaSalida)
                .HasColumnType("datetime")
                .HasColumnName("fecha_salida");
            entity.Property(e => e.IdAutomovil)
                .HasColumnType("int unsigned")
                .HasColumnName("id_automovil");
            entity.Property(e => e.IdParqueo)
                .HasColumnType("int unsigned")
                .HasColumnName("id_parqueo");

            entity.HasOne(d => d.IdAutomovilNavigation).WithMany(p => p.PrqIngresoAutomoviles)
                .HasForeignKey(d => d.IdAutomovil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prq_ingreso_automoviles_automovil");

            entity.HasOne(d => d.IdParqueoNavigation).WithMany(p => p.PrqIngresoAutomoviles)
                .HasForeignKey(d => d.IdParqueo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prq_ingreso_automoviles_parqueo");
        });

        modelBuilder.Entity<PrqParqueo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prq_parqueo", tb =>
            {
                tb.HasComment("Catalog of parking lots, including province and hourly rate.");
                tb.HasCheckConstraint("chk_prq_parqueo_precio_hora", "(`precio_hora` >= 0)");
            });

            entity.Property(e => e.Id)
                .HasColumnType("int unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioHora)
                .HasPrecision(10, 2)
                .HasColumnName("precio_hora");
            entity.Property(e => e.Provincia)
                .HasColumnType("enum('San José','Heredia','Alajuela','Cartago','Puntarenas','Limón','Guanacaste')")
                .HasColumnName("provincia");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
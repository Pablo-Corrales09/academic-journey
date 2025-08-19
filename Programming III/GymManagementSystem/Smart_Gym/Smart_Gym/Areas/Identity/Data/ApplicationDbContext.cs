using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Smart_Gym.Models;

namespace Smart_Gym.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

    }

public DbSet<Smart_Gym.Models.Maquina> Maquina { get; set; } = default!;

public DbSet<Smart_Gym.Models.Ejercicio> Ejercicio { get; set; } = default!;

public DbSet<Smart_Gym.Models.Rutina> Rutina { get; set; } = default!;

public DbSet<Smart_Gym.Models.EjercicioRutina> EjercicioRutina { get; set; } = default!;

public DbSet<Smart_Gym.Models.Clase> Clase { get; set; } = default!;

public DbSet<Smart_Gym.Models.ClaseRutina> ClaseRutina { get; set; } = default!;

public DbSet<Smart_Gym.Models.Gimnasio> Gimnasio { get; set; } = default!;

public DbSet<Smart_Gym.Models.UsuarioClase> UsuarioClase { get; set; } = default!;

public DbSet<Smart_Gym.Models.Factura> Factura { get; set; } = default!;

public DbSet<Smart_Gym.Models.Pago> Pago { get; set; } = default!;

public DbSet<Smart_Gym.Models.Membresia> Membresia { get; set; } = default!;
}

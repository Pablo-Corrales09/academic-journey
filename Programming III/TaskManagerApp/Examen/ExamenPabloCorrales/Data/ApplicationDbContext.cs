using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExamenPabloCorrales.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExamenPabloCorrales.Models.Tarea> Tarea { get; set; } = default!;

public DbSet<ExamenPabloCorrales.Models.MetaPrincipal> MetaPrincipal { get; set; } = default!;
    }

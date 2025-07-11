using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Paneles_CFT.Models;

public partial class PanelesWebContext : DbContext
{
    public PanelesWebContext()
    {
    }

    public PanelesWebContext(DbContextOptions<PanelesWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Componente> Componentes { get; set; }

    public virtual DbSet<Instalador> Instaladores { get; set; }

    public virtual DbSet<Normativa> Normativas { get; set; }

    public virtual DbSet<Proyecto> Proyectos { get; set; }

    public virtual DbSet<ProyectosComponentesMap> ProyectosComponentesMaps { get; set; }

    public virtual DbSet<ProyectosNormativasMap> ProyectosNormativasMaps { get; set; }

    public virtual DbSet<Rol> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            // => optionsBuilder.UseSqlServer("server=localhost; database=Paneles_Web; integrated security=true; TrustServerCertificate=Yes;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.RunCliente);

            entity.Property(e => e.RunCliente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DireccionCliente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombresCliente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoCliente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UsernameCliente)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Componente>(entity =>
        {
            entity.HasKey(e => e.CodigoComponente);

            entity.Property(e => e.CodigoComponente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion).IsUnicode(false);
            entity.Property(e => e.MarcaComponente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModeloComponente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombreComponenbte)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoComponente)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Instalador>(entity =>
        {
            entity.HasKey(e => e.RunInstalador).HasName("PK_Instaladoress");

            entity.Property(e => e.RunInstalador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ApellidosInstalador)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContrasenaInstalador)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DireccionInstalador)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombresInstalador)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoInstalador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UsernameInstalador)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolInstaladorNavigation).WithMany(p => p.Instaladores)
                .HasForeignKey(d => d.IdRolInstalador)
                .HasConstraintName("FK__Instalado__IdRol__5629CD9C");
        });

        modelBuilder.Entity<Normativa>(entity =>
        {
            entity.HasKey(e => e.CodigoNormativa);

            entity.Property(e => e.CodigoNormativa)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ComunaNormativa)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DescripcionUrlNormativa)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InstitucionNormativa)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegionNormativa)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.TipoNormativa)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TituloNormativa)
                .HasMaxLength(70)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(e => e.IdProyecto);

            entity.Property(e => e.IdProyecto).ValueGeneratedNever();
            entity.Property(e => e.DireccionProyecto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreProyecto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ResumenDatosProyecto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RunClienteProyecto)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RunInstaladorProyecto)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.RunClienteProyectoNavigation).WithMany(p => p.Proyectos)
                .HasForeignKey(d => d.RunClienteProyecto)
                .HasConstraintName("FK_Proyectos_Clientes");

            entity.HasOne(d => d.RunInstaladorProyectoNavigation).WithMany(p => p.Proyectos)
                .HasForeignKey(d => d.RunInstaladorProyecto)
                .HasConstraintName("FK_Proyectos_Instaladores");
        });

        modelBuilder.Entity<ProyectosComponentesMap>(entity =>
        {
            entity.HasKey(e => e.IdProyectosComponentes);

            entity.ToTable("ProyectosComponentesMap");

            entity.Property(e => e.IdProyectosComponentes).ValueGeneratedNever();
            entity.Property(e => e.CodigoComponentePsCs)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EstadoUsoComponente)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CodigoComponentePsCsNavigation).WithMany(p => p.ProyectosComponentesMaps)
                .HasForeignKey(d => d.CodigoComponentePsCs)
                .HasConstraintName("FK_ProyectosComponentesMap_Componentes");

            entity.HasOne(d => d.IdProyectoPsCsNavigation).WithMany(p => p.ProyectosComponentesMaps)
                .HasForeignKey(d => d.IdProyectoPsCs)
                .HasConstraintName("FK_ProyectosComponentesMap_Proyectos");
        });

        modelBuilder.Entity<ProyectosNormativasMap>(entity =>
        {
            entity.HasKey(e => e.IdProyectosNormativas);

            entity.ToTable("ProyectosNormativasMap");

            entity.Property(e => e.IdProyectosNormativas).ValueGeneratedNever();
            entity.Property(e => e.CodigoNormativaPsNs)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.CodigoNormativaPsNsNavigation).WithMany(p => p.ProyectosNormativasMaps)
                .HasForeignKey(d => d.CodigoNormativaPsNs)
                .HasConstraintName("FK_ProyectosNormativasMap_Normativas");

            entity.HasOne(d => d.IdProyectoPsNsNavigation).WithMany(p => p.ProyectosNormativasMaps)
                .HasForeignKey(d => d.IdProyectoPsNs)
                .HasConstraintName("FK_ProyectosNormativasMap_Proyectos");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.Property(e => e.IdRol).ValueGeneratedNever();
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

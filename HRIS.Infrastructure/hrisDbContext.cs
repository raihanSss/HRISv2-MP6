using System;
using System.Collections.Generic;
using HRIS.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRIS.Infrastructure;

public partial class hrisDbContext : IdentityDbContext<AppUser>
{

    public hrisDbContext(DbContextOptions<hrisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Departments> Departments { get; set; }

    public virtual DbSet<Dependents> Dependents { get; set; }

    public virtual DbSet<Employees> Employees { get; set; }

    public virtual DbSet<Locations> Locations { get; set; }

    public virtual DbSet<Projects> Projects { get; set; }

    public virtual DbSet<Projemp> Projemp { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=PostgreSQLConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Departments>(entity =>
        {
            entity.HasKey(e => e.IdDept).HasName("departments_pkey");

            entity.HasOne(d => d.HeadempNavigation).WithMany(p => p.Departments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_heademp");

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.Departments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_id_location");
        });

        modelBuilder.Entity<Dependents>(entity =>
        {
            entity.HasKey(e => e.IdDependent).HasName("dependents_pkey");

            entity.HasOne(d => d.IdEmpNavigation).WithMany(p => p.Dependents)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_id_emp");
        });

        modelBuilder.Entity<Employees>(entity =>
        {

            entity.HasKey(e => e.IdEmp).HasName("employees_pkey");

            entity.Property(e => e.Lastupdate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdDeptNavigation)
                .WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_id_dept");

            entity.HasOne(e => e.AppUser)
           .WithOne(u => u.Employee)
           .HasForeignKey<AppUser>(u => u.EmployeeId)
           .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Locations>(entity =>
        {
            entity.HasKey(e => e.IdLocation).HasName("locations_pkey");
        });

        modelBuilder.Entity<Projects>(entity =>
        {
            entity.HasKey(e => e.IdProj).HasName("projects_pkey");

            entity.HasOne(d => d.IdDeptNavigation)
                .WithMany(p => p.Projects)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_id_dept");

            entity.HasOne(d => d.IdLocationNavigation)
                .WithMany(p => p.Projects)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_id_location");
        });

        modelBuilder.Entity<Projemp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projemp_pkey");

            entity.HasOne(d => d.IdEmpNavigation).WithMany(p => p.Projemp)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_projemp_id_emp");

            entity.HasOne(d => d.IdProjNavigation).WithMany(p => p.Projemp)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_projemp_id_proj");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

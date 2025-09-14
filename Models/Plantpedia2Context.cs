using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Plantpedia.Models;

public partial class Plantpedia2Context : DbContext
{
    public Plantpedia2Context()
    {
    }

    public Plantpedia2Context(DbContextOptions<Plantpedia2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Climate> Climates { get; set; }

    public virtual DbSet<PlantClass> PlantClasses { get; set; }

    public virtual DbSet<PlantFamily> PlantFamilies { get; set; }

    public virtual DbSet<PlantImg> PlantImgs { get; set; }

    public virtual DbSet<PlantInfo> PlantInfos { get; set; }

    public virtual DbSet<PlantOrder> PlantOrders { get; set; }

    public virtual DbSet<PlantType> PlantTypes { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<SoilType> SoilTypes { get; set; }

    public virtual DbSet<Usage> Usages { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserLoginDatum> UserLoginData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Plantpedia2;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Climate>(entity =>
        {
            entity.ToTable("climate");

            entity.Property(e => e.ClimateId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("climate_id");
            entity.Property(e => e.HumidityRange)
                .HasMaxLength(50)
                .HasColumnName("humidity_range");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RainfallRange)
                .HasMaxLength(50)
                .HasColumnName("rainfall_range");
            entity.Property(e => e.TemperatureRange)
                .HasMaxLength(50)
                .HasColumnName("temperature_range");
        });

        modelBuilder.Entity<PlantClass>(entity =>
        {
            entity.HasKey(e => e.ClassId);

            entity.ToTable("plant_class");

            entity.Property(e => e.ClassId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("class_id");
            entity.Property(e => e.ClassName)
                .HasMaxLength(255)
                .HasColumnName("class_name");
        });

        modelBuilder.Entity<PlantFamily>(entity =>
        {
            entity.HasKey(e => e.FamilyId);

            entity.ToTable("plant_family");

            entity.Property(e => e.FamilyId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("family_id");
            entity.Property(e => e.FamilyName)
                .HasMaxLength(255)
                .HasColumnName("family_name");
        });

        modelBuilder.Entity<PlantImg>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.ToTable("plant_img");

            entity.HasIndex(e => e.PlantId, "IX_plant_img_plant_id");

            entity.Property(e => e.ImageId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("image_id");
            entity.Property(e => e.Caption)
                .HasMaxLength(255)
                .HasColumnName("caption");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.PlantId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("plant_id");

            entity.HasOne(d => d.Plant).WithMany(p => p.PlantImgs).HasForeignKey(d => d.PlantId);
        });

        modelBuilder.Entity<PlantInfo>(entity =>
        {
            entity.HasKey(e => e.PlantId);

            entity.ToTable("plant_info");

            entity.HasIndex(e => e.ClassId, "IX_plant_info_class_id");

            entity.HasIndex(e => e.FamilyId, "IX_plant_info_family_id");

            entity.HasIndex(e => e.OrderId, "IX_plant_info_order_id");

            entity.HasIndex(e => e.PlantTypeId, "IX_plant_info_plant_type_id");

            entity.Property(e => e.PlantId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("plant_id");
            entity.Property(e => e.ClassId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("class_id");
            entity.Property(e => e.CommonName)
                .HasMaxLength(255)
                .HasColumnName("common_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FamilyId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("family_id");
            entity.Property(e => e.OrderId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("order_id");
            entity.Property(e => e.PlantTypeId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("plant_type_id");
            entity.Property(e => e.ScientificName)
                .HasMaxLength(255)
                .HasColumnName("scientific_name");

            entity.HasOne(d => d.Class).WithMany(p => p.PlantInfos).HasForeignKey(d => d.ClassId);

            entity.HasOne(d => d.Family).WithMany(p => p.PlantInfos).HasForeignKey(d => d.FamilyId);

            entity.HasOne(d => d.Order).WithMany(p => p.PlantInfos).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.PlantType).WithMany(p => p.PlantInfos).HasForeignKey(d => d.PlantTypeId);

            entity.HasMany(d => d.Climates).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantClimate",
                    r => r.HasOne<Climate>().WithMany().HasForeignKey("ClimateId"),
                    l => l.HasOne<PlantInfo>().WithMany().HasForeignKey("PlantId"),
                    j =>
                    {
                        j.HasKey("PlantId", "ClimateId");
                        j.ToTable("plant_climate");
                        j.HasIndex(new[] { "ClimateId" }, "IX_plant_climate_climate_id");
                        j.IndexerProperty<string>("PlantId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("plant_id");
                        j.IndexerProperty<string>("ClimateId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("climate_id");
                    });

            entity.HasMany(d => d.Regions).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantRegion",
                    r => r.HasOne<Region>().WithMany().HasForeignKey("RegionId"),
                    l => l.HasOne<PlantInfo>().WithMany().HasForeignKey("PlantId"),
                    j =>
                    {
                        j.HasKey("PlantId", "RegionId");
                        j.ToTable("plant_region");
                        j.HasIndex(new[] { "RegionId" }, "IX_plant_region_region_id");
                        j.IndexerProperty<string>("PlantId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("plant_id");
                        j.IndexerProperty<string>("RegionId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("region_id");
                    });

            entity.HasMany(d => d.SoilTypes).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantSoil",
                    r => r.HasOne<SoilType>().WithMany().HasForeignKey("SoilTypeId"),
                    l => l.HasOne<PlantInfo>().WithMany().HasForeignKey("PlantId"),
                    j =>
                    {
                        j.HasKey("PlantId", "SoilTypeId");
                        j.ToTable("plant_soil");
                        j.HasIndex(new[] { "SoilTypeId" }, "IX_plant_soil_soil_type_id");
                        j.IndexerProperty<string>("PlantId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("plant_id");
                        j.IndexerProperty<string>("SoilTypeId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("soil_type_id");
                    });

            entity.HasMany(d => d.Usages).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantUsage",
                    r => r.HasOne<Usage>().WithMany().HasForeignKey("UsageId"),
                    l => l.HasOne<PlantInfo>().WithMany().HasForeignKey("PlantId"),
                    j =>
                    {
                        j.HasKey("PlantId", "UsageId");
                        j.ToTable("plant_usage");
                        j.HasIndex(new[] { "UsageId" }, "IX_plant_usage_usage_id");
                        j.IndexerProperty<string>("PlantId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("plant_id");
                        j.IndexerProperty<string>("UsageId")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("usage_id");
                    });
        });

        modelBuilder.Entity<PlantOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.ToTable("plant_order");

            entity.Property(e => e.OrderId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("order_id");
            entity.Property(e => e.OrderName)
                .HasMaxLength(255)
                .HasColumnName("order_name");
        });

        modelBuilder.Entity<PlantType>(entity =>
        {
            entity.ToTable("plant_type");

            entity.Property(e => e.PlantTypeId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("plant_type_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.ToTable("region");

            entity.Property(e => e.RegionId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("region_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
        });

        modelBuilder.Entity<SoilType>(entity =>
        {
            entity.ToTable("soil_type");

            entity.Property(e => e.SoilTypeId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("soil_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
        });

        modelBuilder.Entity<Usage>(entity =>
        {
            entity.ToTable("usage");

            entity.Property(e => e.UsageId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("usage_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("user_account");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
        });

        modelBuilder.Entity<UserLoginDatum>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("user_login_data");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(250)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(100)
                .HasColumnName("password_salt");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.User).WithOne(p => p.UserLoginDatum).HasForeignKey<UserLoginDatum>(d => d.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

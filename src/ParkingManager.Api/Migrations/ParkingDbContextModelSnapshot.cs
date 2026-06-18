using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ParkingManager.Api.Services;

#nullable disable

namespace ParkingManager.Api.Migrations
{
    [DbContext(typeof(ParkingDbContext))]
    partial class ParkingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ParkingManager.Api.Domain.ParkingSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("AmountPaid")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("EntryTimeUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExitTimeUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Floor")
                        .HasColumnType("integer");

                    b.Property<bool>("IsContractUser")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("PaidAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("PaidUntilUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SpaceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SpaceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("VehiclePlate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SpaceId");

                    b.HasIndex("Status");

                    b.HasIndex("VehiclePlate");

                    b.ToTable("ParkingSessions");
                });

            modelBuilder.Entity("ParkingManager.Api.Domain.ParkingSpace", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("Floor")
                        .HasColumnType("integer");

                    b.Property<bool>("IsOccupied")
                        .HasColumnType("boolean");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Floor", "Type");

                    b.ToTable("ParkingSpaces");
                });

            modelBuilder.Entity("ParkingManager.Api.Domain.PaymentRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("BaseAmount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("ChargedAmount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("DiscountAmount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("PaidAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PaymentChannel")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PaidAtUtc");

                    b.HasIndex("SessionId");

                    b.ToTable("PaymentRecords");
                });

            modelBuilder.Entity("ParkingManager.Api.Domain.WeatherInterval", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsRainy")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("StartUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("IsRainy", "StartUtc", "EndUtc");

                    b.ToTable("WeatherIntervals");
                });
#pragma warning restore 612, 618
        }
    }
}

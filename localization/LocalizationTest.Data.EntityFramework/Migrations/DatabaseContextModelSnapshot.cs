﻿// <auto-generated />
using System;
using LocalizationTest.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LocalizationTest.Data.EntityFramework.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AsDefault");

                    b.Property<Guid?>("CityId");

                    b.Property<string>("ContentAddress")
                        .HasMaxLength(1000);

                    b.Property<Guid?>("PersonId");

                    b.HasKey("Id");

                    b.HasIndex("CityId");

                    b.HasIndex("PersonId");

                    b.ToTable("Address","Basic");

                    b.HasData(
                        new
                        {
                            Id = new Guid("a0ba6506-1993-44d3-901b-e199f6800d82"),
                            AsDefault = true,
                            CityId = new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"),
                            ContentAddress = "Test Address",
                            PersonId = new Guid("c96c5020-31f9-4dca-a7c9-1cb55722e4b2")
                        });
                });

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PhoneCode");

                    b.HasKey("Id");

                    b.ToTable("City","Basic");

                    b.HasData(
                        new
                        {
                            Id = new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"),
                            PhoneCode = "21"
                        });
                });

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.CityLocalization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CultureId");

                    b.Property<string>("Description");

                    b.Property<Guid>("LocalizableId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("LocalizableId");

                    b.ToTable("CityLocalization","Basic");

                    b.HasData(
                        new
                        {
                            Id = new Guid("e7000da7-a93f-46f8-843e-c7840f7c1ffc"),
                            CultureId = new Guid("ca8a9c53-31c1-458d-9844-504a33309e31"),
                            Description = "Tehran Description",
                            LocalizableId = new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"),
                            Title = "Tehran"
                        },
                        new
                        {
                            Id = new Guid("49e24a56-9a47-4684-9a15-a5891e957c22"),
                            CultureId = new Guid("0c859760-624e-459d-9ddd-fa8b0bcca02b"),
                            Description = "توضیحات تهران",
                            LocalizableId = new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"),
                            Title = "تهران"
                        });
                });

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.Person", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasKey("Id");

                    b.ToTable("Persons");

                    b.HasData(
                        new
                        {
                            Id = new Guid("c96c5020-31f9-4dca-a7c9-1cb55722e4b2"),
                            FirstName = "Soheil",
                            LastName = "Hasankhani"
                        });
                });

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.Address", b =>
                {
                    b.HasOne("LocalizationTest.Data.EntityFramework.Entities.Basic.City", "City")
                        .WithMany()
                        .HasForeignKey("CityId");

                    b.HasOne("LocalizationTest.Data.EntityFramework.Entities.Basic.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("LocalizationTest.Data.EntityFramework.Entities.Basic.CityLocalization", b =>
                {
                    b.HasOne("LocalizationTest.Data.EntityFramework.Entities.Basic.City", "Localizable")
                        .WithMany("Localizations")
                        .HasForeignKey("LocalizableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

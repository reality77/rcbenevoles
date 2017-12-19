﻿// <auto-generated />
using dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace dal.Migrations
{
    [DbContext(typeof(RCBenevoleContext))]
    partial class RCBenevoleContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("dal.models.Benevole", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdresseLigne1")
                        .IsRequired();

                    b.Property<string>("AdresseLigne2");

                    b.Property<string>("AdresseLigne3");

                    b.Property<int>("CentreID");

                    b.Property<string>("CodePostal")
                        .IsRequired();

                    b.Property<string>("Nom")
                        .IsRequired();

                    b.Property<string>("Prenom")
                        .IsRequired();

                    b.Property<string>("Telephone")
                        .IsRequired();

                    b.Property<string>("Ville")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasAlternateKey("Nom", "Prenom")
                        .HasName("UQ_Benevole_NomPrenom");

                    b.HasIndex("CentreID");

                    b.HasIndex("Nom", "Prenom");

                    b.ToTable("Benevoles");
                });

            modelBuilder.Entity("dal.models.Centre", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Adresse")
                        .IsRequired();

                    b.Property<string>("Nom")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("Centres");
                });

            modelBuilder.Entity("dal.models.Frais", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Annee");

                    b.Property<decimal>("TauxKilometrique");

                    b.HasKey("ID");

                    b.HasAlternateKey("Annee")
                        .HasName("UQ_Frais_Annee");

                    b.ToTable("Frais");
                });

            modelBuilder.Entity("dal.models.Pointage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BenevoleID");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<decimal>("Distance");

                    b.Property<int>("NbDemiJournees");

                    b.HasKey("ID");

                    b.HasAlternateKey("BenevoleID", "Date")
                        .HasName("UQ_Pointage_Benevole");

                    b.ToTable("Pointages");
                });

            modelBuilder.Entity("dal.models.Utilisateur", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CentreID");

                    b.Property<string>("Login")
                        .IsRequired();

                    b.Property<string>("Password")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasAlternateKey("Login")
                        .HasName("UQ_Utilisateur_Login");

                    b.HasIndex("CentreID");

                    b.ToTable("Utilisateurs");
                });

            modelBuilder.Entity("dal.models.Benevole", b =>
                {
                    b.HasOne("dal.models.Centre", "Centre")
                        .WithMany()
                        .HasForeignKey("CentreID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("dal.models.Pointage", b =>
                {
                    b.HasOne("dal.models.Benevole", "Benevole")
                        .WithMany()
                        .HasForeignKey("BenevoleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("dal.models.Utilisateur", b =>
                {
                    b.HasOne("dal.models.Centre", "Centre")
                        .WithMany()
                        .HasForeignKey("CentreID");
                });
#pragma warning restore 612, 618
        }
    }
}

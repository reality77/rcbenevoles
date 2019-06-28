using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dal.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benevoles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Nom = table.Column<string>(nullable: false),
                    Prenom = table.Column<string>(nullable: false),
                    Telephone = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benevoles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Annee = table.Column<int>(nullable: false),
                    TauxKilometrique = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sieges",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Adresse = table.Column<string>(nullable: false),
                    Nom = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sieges", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Adresse = table.Column<string>(nullable: false),
                    Nom = table.Column<string>(nullable: false),
                    SiegeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Centres_Sieges_SiegeID",
                        column: x => x.SiegeID,
                        principalTable: "Sieges",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Adresse",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AdresseLigne1 = table.Column<string>(nullable: false),
                    AdresseLigne2 = table.Column<string>(nullable: true),
                    AdresseLigne3 = table.Column<string>(nullable: true),
                    BenevoleID = table.Column<int>(nullable: false),
                    CentreID = table.Column<int>(nullable: false),
                    CodePostal = table.Column<string>(nullable: false),
                    DateChangement = table.Column<DateTime>(nullable: false),
                    DistanceCentre = table.Column<decimal>(nullable: false),
                    IsCurrent = table.Column<bool>(nullable: false),
                    Ville = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Adresse_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adresse_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pointages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BenevoleID = table.Column<int>(nullable: false),
                    CentreID = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    NbDemiJournees = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pointages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Pointages_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pointages_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CentreID = table.Column<int>(nullable: true),
                    Login = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adresse_CentreID",
                table: "Adresse",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_Adresse_BenevoleID_DateChangement",
                table: "Adresse",
                columns: new[] { "BenevoleID", "DateChangement" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Benevoles_Nom_Prenom",
                table: "Benevoles",
                columns: new[] { "Nom", "Prenom" });

            migrationBuilder.CreateIndex(
                name: "IX_Centres_Nom",
                table: "Centres",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Centres_SiegeID",
                table: "Centres",
                column: "SiegeID");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_Annee",
                table: "Frais",
                column: "Annee",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_CentreID",
                table: "Pointages",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_BenevoleID_CentreID_Date",
                table: "Pointages",
                columns: new[] { "BenevoleID", "CentreID", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sieges_Nom",
                table: "Sieges",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_CentreID",
                table: "Utilisateurs",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Login",
                table: "Utilisateurs",
                column: "Login",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adresse");

            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "Pointages");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Benevoles");

            migrationBuilder.DropTable(
                name: "Centres");

            migrationBuilder.DropTable(
                name: "Sieges");
        }
    }
}

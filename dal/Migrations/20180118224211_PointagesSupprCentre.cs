using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dal.Migrations
{
    public partial class PointagesSupprCentre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages");

            migrationBuilder.DropForeignKey(
                name: "FK_Pointages_Centres_CentreID",
                table: "Pointages");

            migrationBuilder.DropIndex(
                name: "IX_Pointages_CentreID",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "CentreID",
                table: "Pointages");

            migrationBuilder.AlterColumn<int>(
                name: "AdresseID",
                table: "Pointages",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages",
                column: "AdresseID",
                principalTable: "Adresse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages");

            migrationBuilder.AlterColumn<int>(
                name: "AdresseID",
                table: "Pointages",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "CentreID",
                table: "Pointages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_CentreID",
                table: "Pointages",
                column: "CentreID");

            migrationBuilder.AddForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages",
                column: "AdresseID",
                principalTable: "Adresse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pointages_Centres_CentreID",
                table: "Pointages",
                column: "CentreID",
                principalTable: "Centres",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

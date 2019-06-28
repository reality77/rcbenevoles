using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dal.Migrations
{
    public partial class PointagesAjoutAdresse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pointages_BenevoleID_CentreID_Date",
                table: "Pointages");

            migrationBuilder.AddColumn<int>(
                name: "AdresseID",
                table: "Pointages",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_AdresseID",
                table: "Pointages",
                column: "AdresseID");

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_BenevoleID_Date",
                table: "Pointages",
                columns: new[] { "BenevoleID", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages",
                column: "AdresseID",
                principalTable: "Adresse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"UPDATE ""Pointages"" AS p SET ""AdresseID"" = 
	(SELECT ""ID"" FROM ""Adresse"" AS a WHERE a.""BenevoleID"" = p.""BenevoleID"" AND a.""DateChangement"" <= p.""Date"" ORDER BY a.""DateChangement"" DESC LIMIT 1) ;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pointages_Adresse_AdresseID",
                table: "Pointages");

            migrationBuilder.DropIndex(
                name: "IX_Pointages_AdresseID",
                table: "Pointages");

            migrationBuilder.DropIndex(
                name: "IX_Pointages_BenevoleID_Date",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "AdresseID",
                table: "Pointages");

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_BenevoleID_CentreID_Date",
                table: "Pointages",
                columns: new[] { "BenevoleID", "CentreID", "Date" },
                unique: true);
        }
    }
}

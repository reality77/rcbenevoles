using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dal.Migrations
{
    public partial class ChevauxFiscaux : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NbChevauxFiscauxVoiture",
                table: "Benevoles",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NbChevauxFiscauxVoiture",
                table: "Benevoles");
        }
    }
}

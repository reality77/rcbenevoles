using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class BaremeFiscal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaremeFiscalLignes",
                columns: table => new
                {
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    NbChevaux = table.Column<int>(type: "integer", nullable: false),
                    LimiteKm = table.Column<int>(type: "integer", nullable: false),
                    Coef = table.Column<decimal>(type: "numeric", nullable: false),
                    Ajout = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaremeFiscalLignes", x => new { x.Annee, x.NbChevaux, x.LimiteKm });
                });

            InsertBareme(migrationBuilder, 2022, 3, 5000, 0.502m, 0m);
            InsertBareme(migrationBuilder, 2022, 4, 5000, 0.575m, 0m);
            InsertBareme(migrationBuilder, 2022, 5, 5000, 0.603m, 0m);
            InsertBareme(migrationBuilder, 2022, 6, 5000, 0.631m, 0m);
            InsertBareme(migrationBuilder, 2022, 7, 5000, 0.661m, 0m);

            InsertBareme(migrationBuilder, 2022, 3, 20000, 0.300m, 1007m);
            InsertBareme(migrationBuilder, 2022, 4, 20000, 0.323m, 1262m);
            InsertBareme(migrationBuilder, 2022, 5, 20000, 0.339m, 1320m);
            InsertBareme(migrationBuilder, 2022, 6, 20000, 0.355m, 1382m);
            InsertBareme(migrationBuilder, 2022, 7, 20000, 0.374m, 1435m);
        }

        private void InsertBareme(MigrationBuilder migrationBuilder, int annee, int nbChevaux, int limiteKm, decimal coef, decimal ajout)
        {
            migrationBuilder.InsertData("BaremeFiscalLignes", 
                new string[] { "Annee", "NbChevaux", "LimiteKm", "Coef", "Ajout" },
                new object[] { annee, nbChevaux, limiteKm, coef, ajout });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaremeFiscalLignes");
        }
    }
}

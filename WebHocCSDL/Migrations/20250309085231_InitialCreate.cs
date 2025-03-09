using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHocCSDL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConceptualDesign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ERD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogicalDesign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhysicalDesign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelationshipsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}

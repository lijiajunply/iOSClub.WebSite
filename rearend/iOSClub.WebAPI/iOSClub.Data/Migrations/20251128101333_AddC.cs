using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOSClub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. First, create the Categories table
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            // 2. Update existing Category values to NULL before renaming
            // This prevents foreign key constraint violations
            migrationBuilder.Sql("UPDATE \"Articles\" SET \"Category\" = NULL");
            
            // 3. Rename the column and add other changes
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Articles",
                newName: "CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "ArticleOrder",
                table: "Articles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Categories_CategoryId",
                table: "Articles",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Categories_CategoryId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ArticleOrder",
                table: "Articles");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Articles",
                newName: "Category");
        }
    }
}

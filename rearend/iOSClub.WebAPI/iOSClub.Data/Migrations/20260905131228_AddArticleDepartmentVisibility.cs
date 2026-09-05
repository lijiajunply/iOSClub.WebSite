using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOSClub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleDepartmentVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisibleToDepartment",
                table: "Articles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_VisibleToDepartment",
                table: "Articles",
                column: "VisibleToDepartment");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Departments_VisibleToDepartment",
                table: "Articles",
                column: "VisibleToDepartment",
                principalTable: "Departments",
                principalColumn: "Name",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Departments_VisibleToDepartment",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_VisibleToDepartment",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "VisibleToDepartment",
                table: "Articles");
        }
    }
}

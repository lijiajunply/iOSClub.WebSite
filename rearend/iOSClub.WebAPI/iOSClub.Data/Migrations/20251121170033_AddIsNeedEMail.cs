using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOSClub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNeedEMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNeedEMail",
                table: "ClientApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNeedEMail",
                table: "ClientApplications");
        }
    }
}

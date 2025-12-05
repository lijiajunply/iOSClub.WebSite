using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOSClub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Students_Academy",
                table: "Students",
                column: "Academy");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassName",
                table: "Students",
                column: "ClassName");

            migrationBuilder.CreateIndex(
                name: "IX_Students_JoinTime",
                table: "Students",
                column: "JoinTime");

            migrationBuilder.CreateIndex(
                name: "IX_Students_PhoneNum",
                table: "Students",
                column: "PhoneNum");

            migrationBuilder.CreateIndex(
                name: "IX_Students_PoliticalLandscape",
                table: "Students",
                column: "PoliticalLandscape");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserName",
                table: "Students",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Identity",
                table: "Staffs",
                column: "Identity");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Academy",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ClassName",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_JoinTime",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_PhoneNum",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_PoliticalLandscape",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserName",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_Identity",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs");
        }
    }
}

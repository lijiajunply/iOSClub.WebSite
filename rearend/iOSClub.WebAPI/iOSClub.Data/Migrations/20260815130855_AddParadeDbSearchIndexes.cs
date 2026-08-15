using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOSClub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParadeDbSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 数据模型从 DataModels 迁移到 DataObjects 时，多对多连接表被重命名。
            // 使用 RenameTable/RenameIndex 保留原有数据（而非脚手架默认的 Drop+Create）。
            migrationBuilder.RenameTable(
                name: "ProjectModelStaffModel",
                newName: "ProjectDOStaffDO");

            migrationBuilder.RenameTable(
                name: "StaffModelTaskModel",
                newName: "StaffDOTaskDO");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectModelStaffModel_StaffsUserId",
                newName: "IX_ProjectDOStaffDO_StaffsUserId",
                table: "ProjectDOStaffDO");

            migrationBuilder.RenameIndex(
                name: "IX_StaffModelTaskModel_UsersUserId",
                newName: "IX_StaffDOTaskDO_UsersUserId",
                table: "StaffDOTaskDO");

            // UserId 已为主键（天然唯一），移除冗余的唯一索引，为 ParadeDB 索引让位
            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_search", ",,");

            migrationBuilder.CreateIndex(
                name: "students_search_idx",
                table: "Students",
                column: "UserId")
                .Annotation("ParadeDB:IndexFields", new[] { "\"UserId\"", "(\"UserName\"::pdb.jieba)", "(\"ClassName\"::pdb.jieba)", "(\"Academy\"::pdb.jieba)" })
                .Annotation("ParadeDB:IndexKeyField", "UserId")
                .Annotation("ParadeDB:IndexSearchTokenizer", "jieba");

            migrationBuilder.CreateIndex(
                name: "articles_search_idx",
                table: "Articles",
                column: "Path")
                .Annotation("ParadeDB:IndexFields", new[] { "\"Path\"", "(\"Title\"::pdb.jieba)", "(\"Content\"::pdb.jieba)" })
                .Annotation("ParadeDB:IndexKeyField", "Path")
                .Annotation("ParadeDB:IndexSearchTokenizer", "jieba");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "articles_search_idx",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "students_search_idx",
                table: "Students");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_search", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDOStaffDO_StaffsUserId",
                newName: "IX_ProjectModelStaffModel_StaffsUserId",
                table: "ProjectDOStaffDO");

            migrationBuilder.RenameIndex(
                name: "IX_StaffDOTaskDO_UsersUserId",
                newName: "IX_StaffModelTaskModel_UsersUserId",
                table: "StaffDOTaskDO");

            migrationBuilder.RenameTable(
                name: "ProjectDOStaffDO",
                newName: "ProjectModelStaffModel");

            migrationBuilder.RenameTable(
                name: "StaffDOTaskDO",
                newName: "StaffModelTaskModel");
        }
    }
}

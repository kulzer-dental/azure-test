using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KDC.Main.Data.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddRequiredStoreCodeToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreCode",
                schema: "Application",
                table: "Users",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreCode",
                schema: "Application",
                table: "Users");
        }
    }
}

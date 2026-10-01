using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsBlockedUseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "BillPays");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BillPays",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillPays");

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "BillPays",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

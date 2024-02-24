using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomsNest.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class COD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayId",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayId",
                table: "OrderHeader");
        }
    }
}

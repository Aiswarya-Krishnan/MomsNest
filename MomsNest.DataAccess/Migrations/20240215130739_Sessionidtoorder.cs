﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomsNest.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Sessionidtoorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeader");
        }
    }
}

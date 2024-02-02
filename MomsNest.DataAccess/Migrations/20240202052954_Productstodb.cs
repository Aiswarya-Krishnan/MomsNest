using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MomsNest.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Productstodb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Footwear" },
                    { 2, 2, "Toys" },
                    { 3, 3, "Hygiene" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Brand", "CategoryID", "Color", "Description", "Material", "Price", "ProductName", "StockQuantity", "Weight" },
                values: new object[,]
                {
                    { 1, "SnuggleCo", 2, "Brown", "Soft and cuddly teddy bear for newborns", "Plush", 19.99m, "Cuddly Teddy Bear", 50, 0.5m },
                    { 2, "TinyTots", 2, "Pastel", "Adorable onesie set for infants", "Cotton", 29.99m, "Baby Onesie Set", 30, 0.3m },
                    { 3, "PlayfulHands", 2, "Multicolor", "Colorful mobile with music for crib entertainment", "Plastic", 39.99m, "Interactive Baby Mobile", 20, 0.8m },
                    { 4, "MunchieMunch", 2, "Blue", "Complete feeding set for infants", "BPA-Free Plastic", 49.99m, "Baby Feeding Set", 25, 0.6m },
                    { 5, "LearnSmart", 2, "Yellow", "Interactive and colorful book for early learning", "Cardboard", 14.99m, "Educational Baby Book", 40, 0.2m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

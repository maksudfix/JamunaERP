using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JamunaERP.Infrastructure.Migrations
{
    public partial class LinkSaleItemsToStockItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockItemId",
                table: "SaleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_StockItemId",
                table: "SaleItems",
                column: "StockItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_StockItems_StockItemId",
                table: "SaleItems",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_StockItems_StockItemId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_StockItemId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "StockItemId",
                table: "SaleItems");
        }
    }
}
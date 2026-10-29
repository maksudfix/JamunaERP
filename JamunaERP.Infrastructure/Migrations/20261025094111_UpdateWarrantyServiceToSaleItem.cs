using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JamunaERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarrantyServiceToSaleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyServices_Customers_CustomerId",
                table: "WarrantyServices");

            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyServices_Products_ProductId",
                table: "WarrantyServices");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyServices_CustomerId",
                table: "WarrantyServices");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "WarrantyServices");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "WarrantyServices");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "WarrantyServices",
                newName: "SaleItemId");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyServices_ProductId",
                table: "WarrantyServices",
                newName: "IX_WarrantyServices_SaleItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyServices_SaleItems_SaleItemId",
                table: "WarrantyServices",
                column: "SaleItemId",
                principalTable: "SaleItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyServices_SaleItems_SaleItemId",
                table: "WarrantyServices");

            migrationBuilder.RenameColumn(
                name: "SaleItemId",
                table: "WarrantyServices",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyServices_SaleItemId",
                table: "WarrantyServices",
                newName: "IX_WarrantyServices_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "WarrantyServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "WarrantyServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyServices_CustomerId",
                table: "WarrantyServices",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyServices_Customers_CustomerId",
                table: "WarrantyServices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyServices_Products_ProductId",
                table: "WarrantyServices",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

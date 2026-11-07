using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JamunaERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStep2AndStep3ErpFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DownPayment",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorAddress",
                table: "Sales",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorName",
                table: "Sales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorNid",
                table: "Sales",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorPhone",
                table: "Sales",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCharge",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "SaleItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowInstallment",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Jamuna");

            migrationBuilder.AddColumn<int>(
                name: "DefaultDealerId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "MobileBankingProvider",
                table: "Payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollectionNote",
                table: "InstallmentPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFollowUpDate",
                table: "InstallmentPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPromiseDate",
                table: "InstallmentPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "Customers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NidNumber",
                table: "Customers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultDealerId",
                table: "Products",
                column: "DefaultDealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Date",
                table: "Payments",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NidNumber",
                table: "Customers",
                column: "NidNumber",
                filter: "[NidNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Dealers_DefaultDealerId",
                table: "Products",
                column: "DefaultDealerId",
                principalTable: "Dealers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Dealers_DefaultDealerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Brand",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultDealerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Date",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NidNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DownPayment",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "GuarantorAddress",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "GuarantorName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "GuarantorNid",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "GuarantorPhone",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ServiceCharge",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "AllowInstallment",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultDealerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MobileBankingProvider",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CollectionNote",
                table: "InstallmentPlans");

            migrationBuilder.DropColumn(
                name: "LastFollowUpDate",
                table: "InstallmentPlans");

            migrationBuilder.DropColumn(
                name: "NextPromiseDate",
                table: "InstallmentPlans");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NidNumber",
                table: "Customers");
        }
    }
}

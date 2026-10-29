using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JamunaERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_DealerId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_InstallmentPlans_SaleId",
                table: "InstallmentPlans");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePayments_EmployeeId",
                table: "EmployeePayments");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WarrantyServices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "WarrantyServices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "StockItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StockItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StockItems",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "StockItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "Sales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Sales",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Sales",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SaleItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SaleItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DealerInvoiceNo",
                table: "PurchaseOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReceived",
                table: "PurchaseOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseOrderItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PurchaseOrderItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InstallmentPlans",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InstallmentPlans",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeePayments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "EmployeePayments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Dealers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Dealers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Customers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_SerialNumber",
                table: "StockItems",
                column: "SerialNumber",
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_InvoiceNo",
                table: "Sales",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_DealerId_DealerInvoiceNo",
                table: "PurchaseOrders",
                columns: new[] { "DealerId", "DealerInvoiceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPlans_SaleId_InstallmentNumber",
                table: "InstallmentPlans",
                columns: new[] { "SaleId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayments_EmployeeId_PaymentMonth_PaymentYear",
                table: "EmployeePayments",
                columns: new[] { "EmployeeId", "PaymentMonth", "PaymentYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_SerialNumber",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Sales_InvoiceNo",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_DealerId_DealerInvoiceNo",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_InstallmentPlans_SaleId_InstallmentNumber",
                table: "InstallmentPlans");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePayments_EmployeeId_PaymentMonth_PaymentYear",
                table: "EmployeePayments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WarrantyServices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WarrantyServices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "IsReceived",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InstallmentPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InstallmentPlans");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeePayments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "EmployeePayments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "StockItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DealerInvoiceNo",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_DealerId",
                table: "PurchaseOrders",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPlans_SaleId",
                table: "InstallmentPlans",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayments_EmployeeId",
                table: "EmployeePayments",
                column: "EmployeeId");
        }
    }
}

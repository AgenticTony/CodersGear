using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodersGear.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPrintifyApiSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrintifyProduct",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyProductId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyShopId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyVariantData",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyOrderId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SentToPrintify",
                table: "OrderHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentToPrintifyAt",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "IsPrintifyProduct", "LastSyncedAt", "PrintifyProductId", "PrintifyShopId", "PrintifyVariantData" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "IsPrintifyProduct", "LastSyncedAt", "PrintifyProductId", "PrintifyShopId", "PrintifyVariantData" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "IsPrintifyProduct", "LastSyncedAt", "PrintifyProductId", "PrintifyShopId", "PrintifyVariantData" },
                values: new object[] { false, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "IsPrintifyProduct", "LastSyncedAt", "PrintifyProductId", "PrintifyShopId", "PrintifyVariantData" },
                values: new object[] { false, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrintifyProduct",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PrintifyProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PrintifyShopId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PrintifyVariantData",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PrintifyOrderId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "SentToPrintify",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "SentToPrintifyAt",
                table: "OrderHeaders");
        }
    }
}

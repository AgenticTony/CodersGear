using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodersGear.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantFieldsToCartAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                table: "ShoppingCarts");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ShoppingCarts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyVariantId",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintifyVariantId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                table: "ShoppingCarts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "PrintifyVariantId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PrintifyVariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "OrderDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ShoppingCarts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                table: "ShoppingCarts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

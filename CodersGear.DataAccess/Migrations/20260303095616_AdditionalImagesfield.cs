using Microsoft.EntityFrameworkCore.Migrations;

using System;
using System.Collections.Generic;
using CodersGear.DataAccess.Data;
using CodersGear.Models;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using System.IO;

namespace CodersGear.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalImagesfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalImages",
-                table: "Products"
-                type: "List<string>()",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "AdditionalImages",
                type: "List<string>()",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "IsPrintifyProduct",
                type: "bool",
                nullable: false);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessiTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AIfuckup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Audits",
                newName: "ErrorMessage");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Audits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PassCount",
                table: "Audits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Audits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViolationCount",
                table: "Audits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "PassCount",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "ViolationCount",
                table: "Audits");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "Audits",
                newName: "Notes");
        }
    }
}

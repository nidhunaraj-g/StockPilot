using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDeactivationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAtUtc",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminDeactivated",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsAdminDeactivated",
                table: "Customers");
        }
    }
}

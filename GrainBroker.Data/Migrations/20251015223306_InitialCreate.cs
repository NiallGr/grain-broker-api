using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrainBroker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrainOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerLocation = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RequestedTons = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SuppliedTons = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FulfilledById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FulfilledByLocation = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DeliveryCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainOrders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrainOrders_PurchaseOrderId",
                table: "GrainOrders",
                column: "PurchaseOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrainOrders");
        }
    }
}

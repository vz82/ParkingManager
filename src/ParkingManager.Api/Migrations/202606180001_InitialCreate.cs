using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManager.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingSpaces",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsOccupied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePlate = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    IsContractUser = table.Column<bool>(type: "boolean", nullable: false),
                    SpaceId = table.Column<string>(type: "text", nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    SpaceType = table.Column<string>(type: "text", nullable: false),
                    EntryTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExitTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ChargedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentChannel = table.Column<string>(type: "text", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherIntervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRainy = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherIntervals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_SpaceId",
                table: "ParkingSessions",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_Status",
                table: "ParkingSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_VehiclePlate",
                table: "ParkingSessions",
                column: "VehiclePlate");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpaces_Floor_Type",
                table: "ParkingSpaces",
                columns: new[] { "Floor", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_PaidAtUtc",
                table: "PaymentRecords",
                column: "PaidAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_SessionId",
                table: "PaymentRecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherIntervals_IsRainy_StartUtc_EndUtc",
                table: "WeatherIntervals",
                columns: new[] { "IsRainy", "StartUtc", "EndUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "ParkingSpaces");

            migrationBuilder.DropTable(
                name: "PaymentRecords");

            migrationBuilder.DropTable(
                name: "WeatherIntervals");
        }
    }
}

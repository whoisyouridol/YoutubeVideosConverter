using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoutubeVideoConverter.Infrastructure.SQL.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRequestResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestSucceeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConvertedSuccessfully = table.Column<bool>(type: "bit", nullable: false),
                    ConversionTime = table.Column<TimeSpan>(type: "time", nullable: false, defaultValue: new TimeSpan(0, 0, 0, 0, 0)),
                    ConvertTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversionDestination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversionLogs_UserRequestResponses_UserRequestId",
                        column: x => x.UserRequestId,
                        principalTable: "UserRequestResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversionLogs_UserRequestId",
                table: "ConversionLogs",
                column: "UserRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionLogs");

            migrationBuilder.DropTable(
                name: "UserRequestResponses");
        }
    }
}

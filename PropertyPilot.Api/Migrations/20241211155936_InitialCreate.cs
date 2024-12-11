using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyPilot.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "properties_list",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_name = table.Column<string>(type: "text", nullable: false),
                    emirate = table.Column<string>(type: "text", nullable: true),
                    property_type = table.Column<string>(type: "text", nullable: false),
                    units_count = table.Column<int>(type: "integer", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    date_archived = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties_list", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "properties_list");
        }
    }
}

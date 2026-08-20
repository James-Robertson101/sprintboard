using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SprintBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class MapRowVersionToXmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProjectMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProjectMembers",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
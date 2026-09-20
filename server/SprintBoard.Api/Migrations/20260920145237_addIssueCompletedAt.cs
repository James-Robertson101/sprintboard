using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SprintBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class addIssueCompletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Issues",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Issues");
        }
    }
}

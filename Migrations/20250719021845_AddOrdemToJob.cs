﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saigor.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdemToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Jobs");
        }
    }
}

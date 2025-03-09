﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHocCSDL.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelationshipsJson",
                table: "Designs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationshipsJson",
                table: "Designs");
        }
    }
}

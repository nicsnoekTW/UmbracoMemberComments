using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberComments.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndModeratorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedUtc",
                table: "membercomments_Comment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeratorId",
                table: "membercomments_Comment",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "membercomments_Comment");

            migrationBuilder.DropColumn(
                name: "ModeratorId",
                table: "membercomments_Comment");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberComments.Migrations
{
    /// <inheritdoc />
    public partial class InitialComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "membercomments_Comment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContentKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EditedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membercomments_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_membercomments_Comment_membercomments_Comment_ParentId",
                        column: x => x.ParentId,
                        principalTable: "membercomments_Comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_membercomments_Comment_ContentKey",
                table: "membercomments_Comment",
                column: "ContentKey");

            migrationBuilder.CreateIndex(
                name: "IX_membercomments_Comment_ParentId",
                table: "membercomments_Comment",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membercomments_Comment");
        }
    }
}

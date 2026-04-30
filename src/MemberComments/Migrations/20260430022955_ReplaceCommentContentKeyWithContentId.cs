using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberComments.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCommentContentKeyWithContentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_membercomments_Comment_ContentKey",
                table: "membercomments_Comment");

            migrationBuilder.DropColumn(
                name: "ContentKey",
                table: "membercomments_Comment");

            migrationBuilder.AddColumn<int>(
                name: "ContentId",
                table: "membercomments_Comment",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_membercomments_Comment_ContentId",
                table: "membercomments_Comment",
                column: "ContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_membercomments_Comment_ContentId",
                table: "membercomments_Comment");

            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "membercomments_Comment");

            migrationBuilder.AddColumn<Guid>(
                name: "ContentKey",
                table: "membercomments_Comment",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_membercomments_Comment_ContentKey",
                table: "membercomments_Comment",
                column: "ContentKey");
        }
    }
}

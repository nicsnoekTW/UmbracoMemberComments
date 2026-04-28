using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberComments.Migrations;

/// <inheritdoc />
public partial class AddCommentSubjectAndRichText : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Subject",
            table: "membercomments_Comment",
            type: "TEXT",
            maxLength: 256,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<string>(
            name: "Text",
            table: "membercomments_Comment",
            type: "TEXT",
            maxLength: 100000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 4000,
            oldNullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Text",
            table: "membercomments_Comment",
            type: "TEXT",
            maxLength: 4000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100000,
            oldNullable: false);

        migrationBuilder.DropColumn(
            name: "Subject",
            table: "membercomments_Comment");
    }
}

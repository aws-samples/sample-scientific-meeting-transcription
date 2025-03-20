using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class prompt_response_change2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_meeting_documents_meetings_meeting_id",
                table: "meeting_documents");

            migrationBuilder.AddForeignKey(
                name: "FK_meeting_documents_meetings_meeting_id",
                table: "meeting_documents",
                column: "meeting_id",
                principalTable: "meetings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_meeting_documents_meetings_meeting_id",
                table: "meeting_documents");

            migrationBuilder.AddForeignKey(
                name: "FK_meeting_documents_meetings_meeting_id",
                table: "meeting_documents",
                column: "meeting_id",
                principalTable: "meetings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class prompt_response_change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_meeting_prompt_responses_prompts_PromptId",
                table: "meeting_prompt_responses");

            migrationBuilder.DropForeignKey(
                name: "FK_meetings_custom_models_custom_model_id",
                table: "meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_meetings_custom_vocabularies_custom_vocabulary_id",
                table: "meetings");

            migrationBuilder.RenameColumn(
                name: "PromptId",
                table: "meeting_prompt_responses",
                newName: "PromptDatabaseTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_meeting_prompt_responses_PromptId",
                table: "meeting_prompt_responses",
                newName: "IX_meeting_prompt_responses_PromptDatabaseTypeId");

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "meeting_prompt_responses",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_meeting_prompt_responses_prompts_PromptDatabaseTypeId",
                table: "meeting_prompt_responses",
                column: "PromptDatabaseTypeId",
                principalTable: "prompts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_meetings_custom_models_custom_model_id",
                table: "meetings",
                column: "custom_model_id",
                principalTable: "custom_models",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_meetings_custom_vocabularies_custom_vocabulary_id",
                table: "meetings",
                column: "custom_vocabulary_id",
                principalTable: "custom_vocabularies",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_meeting_prompt_responses_prompts_PromptDatabaseTypeId",
                table: "meeting_prompt_responses");

            migrationBuilder.DropForeignKey(
                name: "FK_meetings_custom_models_custom_model_id",
                table: "meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_meetings_custom_vocabularies_custom_vocabulary_id",
                table: "meetings");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "meeting_prompt_responses");

            migrationBuilder.RenameColumn(
                name: "PromptDatabaseTypeId",
                table: "meeting_prompt_responses",
                newName: "PromptId");

            migrationBuilder.RenameIndex(
                name: "IX_meeting_prompt_responses_PromptDatabaseTypeId",
                table: "meeting_prompt_responses",
                newName: "IX_meeting_prompt_responses_PromptId");

            migrationBuilder.AddForeignKey(
                name: "FK_meeting_prompt_responses_prompts_PromptId",
                table: "meeting_prompt_responses",
                column: "PromptId",
                principalTable: "prompts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_meetings_custom_models_custom_model_id",
                table: "meetings",
                column: "custom_model_id",
                principalTable: "custom_models",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_meetings_custom_vocabularies_custom_vocabulary_id",
                table: "meetings",
                column: "custom_vocabulary_id",
                principalTable: "custom_vocabularies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    team = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    idp_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    modelname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    language_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    data_access_role_arn = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: true),
                    transcribed_data_s3_uri = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    training_data_s3_uri = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    training_data_s3_uri_folder = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    aws_model_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    model_setup_progress = table.Column<int>(type: "integer", maxLength: 3, nullable: true, defaultValue: 0),
                    model_setup_status = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    presigned_url = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    presigned_url_expire = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    state_machine_execution_arn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    transcribe_base_model_name = table.Column<int>(type: "integer", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_models", x => x.id);
                    table.ForeignKey(
                        name: "FK_custom_models_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "custom_vocabularies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    current_step = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    publish_error = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    language_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_vocabularies", x => x.id);
                    table.ForeignKey(
                        name: "FK_custom_vocabularies_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptsets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    prompt_set_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    bedrock_model_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 3, nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptsets", x => x.id);
                    table.ForeignKey(
                        name: "FK_promptsets_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_phrases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomVocabularyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_as = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_phrases", x => x.id);
                    table.ForeignKey(
                        name: "FK_vocabulary_phrases_custom_vocabularies_CustomVocabularyId",
                        column: x => x.CustomVocabularyId,
                        principalTable: "custom_vocabularies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vocabulary_phrases_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meetings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PromptSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_vocabulary_id = table.Column<Guid>(type: "uuid", nullable: true),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    current_step = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pre_signe_url = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    s3_recording_full_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    s3_transcribed_fullpath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TranscribedPreSignedUrl = table.Column<string>(type: "text", nullable: true),
                    state_machine_execution_arn = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    s3_output_bucket_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    s3_output_key_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    transcribe_error = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    meeting_notes = table.Column<string>(type: "text", nullable: true),
                    IncludeInModelTraining = table.Column<bool>(type: "boolean", nullable: true),
                    MeetingAnalyticsPayload = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meetings", x => x.id);
                    table.ForeignKey(
                        name: "FK_meetings_custom_models_custom_model_id",
                        column: x => x.custom_model_id,
                        principalTable: "custom_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_meetings_custom_vocabularies_custom_vocabulary_id",
                        column: x => x.custom_vocabulary_id,
                        principalTable: "custom_vocabularies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_meetings_promptsets_PromptSetId",
                        column: x => x.PromptSetId,
                        principalTable: "promptsets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_meetings_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingPromptResponseId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    prompt = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 3, nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompts", x => x.id);
                    table.ForeignKey(
                        name: "FK_prompts_promptsets_prompt_set_id",
                        column: x => x.prompt_set_id,
                        principalTable: "promptsets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prompts_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "meeting_prompt_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_response = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_prompt_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_meeting_prompt_responses_meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meeting_prompt_responses_prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "prompts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_meeting_prompt_responses_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_custom_models_TeamId",
                table: "custom_models",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_custom_vocabularies_TeamId",
                table: "custom_vocabularies",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_prompt_responses_MeetingId",
                table: "meeting_prompt_responses",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_prompt_responses_PromptId",
                table: "meeting_prompt_responses",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_prompt_responses_TeamId",
                table: "meeting_prompt_responses",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_custom_model_id",
                table: "meetings",
                column: "custom_model_id");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_custom_vocabulary_id",
                table: "meetings",
                column: "custom_vocabulary_id");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_PromptSetId",
                table: "meetings",
                column: "PromptSetId");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_team_id",
                table: "meetings",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_prompt_set_id",
                table: "prompts",
                column: "prompt_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_TeamId",
                table: "prompts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_promptsets_TeamId",
                table: "promptsets",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_phrases_CustomVocabularyId",
                table: "vocabulary_phrases",
                column: "CustomVocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_phrases_TeamId",
                table: "vocabulary_phrases",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meeting_prompt_responses");

            migrationBuilder.DropTable(
                name: "vocabulary_phrases");

            migrationBuilder.DropTable(
                name: "meetings");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "custom_models");

            migrationBuilder.DropTable(
                name: "custom_vocabularies");

            migrationBuilder.DropTable(
                name: "promptsets");

            migrationBuilder.DropTable(
                name: "teams");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class promptset_generate_prompts3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prompts_promptsets_prompt_set_id",
                table: "prompts");

            migrationBuilder.AddForeignKey(
                name: "FK_prompts_promptsets_prompt_set_id",
                table: "prompts",
                column: "prompt_set_id",
                principalTable: "promptsets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prompts_promptsets_prompt_set_id",
                table: "prompts");

            migrationBuilder.AddForeignKey(
                name: "FK_prompts_promptsets_prompt_set_id",
                table: "prompts",
                column: "prompt_set_id",
                principalTable: "promptsets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

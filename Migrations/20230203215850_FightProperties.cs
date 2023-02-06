using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnetrpg.Migrations
{
    public partial class FightProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterSkill_Skills_SkillId",
                table: "CharacterSkill");

            migrationBuilder.RenameColumn(
                name: "SkillId",
                table: "CharacterSkill",
                newName: "SkillsId");

            migrationBuilder.RenameIndex(
                name: "IX_CharacterSkill_SkillId",
                table: "CharacterSkill",
                newName: "IX_CharacterSkill_SkillsId");

            migrationBuilder.AddColumn<int>(
                name: "Defeats",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Fights",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Victories",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterSkill_Skills_SkillsId",
                table: "CharacterSkill",
                column: "SkillsId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterSkill_Skills_SkillsId",
                table: "CharacterSkill");

            migrationBuilder.DropColumn(
                name: "Defeats",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Fights",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Victories",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "SkillsId",
                table: "CharacterSkill",
                newName: "SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_CharacterSkill_SkillsId",
                table: "CharacterSkill",
                newName: "IX_CharacterSkill_SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterSkill_Skills_SkillId",
                table: "CharacterSkill",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

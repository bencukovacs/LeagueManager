using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeagueManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMOMVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MOMVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FixtureId = table.Column<int>(type: "integer", nullable: false),
                    VotingTeamId = table.Column<int>(type: "integer", nullable: false),
                    VotedForOwnPlayerId = table.Column<int>(type: "integer", nullable: false),
                    VotedForOpponentPlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOMVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MOMVotes_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOMVotes_Players_VotedForOpponentPlayerId",
                        column: x => x.VotedForOpponentPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MOMVotes_Players_VotedForOwnPlayerId",
                        column: x => x.VotedForOwnPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MOMVotes_Teams_VotingTeamId",
                        column: x => x.VotingTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOMVotes_FixtureId",
                table: "MOMVotes",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MOMVotes_VotedForOpponentPlayerId",
                table: "MOMVotes",
                column: "VotedForOpponentPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MOMVotes_VotedForOwnPlayerId",
                table: "MOMVotes",
                column: "VotedForOwnPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MOMVotes_VotingTeamId",
                table: "MOMVotes",
                column: "VotingTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOMVotes");
        }
    }
}

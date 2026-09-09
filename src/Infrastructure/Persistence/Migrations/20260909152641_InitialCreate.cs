using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActiveScorerId = table.Column<int>(type: "integer", nullable: true),
                    ConcurrencyVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxOvers = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TossWonTeamId = table.Column<int>(type: "integer", nullable: true),
                    TeamBattingFirstId = table.Column<int>(type: "integer", nullable: true),
                    TeamBattingSecondId = table.Column<int>(type: "integer", nullable: true),
                    CurrentSuperOverNumber = table.Column<int>(type: "integer", nullable: false),
                    WinnerTeamId = table.Column<int>(type: "integer", nullable: true),
                    Result = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Innings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InningsNumber = table.Column<int>(type: "integer", nullable: false),
                    BattingTeamId = table.Column<int>(type: "integer", nullable: false),
                    BowlingTeamId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MaxOvers = table.Column<int>(type: "integer", nullable: false),
                    TargetRuns = table.Column<int>(type: "integer", nullable: true),
                    TotalRuns = table.Column<int>(type: "integer", nullable: false),
                    Wickets = table.Column<int>(type: "integer", nullable: false),
                    TotalBalls = table.Column<int>(type: "integer", nullable: false),
                    CurrentStrikerId = table.Column<int>(type: "integer", nullable: false),
                    CurrentNonStrikerId = table.Column<int>(type: "integer", nullable: false),
                    CurrentBowlerId = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    SuperOverNumber = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Innings_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchTeam1Details",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTeam1Details", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_MatchTeam1Details_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchTeam2Details",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTeam2Details", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_MatchTeam2Details_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BattingScores",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    InningsId = table.Column<Guid>(type: "uuid", nullable: false),
                    Runs = table.Column<int>(type: "integer", nullable: false),
                    Balls = table.Column<int>(type: "integer", nullable: false),
                    Fours = table.Column<int>(type: "integer", nullable: false),
                    Sixes = table.Column<int>(type: "integer", nullable: false),
                    DismissalFielderId = table.Column<int>(type: "integer", nullable: true),
                    DismissalWicketType = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattingScores", x => new { x.InningsId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_BattingScores_Innings_InningsId",
                        column: x => x.InningsId,
                        principalTable: "Innings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BowlingScores",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    InningsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunsConceded = table.Column<int>(type: "integer", nullable: false),
                    Balls = table.Column<int>(type: "integer", nullable: false),
                    Maidens = table.Column<int>(type: "integer", nullable: false),
                    TotalWickets = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BowlingScores", x => new { x.InningsId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_BowlingScores_Innings_InningsId",
                        column: x => x.InningsId,
                        principalTable: "Innings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Overs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsEditable = table.Column<bool>(type: "boolean", nullable: false),
                    InningsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Overs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Overs_Innings_InningsId",
                        column: x => x.InningsId,
                        principalTable: "Innings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchTeam1Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTeam1Players", x => new { x.MatchId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_MatchTeam1Players_MatchTeam1Details_MatchId",
                        column: x => x.MatchId,
                        principalTable: "MatchTeam1Details",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchTeam2Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTeam2Players", x => new { x.MatchId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_MatchTeam2Players_MatchTeam2Details_MatchId",
                        column: x => x.MatchId,
                        principalTable: "MatchTeam2Details",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    BallNumberInOver = table.Column<int>(type: "integer", nullable: false),
                    StrikerId = table.Column<int>(type: "integer", nullable: false),
                    NonStrikerId = table.Column<int>(type: "integer", nullable: false),
                    BowlerId = table.Column<int>(type: "integer", nullable: false),
                    BatterRuns = table.Column<int>(type: "integer", nullable: false),
                    TotalRuns = table.Column<int>(type: "integer", nullable: false),
                    ExtraType = table.Column<int>(type: "integer", nullable: false),
                    DismissedPlayerId = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverId = table.Column<Guid>(type: "uuid", nullable: false),
                    DismissalFielderId = table.Column<int>(type: "integer", nullable: true),
                    DismissalWicketType = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_Overs_OverId",
                        column: x => x.OverId,
                        principalTable: "Overs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattingScores_InningsId",
                table: "BattingScores",
                column: "InningsId");

            migrationBuilder.CreateIndex(
                name: "IX_BowlingScores_InningsId",
                table: "BowlingScores",
                column: "InningsId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OverId",
                table: "Deliveries",
                column: "OverId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_MatchId_InningsNumber",
                table: "Innings",
                columns: new[] { "MatchId", "InningsNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Overs_InningsId_OverNumber",
                table: "Overs",
                columns: new[] { "InningsId", "OverNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattingScores");

            migrationBuilder.DropTable(
                name: "BowlingScores");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "MatchTeam1Players");

            migrationBuilder.DropTable(
                name: "MatchTeam2Players");

            migrationBuilder.DropTable(
                name: "Overs");

            migrationBuilder.DropTable(
                name: "MatchTeam1Details");

            migrationBuilder.DropTable(
                name: "MatchTeam2Details");

            migrationBuilder.DropTable(
                name: "Innings");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}

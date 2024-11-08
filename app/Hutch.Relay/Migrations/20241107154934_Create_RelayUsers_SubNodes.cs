using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hutch.Relay.Migrations
{
    /// <inheritdoc />
    public partial class Create_RelayUsers_SubNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SubNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelayUserSubNode",
                columns: table => new
                {
                    RelayUsersId = table.Column<string>(type: "text", nullable: false),
                    SubNodesId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelayUserSubNode", x => new { x.RelayUsersId, x.SubNodesId });
                    table.ForeignKey(
                        name: "FK_RelayUserSubNode_AspNetUsers_RelayUsersId",
                        column: x => x.RelayUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelayUserSubNode_SubNodes_SubNodesId",
                        column: x => x.SubNodesId,
                        principalTable: "SubNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelayUserSubNode_SubNodesId",
                table: "RelayUserSubNode",
                column: "SubNodesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelayUserSubNode");

            migrationBuilder.DropTable(
                name: "SubNodes");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");
        }
    }
}

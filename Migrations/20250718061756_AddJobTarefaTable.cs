using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saigor.Migrations
{
    /// <inheritdoc />
    public partial class AddJobTarefaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobTarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    TarefaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConexoesId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTarefas_Conexoes_ConexoesId",
                        column: x => x.ConexoesId,
                        principalTable: "Conexoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTarefas_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTarefas_Tarefas_TarefaId",
                        column: x => x.TarefaId,
                        principalTable: "Tarefas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobTarefas_ConexoesId",
                table: "JobTarefas",
                column: "ConexoesId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTarefas_JobId",
                table: "JobTarefas",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTarefas_TarefaId",
                table: "JobTarefas",
                column: "TarefaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobTarefas");
        }
    }
}

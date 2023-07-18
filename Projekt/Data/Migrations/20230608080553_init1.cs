using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InsurenceCount",
                table: "InsurenceHolder",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InsurenceEventsCount",
                table: "Insurence",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsurenceCount",
                table: "InsurenceHolder");

            migrationBuilder.DropColumn(
                name: "InsurenceEventsCount",
                table: "Insurence");
        }
    }
}

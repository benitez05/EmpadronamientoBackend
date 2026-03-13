using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seagregaestructurabasedelsistemadeempadronamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Organizaciones",
                newName: "Municipio");

            migrationBuilder.RenameColumn(
                name: "Ciudad",
                table: "Organizaciones",
                newName: "Estado");

            migrationBuilder.AddColumn<int>(
                name: "CP",
                table: "Organizaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Calle",
                table: "Organizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Colonia",
                table: "Organizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NumeroExterior",
                table: "Organizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NumeroInterior",
                table: "Organizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Organizaciones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CP", "Calle", "Colonia", "NumeroExterior", "NumeroInterior" },
                values: new object[] { 0, "", null, "", "" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHjl5iseFJUGDbCs/1fgQOkSLLQAwFU1ivaX5qCixnnJiSrkZU8kB7MkRgyqxyN1DA==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGVdU5YfTB/cVkGuPh0+MQwLwwlf8FfXqKy4hAKkKCIUHsEAU2A4Tb1BTIj+0AcgfQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CP",
                table: "Organizaciones");

            migrationBuilder.DropColumn(
                name: "Calle",
                table: "Organizaciones");

            migrationBuilder.DropColumn(
                name: "Colonia",
                table: "Organizaciones");

            migrationBuilder.DropColumn(
                name: "NumeroExterior",
                table: "Organizaciones");

            migrationBuilder.DropColumn(
                name: "NumeroInterior",
                table: "Organizaciones");

            migrationBuilder.RenameColumn(
                name: "Municipio",
                table: "Organizaciones",
                newName: "Direccion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Organizaciones",
                newName: "Ciudad");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELw3oS4nY4cvAUHVKq19Pjp+eGUtP8aA7jJyPTaLB1fqkPoB0mgIFTGWxlXmF5BQjQ==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFMIipmYabutW8CvYhVkG1SFi6q8LqtGo90YXXd6yGkBQuB72Az+UY5MPzDACPVs7w==");
        }
    }
}

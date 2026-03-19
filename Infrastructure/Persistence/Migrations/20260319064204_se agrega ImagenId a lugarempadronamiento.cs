using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seagregaImagenIdalugarempadronamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImagenId",
                table: "LugaresEmpadronamiento",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHjkMyrj0Jh/n7rBhPfmJJyANxguhTd3DbHcBzBAixkFYjU36sEJYcJ+6Wmn17mrQA==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAVN9QNwZuSwkq1SWzBpLzjjH9Lw0ELQ+SfQte7jElNK3yIjJHarRHqe0KasdqR3MA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenId",
                table: "LugaresEmpadronamiento");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEKWzdzP/P6mrZttyDRuyukZr3uPT1tCKREhCtOHFphcUSnABVE2dL1dI5QnypkNQSA==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK+6oRidzBafsPDQ2nQHEVvgFOQAJ/N9BBF4ypJDXM4LR9zwZLGkwNbyjyGfEbXvJw==");
        }
    }
}

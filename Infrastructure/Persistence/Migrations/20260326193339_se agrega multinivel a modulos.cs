using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seagregamultinivelamodulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Multinivel",
                table: "Modulos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.InsertData(
                table: "Modulos",
                columns: new[] { "Id", "Color", "Icono", "K", "Nombre" },
                values: new object[] { 8, "#6B7280", null, "m", "Modulo" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBs13pDjqsTJqwOdL5ZJ9orH7LrEiAsuMDA9ExlffMMS48vN/JIjibvVDZJBZ3CwSw==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEMK2vJy4wT+hcsYv/FY8diEWZ8WKbueC0dHXZc86x/JkQVSslb2UHEdswl0tilJU6w==");

            migrationBuilder.InsertData(
                table: "OrganizacionModulos",
                columns: new[] { "ModuloId", "OrganizacionId", "Activo", "ActualizadoPor", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaActivacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion" },
                values: new object[] { 8, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrganizacionModulos",
                keyColumns: new[] { "ModuloId", "OrganizacionId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "Modulos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "Multinivel",
                table: "Modulos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEG2lA98V9+/9NtS/oObiNmp1Yz7gX+UZ6YyBPFrIrvDrL516hnG8sIwEy61v4cczQg==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK1mrlZhy+vHSpVMvTqxYcS+3xaI0YpGv3bCeiMb1GiOt7w61qyAc1gWvQwr7xWtDA==");
        }
    }
}

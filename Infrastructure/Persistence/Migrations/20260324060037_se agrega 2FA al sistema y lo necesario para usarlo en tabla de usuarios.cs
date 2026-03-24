using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seagrega2FAalsistemaylonecesarioparausarloentabladeusuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Catalogos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AddColumn<bool>(
                name: "DosPasosHabilitado",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DosPasosSecretKey",
                table: "Usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DosPasosHabilitado", "DosPasosSecretKey", "PasswordHash" },
                values: new object[] { false, null, "AQAAAAIAAYagAAAAEG2lA98V9+/9NtS/oObiNmp1Yz7gX+UZ6YyBPFrIrvDrL516hnG8sIwEy61v4cczQg==" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DosPasosHabilitado", "DosPasosSecretKey", "PasswordHash" },
                values: new object[] { false, null, "AQAAAAIAAYagAAAAEK1mrlZhy+vHSpVMvTqxYcS+3xaI0YpGv3bCeiMb1GiOt7w61qyAc1gWvQwr7xWtDA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosPasosHabilitado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DosPasosSecretKey",
                table: "Usuarios");

            migrationBuilder.InsertData(
                table: "Catalogos",
                columns: new[] { "Id", "ActualizadoPor", "Clave", "CreadoPor", "Descripcion", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "Nombre", "OrganizacionId" },
                values: new object[] { 4, null, "TIPO_FOTO", "System_Seed", "Clasificación de fotografías", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Tipo de Foto", 1 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHNaXgysEzeamjHLWdfFivk415qTOSR41weSaDVAM5a0B3DxNu47rI9nOcw1LA2A+A==");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEE2qoEKwQhP9Wsvm7KU2rAwSGDRWcVYw4JkkqgRImfvfEZ49W4aMLeGrY2CJZYADkA==");

            migrationBuilder.InsertData(
                table: "CatalogoItems",
                columns: new[] { "Id", "Activo", "ActualizadoPor", "CatalogoId", "Codigo", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "Nombre", "Orden", "OrganizacionId" },
                values: new object[,]
                {
                    { 30, true, null, 4, "ROSTRO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Rostro", 1, 1 },
                    { 31, true, null, 4, "CUERPO_COMPLETO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Cuerpo Completo", 2, 1 },
                    { 32, true, null, 4, "IDENTIFICACION", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Identificación", 3, 1 },
                    { 33, true, null, 4, "SENAS", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Señas Particulares", 4, 1 }
                });
        }
    }
}

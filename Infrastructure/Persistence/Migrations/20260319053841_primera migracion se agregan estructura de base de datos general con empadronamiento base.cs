using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class primeramigracionseagreganestructuradebasededatosgeneralconempadronamientobase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfiguracionGlobal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreSistema = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizacionMaestraId = table.Column<int>(type: "int", nullable: false),
                    ModoAdminGlobalActivo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxIntentosLoginFallidos = table.Column<int>(type: "int", nullable: false),
                    MinutosBloqueo = table.Column<int>(type: "int", nullable: false),
                    ZonaHoraria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionGlobal", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    K = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false, defaultValue: "#3B82F6")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxUsuarios = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailContacto = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Calle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroExterior = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroInterior = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CP = table.Column<int>(type: "int", nullable: false),
                    Colonia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Municipio = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Pais = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MotivoSuspension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizaciones_Planes_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Catalogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Clave = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalogos_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LugaresEmpadronamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Calle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroExterior = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroInterior = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CP = table.Column<int>(type: "int", nullable: false),
                    Colonia = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Municipio = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referencia = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitud = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LugaresEmpadronamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LugaresEmpadronamiento_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganizacionModulos",
                columns: table => new
                {
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    ModuloId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaActivacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizacionModulos", x => new { x.OrganizacionId, x.ModuloId });
                    table.ForeignKey(
                        name: "FK_OrganizacionModulos_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizacionModulos_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApellidoPaterno = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApellidoMaterno = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Edad = table.Column<int>(type: "int", nullable: true),
                    Estatura = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Sexo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Originario = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apodo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nacionalidad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstadoCivil = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Escolaridad = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OficioProfesion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObservacionesGenerales = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personas_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CatalogoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CatalogoId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogoItems_Catalogos_CatalogoId",
                        column: x => x.CatalogoId,
                        principalTable: "Catalogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogoItems_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DireccionesPersona",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Calle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroExterior = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroInterior = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CP = table.Column<int>(type: "int", nullable: false),
                    Colonia = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Municipio = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Pais = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referencia = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitud = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EsPrincipal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DireccionesPersona", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DireccionesPersona_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Familiares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreCompleto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parentesco = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familiares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Familiares_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RedesSociales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TipoRedSocial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Usuario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlPerfil = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedesSociales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedesSociales_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ModuloId = table.Column<int>(type: "int", nullable: false),
                    Lvl = table.Column<int>(type: "int", nullable: false, comment: "Nivel de acceso: 1-Leer, 2-Escribir, 3-Borrar")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => new { x.RoleId, x.ModuloId });
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellidos = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Celular = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Imagen = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    CorreoConfirmado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    TokenConfirmacionCorreo = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UltimoLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Empadronamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Folio = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CRP = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NarrativaHechos = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioResponsableId = table.Column<int>(type: "int", nullable: false),
                    LugarEmpadronamientoId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empadronamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empadronamientos_LugaresEmpadronamiento_LugarEmpadronamiento~",
                        column: x => x.LugarEmpadronamientoId,
                        principalTable: "LugaresEmpadronamiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Empadronamientos_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Empadronamientos_Usuarios_UsuarioResponsableId",
                        column: x => x.UsuarioResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsuarioSesiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Jti = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshToken = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceInfo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UltimaActividad = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioSesiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioSesiones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmpadronamientoPersonas",
                columns: table => new
                {
                    EmpadronamientoId = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpadronamientoPersonas", x => new { x.EmpadronamientoId, x.PersonaId });
                    table.ForeignKey(
                        name: "FK_EmpadronamientoPersonas_Empadronamientos_EmpadronamientoId",
                        column: x => x.EmpadronamientoId,
                        principalTable: "Empadronamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpadronamientoPersonas_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdPersona = table.Column<int>(type: "int", nullable: true),
                    IdEmpadronamiento = table.Column<int>(type: "int", nullable: true),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    S3Key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotos_Empadronamientos_IdEmpadronamiento",
                        column: x => x.IdEmpadronamiento,
                        principalTable: "Empadronamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fotos_Personas_IdPersona",
                        column: x => x.IdPersona,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Caras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdFoto = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    FaceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    S3Key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Confidence = table.Column<float>(type: "float", nullable: false),
                    BoundingBox = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonaId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpCreacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoCreacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpUltimaActualizacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispositivoUltimaActualizacion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Caras_Fotos_IdFoto",
                        column: x => x.IdFoto,
                        principalTable: "Fotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Caras_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ConfiguracionGlobal",
                columns: new[] { "Id", "ActualizadoPor", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "MaxIntentosLoginFallidos", "MinutosBloqueo", "ModoAdminGlobalActivo", "NombreSistema", "OrganizacionMaestraId", "ZonaHoraria" },
                values: new object[] { 1, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, 5, 15, true, "BenitezLabs Enterprise", 1, "Central Standard Time" });

            migrationBuilder.InsertData(
                table: "Modulos",
                columns: new[] { "Id", "Color", "Icono", "K", "Nombre" },
                values: new object[,]
                {
                    { 1, "#6B7280", null, "u", "Usuarios" },
                    { 2, "#6B7280", null, "r", "Roles" },
                    { 3, "#6B7280", null, "o", "Organizaciones" },
                    { 4, "#6B7280", null, "c", "Configuración" },
                    { 5, "#6B7280", null, "e", "Empadronamiento" },
                    { 6, "#6B7280", null, "b", "Busqueda" },
                    { 7, "#6B7280", null, "t", "Estadisticas" }
                });

            migrationBuilder.InsertData(
                table: "Planes",
                columns: new[] { "Id", "Activo", "FechaCreacion", "MaxUsuarios", "Nombre", "Precio" },
                values: new object[] { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9999, "Plan Maestro / Enterprise", 0m });

            migrationBuilder.InsertData(
                table: "Organizaciones",
                columns: new[] { "Id", "Activa", "ActualizadoPor", "CP", "Calle", "Colonia", "CreadoPor", "Descripcion", "DispositivoCreacion", "DispositivoUltimaActualizacion", "EmailContacto", "Estado", "FechaCreacion", "FechaUltimaActualizacion", "FechaVencimiento", "IpCreacion", "IpUltimaActualizacion", "LogoUrl", "MotivoSuspension", "Municipio", "Nombre", "NumeroExterior", "NumeroInterior", "Pais", "PlanId", "Telefono" },
                values: new object[] { 1, true, null, 0, "", null, "System_Seed", "Organización Maestra del Sistema", "Console_Setup", null, "admin@benitezlabs.com", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2076, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "127.0.0.1", null, null, null, null, "BenitezLabs Admin", "", "", null, 1, null });

            migrationBuilder.InsertData(
                table: "Catalogos",
                columns: new[] { "Id", "ActualizadoPor", "Clave", "CreadoPor", "Descripcion", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "Nombre", "OrganizacionId" },
                values: new object[,]
                {
                    { 1, null, "TIPO_RED_SOCIAL", "System_Seed", "Redes sociales disponibles", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Tipo de Red Social", 1 },
                    { 2, null, "ESTADO_CIVIL", "System_Seed", "Estado civil de la persona", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Estado Civil", 1 },
                    { 3, null, "ESCOLARIDAD", "System_Seed", "Nivel educativo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Escolaridad", 1 },
                    { 4, null, "TIPO_FOTO", "System_Seed", "Clasificación de fotografías", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Tipo de Foto", 1 },
                    { 5, null, "PARENTESCO", "System_Seed", "Relación familiar", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Parentesco", 1 },
                    { 6, null, "TIPO_CARRO_RADIO_PATRULLA", "System_Seed", "Tipos de unidades", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Tipo Carro Radio Patrulla", 1 },
                    { 7, null, "OFICIO_PROFESION", "System_Seed", "Ocupación de la persona", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Oficio / Profesión", 1 }
                });

            migrationBuilder.InsertData(
                table: "OrganizacionModulos",
                columns: new[] { "ModuloId", "OrganizacionId", "Activo", "ActualizadoPor", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaActivacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion" },
                values: new object[,]
                {
                    { 1, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 2, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 3, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 4, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 5, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 6, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null },
                    { 7, 1, true, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ActualizadoPor", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "Nombre", "OrganizacionId" },
                values: new object[,]
                {
                    { 1, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "SuperAdmin", 1 },
                    { 2, null, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Usuario", 1 }
                });

            migrationBuilder.InsertData(
                table: "CatalogoItems",
                columns: new[] { "Id", "Activo", "ActualizadoPor", "CatalogoId", "Codigo", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "IpCreacion", "IpUltimaActualizacion", "Nombre", "Orden", "OrganizacionId" },
                values: new object[,]
                {
                    { 1, true, null, 1, "FACEBOOK", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Facebook", 1, 1 },
                    { 2, true, null, 1, "INSTAGRAM", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Instagram", 2, 1 },
                    { 3, true, null, 1, "X", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "X (Twitter)", 3, 1 },
                    { 4, true, null, 1, "TIKTOK", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "TikTok", 4, 1 },
                    { 5, true, null, 1, "WHATSAPP", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "WhatsApp", 5, 1 },
                    { 6, true, null, 1, "TELEGRAM", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Telegram", 6, 1 },
                    { 10, true, null, 2, "SOLTERO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Soltero", 1, 1 },
                    { 11, true, null, 2, "CASADO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Casado", 2, 1 },
                    { 12, true, null, 2, "DIVORCIADO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Divorciado", 3, 1 },
                    { 13, true, null, 2, "UNION_LIBRE", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Unión Libre", 4, 1 },
                    { 14, true, null, 2, "VIUDO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Viudo", 5, 1 },
                    { 20, true, null, 3, "SIN_ESTUDIOS", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Sin estudios", 1, 1 },
                    { 21, true, null, 3, "PRIMARIA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Primaria", 2, 1 },
                    { 22, true, null, 3, "SECUNDARIA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Secundaria", 3, 1 },
                    { 23, true, null, 3, "PREPARATORIA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Preparatoria", 4, 1 },
                    { 24, true, null, 3, "TECNICO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Técnico", 5, 1 },
                    { 25, true, null, 3, "LICENCIATURA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Licenciatura", 6, 1 },
                    { 26, true, null, 3, "MAESTRIA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Maestría", 7, 1 },
                    { 27, true, null, 3, "DOCTORADO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Doctorado", 8, 1 },
                    { 30, true, null, 4, "ROSTRO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Rostro", 1, 1 },
                    { 31, true, null, 4, "CUERPO_COMPLETO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Cuerpo Completo", 2, 1 },
                    { 32, true, null, 4, "IDENTIFICACION", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Identificación", 3, 1 },
                    { 33, true, null, 4, "SENAS", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Señas Particulares", 4, 1 },
                    { 40, true, null, 5, "PADRE", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Padre", 1, 1 },
                    { 41, true, null, 5, "MADRE", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Madre", 2, 1 },
                    { 42, true, null, 5, "HERMANO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Hermano(a)", 3, 1 },
                    { 43, true, null, 5, "PAREJA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Pareja", 4, 1 },
                    { 44, true, null, 5, "HIJO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Hijo(a)", 5, 1 },
                    { 45, true, null, 5, "OTRO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Otro", 6, 1 },
                    { 50, true, null, 6, "SEDAN", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Sedán", 1, 1 },
                    { 51, true, null, 6, "PICKUP", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "PickUp", 2, 1 },
                    { 52, true, null, 6, "MOTO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Motocicleta", 3, 1 },
                    { 53, true, null, 6, "SUV", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "SUV", 4, 1 },
                    { 60, true, null, 7, "EMPLEADO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Empleado", 1, 1 },
                    { 61, true, null, 7, "OBRERO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Obrero", 2, 1 },
                    { 62, true, null, 7, "COMERCIANTE", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Comerciante", 3, 1 },
                    { 63, true, null, 7, "ESTUDIANTE", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Estudiante", 4, 1 },
                    { 64, true, null, 7, "DESEMPLEADO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Desempleado", 5, 1 },
                    { 65, true, null, 7, "CHOFER", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Chofer", 6, 1 },
                    { 66, true, null, 7, "ALBANIL", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Albañil", 7, 1 },
                    { 67, true, null, 7, "TECNICO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Técnico", 8, 1 },
                    { 68, true, null, 7, "PROFESIONISTA", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Profesionista", 9, 1 },
                    { 69, true, null, 7, "OTRO", "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "127.0.0.1", null, "Otro", 10, 1 }
                });

            migrationBuilder.InsertData(
                table: "RolesPermisos",
                columns: new[] { "ModuloId", "RoleId", "Lvl" },
                values: new object[,]
                {
                    { 1, 2, 1 },
                    { 2, 2, 1 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "ActualizadoPor", "Apellidos", "BloqueadoHasta", "Celular", "Correo", "CorreoConfirmado", "CreadoPor", "DispositivoCreacion", "DispositivoUltimaActualizacion", "FechaCreacion", "FechaUltimaActualizacion", "Imagen", "IpCreacion", "IpUltimaActualizacion", "Nombre", "OrganizacionId", "PasswordHash", "RoleId", "Tipo", "TokenConfirmacionCorreo", "UltimoLogin" },
                values: new object[,]
                {
                    { 1, true, null, "BenitezLabs", null, null, "admin@benitezlabs.com", true, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "127.0.0.1", null, "Admin", 1, "AQAAAAIAAYagAAAAEKWzdzP/P6mrZttyDRuyukZr3uPT1tCKREhCtOHFphcUSnABVE2dL1dI5QnypkNQSA==", 1, 3, null, null },
                    { 2, true, null, "Operator", null, null, "user@benitezlabs.com", true, "System_Seed", "Console_Setup", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "127.0.0.1", null, "Demo", 1, "AQAAAAIAAYagAAAAEK+6oRidzBafsPDQ2nQHEVvgFOQAJ/N9BBF4ypJDXM4LR9zwZLGkwNbyjyGfEbXvJw==", 2, 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Caras_FaceId",
                table: "Caras",
                column: "FaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Caras_IdFoto",
                table: "Caras",
                column: "IdFoto");

            migrationBuilder.CreateIndex(
                name: "IX_Caras_OrganizacionId",
                table: "Caras",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Caras_PersonaId",
                table: "Caras",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Caras_S3Key",
                table: "Caras",
                column: "S3Key");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItems_CatalogoId_Orden",
                table: "CatalogoItems",
                columns: new[] { "CatalogoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItems_OrganizacionId",
                table: "CatalogoItems",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogos_Clave_OrganizacionId",
                table: "Catalogos",
                columns: new[] { "Clave", "OrganizacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogos_OrganizacionId",
                table: "Catalogos",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DireccionesPersona_PersonaId",
                table: "DireccionesPersona",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpadronamientoPersonas_PersonaId",
                table: "EmpadronamientoPersonas",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empadronamientos_LugarEmpadronamientoId",
                table: "Empadronamientos",
                column: "LugarEmpadronamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empadronamientos_OrganizacionId",
                table: "Empadronamientos",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Empadronamientos_UsuarioResponsableId",
                table: "Empadronamientos",
                column: "UsuarioResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Familiares_PersonaId",
                table: "Familiares",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_IdEmpadronamiento",
                table: "Fotos",
                column: "IdEmpadronamiento");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_IdPersona",
                table: "Fotos",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_OrganizacionId",
                table: "Fotos",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_S3Key",
                table: "Fotos",
                column: "S3Key");

            migrationBuilder.CreateIndex(
                name: "IX_LugaresEmpadronamiento_OrganizacionId",
                table: "LugaresEmpadronamiento",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_K",
                table: "Modulos",
                column: "K",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizaciones_EmailContacto",
                table: "Organizaciones",
                column: "EmailContacto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizaciones_PlanId",
                table: "Organizaciones",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizacionModulos_ModuloId",
                table: "OrganizacionModulos",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Nombre_ApellidoPaterno_ApellidoMaterno",
                table: "Personas",
                columns: new[] { "Nombre", "ApellidoPaterno", "ApellidoMaterno" });

            migrationBuilder.CreateIndex(
                name: "IX_Personas_OrganizacionId",
                table: "Personas",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RedesSociales_PersonaId",
                table: "RedesSociales",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre_OrganizacionId",
                table: "Roles",
                columns: new[] { "Nombre", "OrganizacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrganizacionId",
                table: "Roles",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_ModuloId",
                table: "RolesPermisos",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_OrganizacionId",
                table: "Usuarios",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RoleId",
                table: "Usuarios",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSesiones_Jti",
                table: "UsuarioSesiones",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSesiones_UsuarioId",
                table: "UsuarioSesiones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Caras");

            migrationBuilder.DropTable(
                name: "CatalogoItems");

            migrationBuilder.DropTable(
                name: "ConfiguracionGlobal");

            migrationBuilder.DropTable(
                name: "DireccionesPersona");

            migrationBuilder.DropTable(
                name: "EmpadronamientoPersonas");

            migrationBuilder.DropTable(
                name: "Familiares");

            migrationBuilder.DropTable(
                name: "OrganizacionModulos");

            migrationBuilder.DropTable(
                name: "RedesSociales");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "UsuarioSesiones");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "Catalogos");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.DropTable(
                name: "Empadronamientos");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "LugaresEmpadronamiento");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Organizaciones");

            migrationBuilder.DropTable(
                name: "Planes");
        }
    }
}

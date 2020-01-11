using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LocalizationTest.Data.EntityFramework.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Basic");

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "City",
                schema: "Basic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PhoneCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "Basic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AsDefault = table.Column<bool>(nullable: false),
                    CityId = table.Column<Guid>(nullable: true),
                    ContentAddress = table.Column<string>(maxLength: 1000, nullable: true),
                    PersonId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_City_CityId",
                        column: x => x.CityId,
                        principalSchema: "Basic",
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CityLocalization",
                schema: "Basic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CultureId = table.Column<Guid>(nullable: false),
                    LocalizableId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityLocalization_City_LocalizableId",
                        column: x => x.LocalizableId,
                        principalSchema: "Basic",
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "FirstName", "LastName" },
                values: new object[] { new Guid("c96c5020-31f9-4dca-a7c9-1cb55722e4b2"), "Soheil", "Hasankhani" });

            migrationBuilder.InsertData(
                schema: "Basic",
                table: "City",
                columns: new[] { "Id", "PhoneCode" },
                values: new object[] { new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"), "21" });

            migrationBuilder.InsertData(
                schema: "Basic",
                table: "Address",
                columns: new[] { "Id", "AsDefault", "CityId", "ContentAddress", "PersonId" },
                values: new object[] { new Guid("a0ba6506-1993-44d3-901b-e199f6800d82"), true, new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"), "Test Address", new Guid("c96c5020-31f9-4dca-a7c9-1cb55722e4b2") });

            migrationBuilder.InsertData(
                schema: "Basic",
                table: "CityLocalization",
                columns: new[] { "Id", "CultureId", "Description", "LocalizableId", "Title" },
                values: new object[] { new Guid("49e24a56-9a47-4684-9a15-a5891e957c22"), new Guid("0c859760-624e-459d-9ddd-fa8b0bcca02b"), "توضیحات تهران", new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"), "تهران" });

            migrationBuilder.InsertData(
                schema: "Basic",
                table: "CityLocalization",
                columns: new[] { "Id", "CultureId", "Description", "LocalizableId", "Title" },
                values: new object[] { new Guid("e7000da7-a93f-46f8-843e-c7840f7c1ffc"), new Guid("ca8a9c53-31c1-458d-9844-504a33309e31"), "Tehran Description", new Guid("e8fd560c-5c5e-45d4-b93a-394a26909cdf"), "Tehran" });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CityId",
                schema: "Basic",
                table: "Address",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_PersonId",
                schema: "Basic",
                table: "Address",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_CityLocalization_LocalizableId",
                schema: "Basic",
                table: "CityLocalization",
                column: "LocalizableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address",
                schema: "Basic");

            migrationBuilder.DropTable(
                name: "CityLocalization",
                schema: "Basic");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "City",
                schema: "Basic");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCore_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRememberMeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RememberMe",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01234567-89ab-cdef-0123-456789abcdef",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "RememberMe", "SecurityStamp" },
                values: new object[] { "12665ac5-b067-40c3-a68b-5c4ba0e2e364", "AQAAAAIAAYagAAAAEPzz9BzgB2JixsKW7mseItUN4C6+oDuHeEvxu9YAD8bfEHepw9hSiwklNLEuYwWBbQ==", false, "566b8089-f523-40ac-9f6c-98378aeca96f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "fedcba98-7654-3210-fedc-ba9876543210",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "RememberMe", "SecurityStamp" },
                values: new object[] { "dca3d846-ed2e-4d42-af4d-64f306b44a66", "AQAAAAIAAYagAAAAEFriyhOSOmLE1bQad3bOwdKGYN94i1FQWKIgMf3GTB0ReD9K0qmWOo+AK/GX0E+sXw==", false, "3cc914a2-b495-4a84-a515-2f12ab679ab0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberMe",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01234567-89ab-cdef-0123-456789abcdef",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2bb5649e-5289-4272-b30e-94ff3c164031", "AQAAAAIAAYagAAAAEHhbRR23wamBnsm24ZtNTnSnSLCO9+zjWhVV49qxAfDl20ET0/hXXsO4T8XlJgqWeg==", "b49a7a2c-e998-4f1e-af4d-c7f0b142d283" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "fedcba98-7654-3210-fedc-ba9876543210",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "64f9ac91-cc98-496a-a3ee-521a07ab21e4", "AQAAAAIAAYagAAAAEGS4oufoUCiYod4ZkeKNphdAUtLD5A9ngjWUrNB79TLX2UsCN/o4CEc6GnZJ/gKZ+Q==", "76f1d9a0-3af1-4208-b41c-4dae159b54ef" });
        }
    }
}

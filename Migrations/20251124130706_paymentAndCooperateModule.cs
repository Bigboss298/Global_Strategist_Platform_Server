using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Global_Strategist_Platform_Server.Migrations
{
    /// <inheritdoc />
    public partial class paymentAndCooperateModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Certification",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CorporateAccountId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileUrl",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortBio",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CorporateAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationName = table.Column<string>(type: "text", nullable: false),
                    RepresentativeFirstName = table.Column<string>(type: "text", nullable: false),
                    RepresentativeLastName = table.Column<string>(type: "text", nullable: false),
                    RepresentativeEmail = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Sector = table.Column<string>(type: "text", nullable: false),
                    CompanyOverview = table.Column<string>(type: "text", nullable: false),
                    ContributionInterestAreasJson = table.Column<string>(type: "text", nullable: false),
                    SupportingDocumentsJson = table.Column<string>(type: "text", nullable: false),
                    OptionalNotes = table.Column<string>(type: "text", nullable: false),
                    DeclarationAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    PaidMemberSlots = table.Column<int>(type: "integer", nullable: false),
                    UsedMemberSlots = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporateInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorporateAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateInvites_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorporatePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorporateAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SlotsPurchased = table.Column<int>(type: "integer", nullable: false),
                    PaymentReference = table.Column<string>(type: "text", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporatePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporatePayments_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CorporateAccountId",
                table: "Users",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateInvites_CorporateAccountId",
                table: "CorporateInvites",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporatePayments_CorporateAccountId",
                table: "CorporatePayments",
                column: "CorporateAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CorporateAccounts_CorporateAccountId",
                table: "Users",
                column: "CorporateAccountId",
                principalTable: "CorporateAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CorporateAccounts_CorporateAccountId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CorporateInvites");

            migrationBuilder.DropTable(
                name: "CorporatePayments");

            migrationBuilder.DropTable(
                name: "CorporateAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Users_CorporateAccountId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Certification",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CorporateAccountId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CvFileUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShortBio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Users");
        }
    }
}

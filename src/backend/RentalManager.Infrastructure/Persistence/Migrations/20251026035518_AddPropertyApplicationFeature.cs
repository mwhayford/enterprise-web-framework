// Copyright (c) RentalManager. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyApplicationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultApplicationFee_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DefaultApplicationFee_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ApplicationFeeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePaymentUpfront = table.Column<bool>(type: "boolean", nullable: false),
                    MaxApplicationsPerUser = table.Column<int>(type: "integer", nullable: true),
                    ApplicationFormFields = table.Column<string>(type: "jsonb", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address_Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address_State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address_ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PropertyType = table.Column<int>(type: "integer", nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    SquareFeet = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MonthlyRent_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyRent_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SecurityDeposit_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SecurityDeposit_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AvailableDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApplicationFee_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ApplicationFee_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Amenities = table.Column<List<string>>(type: "jsonb", nullable: false),
                    Images = table.Column<List<string>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApplicationData = table.Column<string>(type: "jsonb", nullable: false),
                    ApplicationFee_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApplicationFee_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ApplicationFeePaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_AvailableDate",
                table: "Properties",
                column: "AvailableDate");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CreatedAt",
                table: "Properties",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Status",
                table: "Properties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_ApplicantId",
                table: "PropertyApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_CreatedAt",
                table: "PropertyApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_PropertyId",
                table: "PropertyApplications",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_Status",
                table: "PropertyApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_SubmittedAt",
                table: "PropertyApplications",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "PropertyApplications");
        }
    }
}

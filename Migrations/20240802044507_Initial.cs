using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hotel_system_backend.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateSequence(
                name: "AuthorityId_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "BusinessDayId_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "DeletedTransactionId_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "GuestId_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "RecordNumber_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "ReservationNumber_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "TransactionId_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateSequence(
                name: "UserID_seq",
                schema: "public",
                startValue: 1000L);

            migrationBuilder.CreateTable(
                name: "Authorities",
                columns: table => new
                {
                    AuthorityId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuthorityId_seq\"')"),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorities", x => x.AuthorityId);
                });

            migrationBuilder.CreateTable(
                name: "BusinessDay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"BusinessDayId_seq\"')"),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDay", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"UserID_seq\"')"),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Authorities = table.Column<string[]>(type: "text[]", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Record",
                columns: table => new
                {
                    RecordNumber = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"RecordNumber_seq\"')"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DateTimeOfRecord = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record", x => x.RecordNumber);
                    table.ForeignKey(
                        name: "FK_Record_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    ReservationNumber = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"ReservationNumber_seq\"')"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    countOfRoomsReserved = table.Column<int>(type: "integer", nullable: false),
                    ReservedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    StatusMessage = table.Column<string>(type: "text", nullable: true),
                    MealPlan = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    Pax = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.ReservationNumber);
                    table.ForeignKey(
                        name: "FK_Reservation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Guest",
                columns: table => new
                {
                    GuestId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"GuestId_seq\"')"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    DocumentId = table.Column<string>(type: "text", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    BirthPlace = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReservationNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guest", x => x.GuestId);
                    table.ForeignKey(
                        name: "FK_Guest_Reservation_ReservationNumber",
                        column: x => x.ReservationNumber,
                        principalTable: "Reservation",
                        principalColumn: "ReservationNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomNumber = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    isOccupied = table.Column<bool>(type: "boolean", nullable: false),
                    isDirty = table.Column<bool>(type: "boolean", nullable: false),
                    isOutOfOrder = table.Column<bool>(type: "boolean", nullable: false),
                    isReserved = table.Column<bool>(type: "boolean", nullable: false),
                    ReservationNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomNumber);
                    table.ForeignKey(
                        name: "FK_Room_Reservation_ReservationNumber",
                        column: x => x.ReservationNumber,
                        principalTable: "Reservation",
                        principalColumn: "ReservationNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"TransactionId_seq\"')"),
                    ExactDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BusinessDayId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ReservationNumber = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreditOrDebit = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_BusinessDay_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Reservation_ReservationNumber",
                        column: x => x.ReservationNumber,
                        principalTable: "Reservation",
                        principalColumn: "ReservationNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeletedTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DeletedTransactionId_seq\"')"),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExactDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BusinessDayId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeletedTransactions_BusinessDay_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedTransactions_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTransactions_BusinessDayId",
                table: "DeletedTransactions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTransactions_TransactionId",
                table: "DeletedTransactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTransactions_UserId",
                table: "DeletedTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Guest_ReservationNumber",
                table: "Guest",
                column: "ReservationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Record_UserId",
                table: "Record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_UserId",
                table: "Reservation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Room_ReservationNumber",
                table: "Room",
                column: "ReservationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BusinessDayId",
                table: "Transactions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReservationNumber",
                table: "Transactions",
                column: "ReservationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authorities");

            migrationBuilder.DropTable(
                name: "DeletedTransactions");

            migrationBuilder.DropTable(
                name: "Guest");

            migrationBuilder.DropTable(
                name: "Record");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "BusinessDay");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropSequence(
                name: "AuthorityId_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "BusinessDayId_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "DeletedTransactionId_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "GuestId_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "RecordNumber_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "ReservationNumber_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "TransactionId_seq",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "UserID_seq",
                schema: "public");
        }
    }
}

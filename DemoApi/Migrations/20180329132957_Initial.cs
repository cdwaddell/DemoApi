using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DemoApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Sub = table.Column<string>(maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 64, nullable: true),
                    ImageUrl = table.Column<string>(maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Sub);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBySub = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(maxLength: 128, nullable: true),
                    Key = table.Column<string>(maxLength: 32, nullable: false),
                    LastModifiedBySub = table.Column<string>(nullable: true),
                    LastModifiedDate = table.Column<DateTime>(nullable: true),
                    OwnerSub = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.UniqueConstraint("AK_Blogs_Key", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Blogs_UserProfiles_CreatedBySub",
                        column: x => x.CreatedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blogs_UserProfiles_LastModifiedBySub",
                        column: x => x.LastModifiedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blogs_UserProfiles_OwnerSub",
                        column: x => x.OwnerSub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorSub = table.Column<string>(nullable: true),
                    BlogId = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    CreatedBySub = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    LastModifiedBySub = table.Column<string>(nullable: true),
                    LastModifiedDate = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publications_UserProfiles_AuthorSub",
                        column: x => x.AuthorSub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publications_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publications_UserProfiles_CreatedBySub",
                        column: x => x.CreatedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publications_UserProfiles_LastModifiedBySub",
                        column: x => x.LastModifiedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlogId = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    CreatedBySub = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBySub = table.Column<string>(nullable: true),
                    LastModifiedDate = table.Column<DateTime>(nullable: true),
                    ParentMessageId = table.Column<int>(nullable: true),
                    PublicationId = table.Column<int>(nullable: true),
                    SenderSub = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_UserProfiles_CreatedBySub",
                        column: x => x.CreatedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_UserProfiles_LastModifiedBySub",
                        column: x => x.LastModifiedBySub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_UserProfiles_SenderSub",
                        column: x => x.SenderSub,
                        principalTable: "UserProfiles",
                        principalColumn: "Sub",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CreatedBySub",
                table: "Blogs",
                column: "CreatedBySub");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_LastModifiedBySub",
                table: "Blogs",
                column: "LastModifiedBySub");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_OwnerSub",
                table: "Blogs",
                column: "OwnerSub");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_BlogId",
                table: "Messages",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedBySub",
                table: "Messages",
                column: "CreatedBySub");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_LastModifiedBySub",
                table: "Messages",
                column: "LastModifiedBySub");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PublicationId",
                table: "Messages",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderSub",
                table: "Messages",
                column: "SenderSub");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_AuthorSub",
                table: "Publications",
                column: "AuthorSub");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_BlogId",
                table: "Publications",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_CreatedBySub",
                table: "Publications",
                column: "CreatedBySub");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_LastModifiedBySub",
                table: "Publications",
                column: "LastModifiedBySub");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}

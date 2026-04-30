using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EFCore.Migrations.CustomSql.Tests.MigrationTests.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "BlogBaseSequence");

            migrationBuilder.CreateTable(
                name: "ArticleBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleBase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BlogBaseSequence\"')"),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BlogBaseSequence\"')"),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    TextA = table.Column<string>(type: "text", nullable: true),
                    TextB = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostBase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ContentA = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleA_ArticleBase_Id",
                        column: x => x.Id,
                        principalTable: "ArticleBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ContentB = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleB_ArticleBase_Id",
                        column: x => x.Id,
                        principalTable: "ArticleBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.blog_names\nAS SELECT \"Id\", \"Name\" FROM \"Blogs\"");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION GetName(id integer)\nRETURNS text AS $$\nBEGIN\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n END;\n$$ LANGUAGE plpgsql;");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION get_blog_url(id integer)\r\nRETURNS text\r\nLANGUAGE plpgsql\r\nAS $$\r\nBEGIN\r\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\r\nEND;\r\n$$;");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION before_insert_or_update_blog()\r\nRETURNS trigger\r\nLANGUAGE plpgsql\r\nAS $$\r\nBEGIN\r\nIF NEW.\"Url\" IS NOT NULL AND NEW.\"Url\" IS DISTINCT FROM OLD.\"Url\" THEN\n    RAISE EXCEPTION 'Нельзя менять URL';\nEND IF;\nIF NEW.\"Name\" IS NOT NULL THEN\n    UPDATE \"Blogs\" SET \"Url\" = NEW.\"Url\"\n    WHERE \"Name\" = NEW.\"Name\";\nEND IF;\r\nRETURN NEW;\r\nEND;\r\n$$;\r\n\r\nCREATE TRIGGER before_insert_or_update_blog\r\nBEFORE INSERT OR UPDATE ON \"Blogs\"\r\nFOR EACH ROW\r\nEXECUTE FUNCTION before_insert_or_update_blog();");

            migrationBuilder.Sql("CREATE OR REPLACE VIEW blog_view AS\r\nSELECT * FROM \"Blogs\";");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION prevent_update_negative_amount()\r\nRETURNS trigger\r\nLANGUAGE plpgsql\r\nAS $$\r\nBEGIN\r\nIF NEW.total_amount < 0 THEN RAISE EXCEPTION 'amount negative'; END IF;\r\nRETURN NEW;\r\nEND;\r\n$$;\r\n\r\nCREATE TRIGGER prevent_update_negative_amount\r\nBEFORE UPDATE ON \"Orders\"\r\nFOR EACH ROW\r\nEXECUTE FUNCTION prevent_update_negative_amount();");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION set_order_defaults()\r\nRETURNS trigger\r\nLANGUAGE plpgsql\r\nAS $$\r\nBEGIN\r\nNEW.is_confirmed = false;\r\nRETURN NEW;\r\nEND;\r\n$$;\r\n\r\nCREATE TRIGGER set_order_defaults\r\nBEFORE INSERT ON \"Orders\"\r\nFOR EACH ROW\r\nEXECUTE FUNCTION set_order_defaults();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.blog_names");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS GetName");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_blog_url(id integer);");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS before_insert_or_update_blog() CASCADE;");

            migrationBuilder.Sql("DROP VIEW IF EXISTS blog_view;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_update_negative_amount() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_order_defaults() CASCADE;");

            migrationBuilder.DropTable(
                name: "ArticleA");

            migrationBuilder.DropTable(
                name: "ArticleB");

            migrationBuilder.DropTable(
                name: "BlogA");

            migrationBuilder.DropTable(
                name: "BlogB");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PostBase");

            migrationBuilder.DropTable(
                name: "ArticleBase");

            migrationBuilder.DropSequence(
                name: "BlogBaseSequence");
        }
    }
}

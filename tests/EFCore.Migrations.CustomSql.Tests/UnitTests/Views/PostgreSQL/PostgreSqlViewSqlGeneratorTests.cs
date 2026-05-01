using EFCore.Migrations.CustomSql.PostgreSQL.Views;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Views.PostgreSQL;

public class PostgreSqlViewSqlGeneratorTests
{
    private readonly PostgreSqlViewSqlGenerator _generator;

    public PostgreSqlViewSqlGeneratorTests()
    {
        _generator = new PostgreSqlViewSqlGenerator(new FakeSqlGenerationHelper());
    }

    private static ViewObject MakeView(
        string name = "my_view",
        string schema = null,
        string body = "SELECT 1",
        string sqlUp = null)
        => new ViewObject
        {
            Name = name,
            Schema = schema,
            Body = body,
            SqlUp = sqlUp,
        };

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql()
    {
        var view = MakeView(name: "my_view", body: "SELECT 1");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Equal(
            "CREATE OR REPLACE VIEW \"my_view\" AS\nSELECT 1;".ReplaceLineEndings(),
            sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainCreateOrReplaceView()
    {
        var view = MakeView();

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("CREATE OR REPLACE VIEW", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainBody()
    {
        var view = MakeView(body: "SELECT id, name FROM users");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("SELECT id, name FROM users", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_QuoteViewName()
    {
        var view = MakeView(name: "ActiveUsers");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("\"ActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateCreateSql_WithSchema_Should_ContainQualifiedViewName()
    {
        var view = MakeView(name: "active_users", schema: "public");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("\"public\".\"active_users\"", sql);
    }

    [Fact]
    public void GenerateCreateSql_WithoutSchema_Should_ContainUnqualifiedViewName()
    {
        var view = MakeView(name: "active_users", schema: null);

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("\"active_users\"", sql);
        Assert.DoesNotContain("null", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_ReturnDropViewIfExists()
    {
        var view = MakeView(name: "my_view");

        var sql = _generator.GenerateDeleteSql(view);

        Assert.Equal("DROP VIEW IF EXISTS \"my_view\";", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_QuoteViewName()
    {
        var view = MakeView(name: "ActiveUsers");

        var sql = _generator.GenerateDeleteSql(view);

        Assert.Contains("\"ActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateDropSql_WithSchema_Should_ContainQualifiedViewName()
    {
        var view = MakeView(name: "active_users", schema: "public");

        var sql = _generator.GenerateDeleteSql(view);

        Assert.Equal("DROP VIEW IF EXISTS \"public\".\"active_users\";", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql_WhenSqlUpProvided()
    {
        var view = MakeView(sqlUp: "CREATE OR ALTER VIEW my_func() RETURN 1;");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Equal("CREATE OR ALTER VIEW my_func() RETURN 1;".ReplaceLineEndings(), sql.ReplaceLineEndings());
    }

    [Fact]
    public void GenerateCreateSql_Should_NotNormalizeCrLfToLf()
    {
        var view = MakeView(body: "SELECT\r\n1");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("SELECT\r\n1", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainsFullBody_WhenBodyIsRawString()
    {
        const string body = """
                            SELECT
                                id,
                                name
                            FROM users
                            WHERE active = true
                            """;

        var view = MakeView(body: body);

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains(body, sql);
    }
}

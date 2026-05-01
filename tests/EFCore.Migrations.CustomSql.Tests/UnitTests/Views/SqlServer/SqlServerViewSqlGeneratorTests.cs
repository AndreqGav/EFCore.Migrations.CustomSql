using EFCore.Migrations.CustomSql.SqlObjects.Views;
using EFCore.Migrations.CustomSql.SqlServer.Views;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Views.SqlServer;

public class SqlServerViewSqlGeneratorTests
{
    private readonly SqlServerViewSqlGenerator _generator;

    public SqlServerViewSqlGeneratorTests()
    {
        _generator = new SqlServerViewSqlGenerator(new FakeSqlGenerationHelper());
    }

    private static ViewObject MakeView(
        string name = "my_view",
        string schema = null,
        string body = "SELECT 1")
        => new ViewObject
        {
            Name = name, Schema = schema, Body = body,
        };

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql()
    {
        var view = MakeView(name: "my_view", body: "SELECT 1");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Equal(
            "CREATE OR ALTER VIEW \"my_view\" AS\nSELECT 1;".ReplaceLineEndings(),
            sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainCreateOrAlterView()
    {
        var view = MakeView();

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("CREATE OR ALTER VIEW", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainBody()
    {
        var view = MakeView(body: "SELECT id, name FROM dbo.Users");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("SELECT id, name FROM dbo.Users", sql);
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
        var view = MakeView(name: "ActiveUsers", schema: "dbo");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("\"dbo\".\"ActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateCreateSql_WithoutSchema_Should_ContainUnqualifiedViewName()
    {
        var view = MakeView(name: "ActiveUsers", schema: null);

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("\"ActiveUsers\"", sql);
        Assert.DoesNotContain("null", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_ReturnDropViewIfExists()
    {
        var view = MakeView(name: "my_view");

        var sql = _generator.GenerateDropSql(view);

        Assert.Equal("DROP VIEW IF EXISTS \"my_view\";", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_QuoteViewName()
    {
        var view = MakeView(name: "ActiveUsers");

        var sql = _generator.GenerateDropSql(view);

        Assert.Contains("\"ActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateDropSql_WithSchema_Should_ContainQualifiedViewName()
    {
        var view = MakeView(name: "ActiveUsers", schema: "dbo");

        var sql = _generator.GenerateDropSql(view);

        Assert.Equal("DROP VIEW IF EXISTS \"dbo\".\"ActiveUsers\";", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_NotNormalizeCrLfToLf()
    {
        var view = MakeView(body: "SELECT\r\n1");

        var sql = _generator.GenerateCreateSql(view);

        Assert.Contains("SELECT\r\n1", sql);
    }
}

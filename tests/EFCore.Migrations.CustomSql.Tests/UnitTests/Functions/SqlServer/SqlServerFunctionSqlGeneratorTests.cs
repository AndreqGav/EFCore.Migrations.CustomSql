using System;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using EFCore.Migrations.CustomSql.SqlServer.Functions;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Functions.SqlServer;

public class SqlServerFunctionSqlGeneratorTests
{
    private readonly SqlServerFunctionSqlGenerator _generator;

    public SqlServerFunctionSqlGeneratorTests()
    {
        _generator = new SqlServerFunctionSqlGenerator(new FakeSqlGenerationHelper(), null!);
    }

    private static FunctionObject MakeFunction(
        string name = "my_func",
        string schema = null,
        string body = "RETURN 1;",
        string storeReturnType = "int",
        FunctionArgument[] args = null,
        string sqlUp = null)
        => new FunctionObject
        {
            Name = name,
            Schema = schema,
            Body = body,
            StoreReturnType = storeReturnType,
            Args = args ?? Array.Empty<FunctionArgument>(),
            SqlUp = sqlUp,
        };

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql()
    {
        var function = MakeFunction(name: "my_func", body: "RETURN 1;", storeReturnType: "int");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Equal(
            "CREATE OR ALTER FUNCTION \"my_func\"()\nRETURNS int\nAS\nBEGIN\nRETURN 1;\nEND;"
                .ReplaceLineEndings(),
            sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainCreateOrAlterFunction()
    {
        var function = MakeFunction();

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("CREATE OR ALTER FUNCTION", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainStoreReturnsType()
    {
        var function = MakeFunction(storeReturnType: "nvarchar(200)");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("RETURNS nvarchar(200)", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainBeginEnd_WhenBodyNotWrapped()
    {
        var function = MakeFunction(body: "RETURN 1;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("BEGIN", sql);
        Assert.Contains("END;", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_NotAddBeginEnd_WhenBodyStartsWithBEGIN()
    {
        var function = MakeFunction(body: "BEGIN\n  RETURN 1;\nEND;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("BEGIN\n  RETURN 1;\nEND;", sql);
        Assert.DoesNotContain("BEGIN\nBEGIN", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_NotAddBeginEnd_ForTableValuedFunction()
    {
        var function = MakeFunction(
            storeReturnType: "TABLE",
            body: "RETURN (SELECT id FROM dbo.Users);");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("RETURNS TABLE", sql);
        Assert.Contains("RETURN (SELECT id FROM dbo.Users);", sql);
        Assert.DoesNotContain("BEGIN", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainBody()
    {
        var function = MakeFunction(body: "SELECT @x + 1;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("SELECT @x + 1;", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainArgs_WhenArgsProvided()
    {
        var function = MakeFunction(args: new[]
        {
            new FunctionArgument("@userId", typeof(int), "int"), new FunctionArgument("@name", typeof(string), "nvarchar(100)"),
        });

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("@userId int", sql);
        Assert.Contains("@name nvarchar(100)", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_HaveEmptyParens_WhenNoArgs()
    {
        var function = MakeFunction(args: Array.Empty<FunctionArgument>());

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("\"my_func\"()", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_QuoteFunctionName()
    {
        var function = MakeFunction(name: "GetActiveUsers");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("\"GetActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateCreateSql_WithSchema_Should_ContainQualifiedFunctionName()
    {
        var function = MakeFunction(name: "my_func", schema: "dbo");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("\"dbo\".\"my_func\"", sql);
    }

    [Fact]
    public void GenerateCreateSql_WithoutSchema_Should_ContainUnqualifiedFunctionName()
    {
        var function = MakeFunction(name: "my_func", schema: null);

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("\"my_func\"", sql);
        Assert.DoesNotContain("null", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_ReturnDropFunctionIfExists()
    {
        var function = MakeFunction(name: "my_func");

        var sql = _generator.GenerateDeleteSql(function);

        Assert.Equal("DROP FUNCTION IF EXISTS \"my_func\";", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_QuoteFunctionName()
    {
        var function = MakeFunction(name: "GetActiveUsers");

        var sql = _generator.GenerateDeleteSql(function);

        Assert.Contains("\"GetActiveUsers\"", sql);
    }

    [Fact]
    public void GenerateDropSql_WithSchema_Should_ContainQualifiedFunctionName()
    {
        var function = MakeFunction(name: "my_func", schema: "dbo");

        var sql = _generator.GenerateDeleteSql(function);

        Assert.Equal("DROP FUNCTION IF EXISTS \"dbo\".\"my_func\";", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql_WhenSqlUpProvided()
    {
        var function = MakeFunction(sqlUp: "CREATE OR ALTER FUNCTION my_func() RETURN 1;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Equal("CREATE OR ALTER FUNCTION my_func() RETURN 1;".ReplaceLineEndings(), sql.ReplaceLineEndings());
    }

    [Fact]
    public void GenerateCreateSql_Should_NotNormalizeCrLfToLf()
    {
        var function = MakeFunction(body: "line1;\r\nline2;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("line1;\r\nline2;", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainsFullBody_WhenBodyIsRawString()
    {
        const string body = """
                            IF @userId IS NULL
                                RETURN 0;

                            RETURN 1;
                            """;

        var function = MakeFunction(body: body);

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains(body, sql);
    }
}

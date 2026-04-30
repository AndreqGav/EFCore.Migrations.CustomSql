using System;
using EFCore.Migrations.CustomSql.PostgreSQL.Functions;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.Functions;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Functions.PostgreSQL;

public class PostgreSqlFunctionSqlGeneratorTests
{
    private readonly PostgreSqlFunctionSqlGenerator _generator;

    public PostgreSqlFunctionSqlGeneratorTests()
    {
        _generator = new PostgreSqlFunctionSqlGenerator(new FakeSqlGenerationHelper(), null!);
    }

    private static FunctionObject MakeFunction(
        string name = "my_func",
        string schema = null,
        string body = "RETURN 1;",
        string storeReturnType = "integer",
        FunctionArgument[] args = null)
        => new FunctionObject
        {
            Name = name,
            Schema = schema,
            Body = body,
            StoreReturnType = storeReturnType,
            Args = args ?? Array.Empty<FunctionArgument>(),
        };

    [Fact]
    public void GenerateCreateSql_Should_ReturnExactFullSql()
    {
        var function = MakeFunction(name: "my_func", body: "RETURN 1;", storeReturnType: "integer");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Equal(
            "CREATE OR REPLACE FUNCTION \"my_func\"()\nRETURNS integer\nLANGUAGE plpgsql\nAS $$\nBEGIN\nRETURN 1;\nEND;\n$$;"
                .ReplaceLineEndings(),
            sql.ReplaceLineEndings());
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainCreateOrReplaceFunction()
    {
        var function = MakeFunction();

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("CREATE OR REPLACE FUNCTION", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainStoreReturnsType()
    {
        var function = MakeFunction(storeReturnType: "void");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("RETURNS void", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainLanguagePlpgsql()
    {
        var function = MakeFunction();

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("LANGUAGE plpgsql", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainDollarQuoteDelimiters()
    {
        var function = MakeFunction();

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("$$", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_WrapWithBeginEnd_WhenBodyNotWrapped()
    {
        var function = MakeFunction(body: "RETURN 1;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("BEGIN", sql);
        Assert.Contains("END;", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_NotAddExtraBeginEnd_WhenBodyStartsWithBEGIN()
    {
        var function = MakeFunction(body: "BEGIN RETURN 1; END;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("BEGIN RETURN 1; END;", sql);
        Assert.DoesNotContain("BEGIN\nBEGIN", sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_NotWrapBody_WhenBodyStartsWithDECLARE()
    {
        var function = MakeFunction(body: "DECLARE\n  x INTEGER;\nBEGIN\nRETURN x;\nEND;");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("DECLARE", sql);
        Assert.DoesNotContain("BEGIN\nDECLARE".ReplaceLineEndings(), sql);
    }

    [Fact]
    public void GenerateCreateSql_Should_ContainArgs_WhenArgsProvided()
    {
        var function = MakeFunction(args: new[]
        {
            new FunctionArgument("user_id", typeof(int), "integer"), new FunctionArgument("name", typeof(string), "text"),
        });

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("user_id integer", sql);
        Assert.Contains("name text", sql);
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
        var function = MakeFunction(name: "my_func", schema: "public");

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains("\"public\".\"my_func\"", sql);
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

        var sql = _generator.GenerateDropSql(function);

        Assert.Equal("DROP FUNCTION IF EXISTS \"my_func\"();", sql);
    }

    [Fact]
    public void GenerateDropSql_Should_IncludeArgsInSignature()
    {
        var function = MakeFunction(name: "my_func", args: new[]
        {
            new FunctionArgument("user_id", typeof(int), "integer"),
        });

        var sql = _generator.GenerateDropSql(function);

        Assert.Equal("DROP FUNCTION IF EXISTS \"my_func\"(user_id integer);", sql);
    }

    [Fact]
    public void GenerateDropSql_WithSchema_Should_ContainQualifiedFunctionName()
    {
        var function = MakeFunction(name: "my_func", schema: "public");

        var sql = _generator.GenerateDropSql(function);

        Assert.Equal("DROP FUNCTION IF EXISTS \"public\".\"my_func\"();", sql);
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
                            IF p_user_id IS NULL THEN
                                RETURN 0;
                            END IF;

                            RETURN 1;
                            """;

        var function = MakeFunction(body: body);

        var sql = _generator.GenerateCreateSql(function);

        Assert.Contains(body, sql);
    }
}
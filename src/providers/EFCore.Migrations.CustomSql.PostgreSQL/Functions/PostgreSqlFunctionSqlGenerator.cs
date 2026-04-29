using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Functions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Functions;

public class PostgreSqlFunctionSqlGenerator : ISqlObjectGenerator<FunctionObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public PostgreSqlFunctionSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var args = function.Args ?? string.Empty;
        var returnType = function.ReturnType ?? "void";
        var language = function.Language ?? "plpgsql";

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR REPLACE FUNCTION {funcName}({args})");
        builder.AppendLine($"RETURNS {returnType}");
        builder.AppendLine($"LANGUAGE {language}");
        builder.AppendLine("AS $$");
        builder.AppendLine(function.Body);
        builder.Append("$$;");

        return builder.ToString();
    }

    public string GenerateDropSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var args = function.Args ?? string.Empty;

        return $"DROP FUNCTION IF EXISTS {funcName}({args});";
    }
}
using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Functions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.SqlServer.Functions;

public class SqlServerFunctionSqlGenerator : ISqlObjectGenerator<FunctionObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public SqlServerFunctionSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var args = function.Args ?? string.Empty;
        var returnType = function.ReturnType ?? "void";

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR ALTER FUNCTION {funcName}({args})");
        builder.AppendLine($"RETURNS {returnType}");
        builder.AppendLine("AS");
        builder.AppendLine("BEGIN");
        builder.AppendLine(function.Body);
        builder.Append("END;");

        return builder.ToString();
    }

    public string GenerateDropSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);

        return $"DROP FUNCTION IF EXISTS {funcName};";
    }
}
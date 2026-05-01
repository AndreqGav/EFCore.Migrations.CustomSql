using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.SqlServer.Functions;

internal class SqlServerFunctionSqlGenerator : ISqlObjectGenerator<FunctionObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public SqlServerFunctionSqlGenerator(ISqlGenerationHelper sqlGenerationHelper, IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
        _typeMappingSource = typeMappingSource;
    }

    public string GenerateCreateSql(FunctionObject function)
    {
        if (function.SqlUp is not null) return function.SqlUp;

        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var args = GenerateArgsSql(function.Args);
        var returnType = function.StoreReturnType ?? _typeMappingSource.GetMapping(function.ReturnType).StoreType;

        var isWrapped = function.Body.TrimStart().StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase);
        var isTableValued = returnType.Trim().Equals("TABLE", StringComparison.OrdinalIgnoreCase);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR ALTER FUNCTION {funcName}({args})");
        builder.AppendLine($"RETURNS {returnType}");
        builder.AppendLine("AS");

        if (isWrapped || isTableValued)
        {
            builder.Append(function.Body);
        }
        else
        {
            builder.AppendLine("BEGIN");
            builder.AppendLine(function.Body);
            builder.Append("END;");
        }

        return builder.ToString();
    }

    public string GenerateDeleteSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);

        return $"DROP FUNCTION IF EXISTS {funcName};";
    }

    private string GenerateArgsSql(IEnumerable<FunctionArgument> arguments)
    {
        if (arguments == null) return string.Empty;

        var argStrings = arguments.Select(a =>
        {
            var pgType = a.StoreType ?? _typeMappingSource.GetMapping(a.Type).StoreType;

            return $"@{a.Name} {pgType}";
        });

        return string.Join(", ", argStrings);
    }
}

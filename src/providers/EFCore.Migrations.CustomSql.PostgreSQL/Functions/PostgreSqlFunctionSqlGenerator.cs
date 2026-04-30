using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Functions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Functions;

internal class PostgreSqlFunctionSqlGenerator : ISqlObjectGenerator<FunctionObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public PostgreSqlFunctionSqlGenerator(ISqlGenerationHelper sqlGenerationHelper,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
        _typeMappingSource = typeMappingSource;
    }

    public string GenerateCreateSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var returnType = function.StoreReturnType ?? _typeMappingSource.GetMapping(function.ReturnType).StoreType;

        var args = GenerateArgsSql(function.Args);

        var isWrapped = function.Body.TrimStart().StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase) ||
                        function.Body.TrimStart().StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR REPLACE FUNCTION {funcName}({args})");
        builder.AppendLine($"RETURNS {returnType}");
        builder.AppendLine("LANGUAGE plpgsql");
        builder.AppendLine("AS $$");

        if (isWrapped)
        {
            builder.AppendLine(function.Body);
        }
        else
        {
            builder.AppendLine("BEGIN");
            builder.AppendLine(function.Body);
            builder.AppendLine("END;");
        }

        builder.Append("$$;");

        return builder.ToString();
    }

    public string GenerateDropSql(FunctionObject function)
    {
        var funcName = _sqlGenerationHelper.DelimitIdentifier(function.Name, function.Schema);
        var args = GenerateArgsSql(function.Args);

        return $"DROP FUNCTION IF EXISTS {funcName}({args});";
    }

    private string GenerateArgsSql(IEnumerable<FunctionArgument> arguments)
    {
        if (arguments == null) return string.Empty;

        var argStrings = arguments.Select(a =>
        {
            var pgType = a.StoreType ?? _typeMappingSource.GetMapping(a.Type).StoreType;

            return $"{a.Name} {pgType}";
        });

        return string.Join(", ", argStrings);
    }
}
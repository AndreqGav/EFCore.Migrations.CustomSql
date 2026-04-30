using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Views;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Views;

internal class PostgreSqlViewSqlGenerator : ISqlObjectGenerator<ViewObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public PostgreSqlViewSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(ViewObject view)
    {
        var viewName = _sqlGenerationHelper.DelimitIdentifier(view.Name, view.Schema);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR REPLACE VIEW {viewName} AS");
        builder.Append(view.Body);
        builder.Append(';');

        return builder.ToString();
    }

    public string GenerateDropSql(ViewObject view)
    {
        var viewName = _sqlGenerationHelper.DelimitIdentifier(view.Name, view.Schema);

        return $"DROP VIEW IF EXISTS {viewName};";
    }
}
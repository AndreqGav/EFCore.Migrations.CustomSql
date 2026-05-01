using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.SqlServer.Views;

internal class SqlServerViewSqlGenerator : ISqlObjectGenerator<ViewObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public SqlServerViewSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(ViewObject view)
    {
        if (view.SqlUp is not null) return view.SqlUp;

        var viewName = _sqlGenerationHelper.DelimitIdentifier(view.Name, view.Schema);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR ALTER VIEW {viewName} AS");
        builder.Append(view.Body);

        builder.Append(";");

        return builder.ToString();
    }

    public string GenerateDeleteSql(ViewObject view)
    {
        var viewName = _sqlGenerationHelper.DelimitIdentifier(view.Name, view.Schema);

        return $"DROP VIEW IF EXISTS {viewName};";
    }
}

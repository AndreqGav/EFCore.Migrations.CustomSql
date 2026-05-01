using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.CustomSql.SqlObjects.Views;

public record ViewObject : ISqlObject
{
    public string Name { get; init; }

    public string ObjectType => CustomSqlAnnotationNames.View;

    public string Schema { get; init; }

    public string Body { get; init; }
}

using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.CustomSql.SqlObjects.Views;

public record ViewObjectBase : ISqlObject
{
    public string ObjectType => CustomSqlAnnotationNames.View;

    public string Name { get; init; }

}

public record ViewObject : ViewObjectBase
{
    public string Schema { get; init; }

    public string Body { get; init; }

    public string SqlUp { get; init; }
}

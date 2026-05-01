using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.CustomSql.SqlObjects.Triggers;

public abstract record TriggerObject : ISqlObject
{
    public string ObjectType => CustomSqlAnnotationNames.Trigger;

    public string Name { get; init; }

    public string Schema { get; init; }

    public string Table { get; init; }

    public string Body { get; init; }
}

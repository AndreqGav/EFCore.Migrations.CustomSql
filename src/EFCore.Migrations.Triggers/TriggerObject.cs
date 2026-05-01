using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.Triggers;

public abstract record TriggerObject : ISqlObject
{
    public string Name { get; init; }

    public string ObjectType => CustomSqlAnnotationNames.Trigger;

    public string Schema { get; init; }

    public string Table { get; init; }

    public string Body { get; init; }
}
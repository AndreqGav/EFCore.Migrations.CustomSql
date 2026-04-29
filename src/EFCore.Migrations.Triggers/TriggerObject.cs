using EFCore.Migrations.CustomSql.Abstractions;

namespace EFCore.Migrations.Triggers;

public abstract record TriggerObject : INamedSqlObject
{
    public string Name { get; init; }

    public string Schema { get; init; }

    public string Table { get; init; }

    public string Body { get; init; }
}
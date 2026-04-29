using EFCore.Migrations.CustomSql.Abstractions;

namespace EFCore.Migrations.Views;

public record ViewObject : INamedSqlObject
{
    public string Name { get; init; }

    public string Schema { get; init; }

    public string Body { get; init; }
}
using EFCore.Migrations.CustomSql.Abstractions;

namespace EFCore.Migrations.Functions;

public record FunctionObject : INamedSqlObject
{
    public string Name { get; init; }

    public string Schema { get; init; }

    public string Args { get; init; }

    public string ReturnType { get; init; }

    public string Body { get; init; }

    public string Language { get; init; }
}

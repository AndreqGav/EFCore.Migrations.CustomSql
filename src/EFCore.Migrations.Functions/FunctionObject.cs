using System;
using EFCore.Migrations.CustomSql.Abstractions;

namespace EFCore.Migrations.Functions;

public record FunctionObject : INamedSqlObject
{
    public string Name { get; init; }

    public string Schema { get; init; } = null;

    public FunctionArgument[] Args { get; init; } = Array.Empty<FunctionArgument>();

    public Type ReturnType { get; init; }

    public string StoreReturnType { get; init; } = null;

    public string Body { get; init; }
}

public record FunctionArgument(string Name, Type Type, string StoreType = null);
using System;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.CustomSql.SqlObjects.Functions;

public record FunctionObject : ISqlObject
{
    public string Name { get; init; }

    public string ObjectType => CustomSqlAnnotationNames.Function;

    public string Schema { get; init; } = null;

    public FunctionArgument[] Args { get; init; } = Array.Empty<FunctionArgument>();

    public Type ReturnType { get; init; }

    public string StoreReturnType { get; init; } = null;

    public string Body { get; init; }
}

public record FunctionArgument(string Name, Type Type, string StoreType = null);

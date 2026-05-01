using System;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;

namespace EFCore.Migrations.CustomSql.SqlObjects.Functions;

public abstract record FunctionObjectBase : ISqlObject
{
    public string ObjectType => CustomSqlAnnotationNames.Function;

    public string Name { get; init; }
}

public record FunctionObject : FunctionObjectBase
{
    public string Schema { get; init; } = null;

    public FunctionArgument[] Args { get; init; } = Array.Empty<FunctionArgument>();

    public Type ReturnType { get; init; }

    public string StoreReturnType { get; init; } = null;

    public string Body { get; init; }

    public string SqlUp { get; init; }
}

public record FunctionArgument(string Name, Type Type, string StoreType = null);

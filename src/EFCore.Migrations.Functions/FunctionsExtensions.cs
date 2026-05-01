using System;
using System.Linq;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class FunctionsExtensions
{
    public static void AddFunctionObject(this IConventionAnnotatableBuilder builder, FunctionObject function)
    {
        builder.AddSqlObject(function);
    }

    public static void AddFunctionObject(this ModelBuilder modelBuilder, FunctionObject function)
    {
        modelBuilder.GetInfrastructure().AddFunctionObject(function);
    }

    public static void AddFunctionObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, FunctionObject function)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().AddFunctionObject(function);
    }

    public static ModelBuilder HasFunctionSql(this ModelBuilder modelBuilder, string name, string body,
        Type returnType = null, FunctionArgument[] args = null, string schema = null)
    {
        var function = new FunctionObject
        {
            Name = name,
            Schema = schema,
            Args = args ?? Array.Empty<FunctionArgument>(),
            ReturnType = returnType ?? typeof(void),
            Body = body,
        };

        modelBuilder.AddFunctionObject(function);

        return modelBuilder;
    }

    public static DbFunctionBuilder HasBodySql(this DbFunctionBuilder builder, string body)
    {
        var conventionBuilder = builder.GetInfrastructure();
        var modelBuilder = conventionBuilder.ModelBuilder;

        var args = builder.Metadata.Parameters
            .Select(p => new FunctionArgument(p.Name, p.ClrType, p.StoreType))
            .ToArray();

        var function = new FunctionObject
        {
            Name = builder.Metadata.Name,
            Schema = builder.Metadata.Schema,
            Body = body,
            StoreReturnType = builder.Metadata.StoreType,
            ReturnType = builder.Metadata.ReturnType,
            Args = args,
        };

        modelBuilder.AddFunctionObject(function);

        return builder;
    }
}
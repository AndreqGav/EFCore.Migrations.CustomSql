using System;
using System.Linq;
using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.Functions;

public static class FunctionsExtensions
{
    public static void AddFunctionAnnotation(this IConventionAnnotatableBuilder builder, FunctionObject function)
    {
        builder.HasAnnotation($"{CustomSqlAnnotationNames.Function}:{function.Name}", function);
    }

    public static void AddFunctionAnnotation(this ModelBuilder modelBuilder, FunctionObject function)
    {
        modelBuilder.GetInfrastructure().AddFunctionAnnotation(function);
    }

    public static void AddFunctionAnnotation<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, FunctionObject function)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().AddFunctionAnnotation(function);
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

        modelBuilder.AddFunctionAnnotation(function);

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

        modelBuilder.AddFunctionAnnotation(function);

        return builder;
    }
}
using System;
using System.Linq;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using EFCore.Migrations.CustomSql.SqlObjects.Triggers;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class CustomSqlExtensions
{
    public static void HasCustomSql(this IConventionAnnotatableBuilder builder, string name, string sqlUp, string sqlDown) =>
        builder.AddRawSqlAnnotations(name, sqlUp, sqlDown);

    public static ModelBuilder HasCustomSql(this ModelBuilder modelBuilder, string name, string sqlUp, string sqlDown)
    {
        modelBuilder.GetInfrastructure().HasCustomSql(name, sqlUp, sqlDown);

        return modelBuilder;
    }

    public static EntityTypeBuilder<TEntity> HasCustomSql<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, string name,
        string sqlUp, string sqlDown)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().HasCustomSql(name, sqlUp, sqlDown);

        return entityTypeBuilder;
    }

    public static IConventionEntityTypeBuilder HasCustomSql(this IConventionEntityTypeBuilder entityTypeBuilder, string name,
        string sqlUp, string sqlDown)
    {
        ((IConventionAnnotatableBuilder)entityTypeBuilder).HasCustomSql(name, sqlUp, sqlDown);

        return entityTypeBuilder;
    }
}

public static class FunctionsExtensions
{
    internal static void AddFunctionObject(this IConventionAnnotatableBuilder builder, FunctionObject function) =>
        builder.AddSqlObject(function);

    internal static void AddFunctionObject(this ModelBuilder modelBuilder, FunctionObject function) =>
        modelBuilder.GetInfrastructure().AddFunctionObject(function);

    internal static void AddFunctionObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, FunctionObject function)
        where TEntity : class => entityTypeBuilder.GetInfrastructure().AddFunctionObject(function);

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

public static class TriggersExtensions
{
    internal static void AddTriggerObject(this IConventionAnnotatableBuilder builder, TriggerObject trigger) =>
        builder.AddSqlObject(trigger);

    internal static void AddTriggerObject(this ModelBuilder modelBuilder, TriggerObject trigger) =>
        modelBuilder.GetInfrastructure().AddTriggerObject(trigger);

    internal static void AddTriggerObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TriggerObject trigger)
        where TEntity : class => entityTypeBuilder.GetInfrastructure().AddTriggerObject(trigger);
}

public static class ViewsExtensions
{
    internal static void AddViewObject(this IConventionAnnotatableBuilder builder, ViewObject view) => builder.AddSqlObject(view);

    internal static void AddViewObject(this ModelBuilder modelBuilder, ViewObject view) =>
        modelBuilder.GetInfrastructure().AddViewObject(view);

    internal static void AddViewObject(this EntityTypeBuilder modelBuilder, ViewObject view) =>
        modelBuilder.GetInfrastructure().AddViewObject(view);

    public static ModelBuilder HasViewSql(this ModelBuilder modelBuilder, string name, string sql, string schema = null)
    {
        var view = new ViewObject
        {
            Name = name, Schema = schema, Body = sql,
        };

        modelBuilder.AddViewObject(view);

        return modelBuilder;
    }

    public static EntityTypeBuilder<TEntity> HasQuerySql<TEntity>(this EntityTypeBuilder<TEntity> builder, string sql)
        where TEntity : class
    {
        var viewName = builder.Metadata.GetViewName()
                       ?? throw new InvalidOperationException(
                           $"Entity '{typeof(TEntity).Name}' is not mapped to a view. Call ToView() first.");

        var view = new ViewObject
        {
            Name = viewName, Schema = builder.Metadata.GetViewSchema(), Body = sql,
        };

        builder.AddViewObject(view);

        return builder;
    }
}

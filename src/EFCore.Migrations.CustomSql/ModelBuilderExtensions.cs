using System;
using System.ComponentModel;
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
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void HasCustomSql(this IConventionAnnotatableBuilder builder, string name, string sqlUp, string sqlDown) =>
        builder.AddRawSqlAnnotations(name, sqlUp, sqlDown);

    public static ModelBuilder HasCustomSql(this ModelBuilder builder, string name, string sqlUp, string sqlDown)
    {
        builder.GetInfrastructure().HasCustomSql(name, sqlUp, sqlDown);

        return builder;
    }

    public static EntityTypeBuilder<TEntity> HasCustomSql<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, string name,
        string sqlUp, string sqlDown)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().HasCustomSql(name, sqlUp, sqlDown);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder HasCustomSql<TEntity>(this EntityTypeBuilder entityTypeBuilder, string name,
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

public static class FunctionExtensions
{
    internal static void AddFunctionObject(this IConventionAnnotatableBuilder builder, FunctionObject function) =>
        builder.AddSqlObject(function);

    internal static void AddFunctionObject(this ModelBuilder builder, FunctionObject function) =>
        builder.GetInfrastructure().AddFunctionObject(function);

    internal static void AddFunctionObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, FunctionObject function)
        where TEntity : class => entityTypeBuilder.GetInfrastructure().AddFunctionObject(function);

    public static DbFunctionBuilder HasCreateSql(this DbFunctionBuilder builder, string sql)
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
            StoreReturnType = builder.Metadata.StoreType,
            ReturnType = builder.Metadata.ReturnType,
            Args = args,
            SqlUp = sql,
        };

        modelBuilder.AddFunctionObject(function);

        return builder;
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

public static class TriggerExtensions
{
    internal static void AddTriggerObject(this IConventionAnnotatableBuilder builder, TriggerObject trigger) =>
        builder.AddSqlObject(trigger);

    internal static void AddTriggerObject(this ModelBuilder builder, TriggerObject trigger) =>
        builder.GetInfrastructure().AddTriggerObject(trigger);

    internal static void AddTriggerObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TriggerObject trigger)
        where TEntity : class => entityTypeBuilder.GetInfrastructure().AddTriggerObject(trigger);
}

public static class ViewExtensions
{
    internal static void AddViewObject(this IConventionAnnotatableBuilder builder, ViewObjectBase view)
        => builder.AddSqlObject(view);

    internal static void AddViewObject(this EntityTypeBuilder builder, ViewObjectBase view) =>
        builder.GetInfrastructure().AddViewObject(view);

    public static ViewBuilder<TEntity> HasCreateSql<TEntity>(this ViewBuilder<TEntity> builder, string sql)
        where TEntity : class
    {
        var view = new ViewObject
        {
            Name = builder.Name,
            Schema = builder.Schema,
            SqlUp = sql,
        };

        ((IInfrastructure<EntityTypeBuilder<TEntity>>)builder).Instance.AddViewObject(view);

        return builder;
    }

    public static ViewBuilder<TEntity> HasQuerySql<TEntity>(this ViewBuilder<TEntity> builder, string query)
        where TEntity : class
    {
        var view = new ViewObject
        {
            Name = builder.Name,
            Schema = builder.Schema,
            Body = query,
        };

        ((IInfrastructure<EntityTypeBuilder<TEntity>>)builder).Instance.AddViewObject(view);

        return builder;
    }

    [Obsolete("This method will be removed. Use ViewBuilder<TEntity>.HasCreateSql() instead.")]
    public static EntityTypeBuilder<TEntity> HasCreateSql<TEntity>(this EntityTypeBuilder<TEntity> builder, string sql)
        where TEntity : class
    {
        var view = new ViewObject
        {
            Name = builder.Metadata.GetViewName(),
            Schema = builder.Metadata.GetViewSchema(),
            SqlUp = sql,
        };

        builder.AddViewObject(view);

        return builder;
    }

    [Obsolete("This method will be removed. Use ViewBuilder<TEntity>.HasQuerySql() instead.")]
    public static EntityTypeBuilder<TEntity> HasQuerySql<TEntity>(this EntityTypeBuilder<TEntity> builder, string query)
        where TEntity : class
    {
        var view = new ViewObject
        {
            Name = builder.Metadata.GetViewName(),
            Schema = builder.Metadata.GetViewSchema(),
            Body = query,
        };

        builder.AddViewObject(view);

        return builder;
    }
}

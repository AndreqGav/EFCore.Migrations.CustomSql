using System;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class ViewsExtensions
{
    public static void AddViewObject(this IConventionAnnotatableBuilder builder, ViewObject view)
    {
        builder.AddSqlObject(view);
    }

    public static void AddViewObject(this ModelBuilder modelBuilder, ViewObject view)
    {
        modelBuilder.GetInfrastructure().AddViewObject(view);
    }

    public static void AddViewObject(this EntityTypeBuilder modelBuilder, ViewObject view)
    {
        modelBuilder.GetInfrastructure().AddViewObject(view);
    }

    public static ModelBuilder HasViewSql(this ModelBuilder modelBuilder, string name, string sql, string schema = null)
    {
        var view = new ViewObject
        {
            Name = name,
            Schema = schema,
            Body = sql,
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
            Name = viewName,
            Schema = builder.Metadata.GetViewSchema(),
            Body = sql,
        };

        builder.AddViewObject(view);

        return builder;
    }
}
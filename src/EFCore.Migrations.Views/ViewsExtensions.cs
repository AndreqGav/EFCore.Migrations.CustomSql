using System;
using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.Views;

public static class ViewsExtensions
{
    public static void AddViewAnnotation(this IConventionAnnotatableBuilder builder, ViewObject view)
    {
        builder.HasAnnotation($"{CustomSqlAnnotationNames.View}:{view.Name}", view);
    }

    public static void AddViewAnnotation(this ModelBuilder modelBuilder, ViewObject view)
    {
        modelBuilder.GetInfrastructure().AddViewAnnotation(view);
    }

    public static void AddViewAnnotation(this EntityTypeBuilder modelBuilder, ViewObject view)
    {
        modelBuilder.GetInfrastructure().AddViewAnnotation(view);
    }

    public static ModelBuilder HasViewSql(this ModelBuilder modelBuilder, string name, string sql, string schema = null)
    {
        var view = new ViewObject
        {
            Name = name,
            Schema = schema,
            Body = sql,
        };

        modelBuilder.AddViewAnnotation(view);

        return modelBuilder;
    }

    public static EntityTypeBuilder<TEntity> HasSqlQuery<TEntity>(this EntityTypeBuilder<TEntity> builder, string sql)
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

        builder.AddViewAnnotation(view);

        return builder;
    }
}
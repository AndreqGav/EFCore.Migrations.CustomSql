using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.CustomSql;

public static class ModelBuilderExtensions
{
    public static void HasCustomSql(this IConventionAnnotatableBuilder builder, string name, string sqlUp, string sqlDown)
    {
        builder.HasAnnotation(CustomSqlAnnotationNames.GetUpName(name), sqlUp);

        if (sqlDown is not null)
        {
            builder.HasAnnotation(CustomSqlAnnotationNames.GetDownName(name), sqlDown);
        }
    }

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
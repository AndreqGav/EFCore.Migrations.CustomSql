using EFCore.Migrations.CustomSql.Constants;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class TriggersExtensions
{
    public static void AddTriggerAnnotation(this IConventionAnnotatableBuilder builder, TriggerObject trigger)
    {
        builder.HasAnnotation($"{CustomSqlAnnotationNames.Trigger}:{trigger.Name}", trigger);
    }

    public static void AddTriggerAnnotation(this ModelBuilder modelBuilder, TriggerObject trigger)
    {
        modelBuilder.GetInfrastructure().AddTriggerAnnotation(trigger);
    }

    public static void AddTriggerAnnotation<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TriggerObject trigger)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().AddTriggerAnnotation(trigger);
    }
}
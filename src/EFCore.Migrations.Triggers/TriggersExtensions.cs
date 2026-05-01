using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class TriggersExtensions
{
    public static void AddTriggerObject(this IConventionAnnotatableBuilder builder, TriggerObject trigger)
    {
        builder.AddSqlObject(trigger);
    }

    public static void AddTriggerObject(this ModelBuilder modelBuilder, TriggerObject trigger)
    {
        modelBuilder.GetInfrastructure().AddTriggerObject(trigger);
    }

    public static void AddTriggerObject<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TriggerObject trigger)
        where TEntity : class
    {
        entityTypeBuilder.GetInfrastructure().AddTriggerObject(trigger);
    }
}
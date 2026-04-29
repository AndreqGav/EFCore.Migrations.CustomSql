using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.Triggers;

public static class TriggersExtensions
{
    public static void AddTriggerAnnotation<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TriggerObject trigger)
        where TEntity : class
    {
        entityTypeBuilder.HasAnnotation($"{CustomSqlAnnotationNames.Trigger}:{trigger.Name}", trigger);
    }
}
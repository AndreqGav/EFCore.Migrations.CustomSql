using EFCore.Migrations.CustomSql.SqlServer.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql.SqlServer;

public static class SqlServerTriggersExtensions
{
    /// <summary>
    /// Registers an AFTER INSERT trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF INSERT trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER UPDATE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF UPDATE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER INSERT OR UPDATE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF INSERT OR UPDATE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER INSERT OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterInsertOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrDelete, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF INSERT OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfInsertOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrDelete, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER UPDATE OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.UpdateOrDelete, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF UPDATE OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.UpdateOrDelete, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an AFTER INSERT OR UPDATE OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> AfterInsertOrUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdateOrDelete, TriggerTimeEnum.After, body);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Registers an INSTEAD OF INSERT OR UPDATE OR DELETE trigger.
    /// </summary>
    public static EntityTypeBuilder<TEntity> InsteadOfInsertOrUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdateOrDelete, TriggerTimeEnum.InsteadOf, body);

        return entityTypeBuilder;
    }

    private static void AddSqlServerTrigger<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, TriggerOperationEnum operation, TriggerTimeEnum time, string body)
        where TEntity : class
    {
        var table = entityTypeBuilder.Metadata.GetTableName();
        var schema = entityTypeBuilder.Metadata.GetSchema();

        var trigger = new SqlServerTriggerObject
        {
            Name = name,
            Schema = schema,
            Table = table,
            Operation = operation,
            Time = time,
            Body = body,
        };

        entityTypeBuilder.AddTriggerObject(trigger);
        entityTypeBuilder.Metadata.AddTrigger(name);
    }
}

using EFCore.Migrations.Triggers.Models;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Triggers;

public record PostgreSqlTriggerObject : TriggerObject
{
    public TriggerOperationEnum Operation { get; init; }

    public TriggerTimeEnum Time { get; init; }

    public ConstraintTriggerType? ConstraintType { get; init; }
}

public enum TriggerOperationEnum
{
    Insert = 1,

    Update = 2,

    Delete = 3,

    InsertOrUpdate = 4,
}

public enum TriggerTimeEnum
{
    Before = 1,

    After = 2,

    Instead = 3,
}

public enum ConstraintTriggerType
{
    /// <summary>
    /// CONSTRAINT TRIGGER ... NOT DEFERRABLE
    /// </summary>
    NotDeferrable = 1,

    /// <summary>
    /// CONSTRAINT TRIGGER ... DEFERRABLE INITIALLY IMMEDIATE
    /// </summary>
    DeferrableInitiallyImmediate = 2,

    /// <summary>
    /// CONSTRAINT TRIGGER ... DEFERRABLE INITIALLY DEFERRED
    /// </summary>
    DeferrableInitiallyDeferred = 3,
}
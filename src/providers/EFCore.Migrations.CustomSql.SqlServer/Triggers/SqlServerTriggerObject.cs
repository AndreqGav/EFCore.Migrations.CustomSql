using EFCore.Migrations.Triggers.Models;

namespace EFCore.Migrations.CustomSql.SqlServer.Triggers;

public record SqlServerTriggerObject : TriggerObject
{
    public TriggerOperationEnum Operation { get; init; }

    public TriggerTimeEnum Time { get; init; }
}

public enum TriggerOperationEnum
{
    Insert = 1,

    Update = 2,

    Delete = 3,

    InsertOrUpdate = 4,

    InsertOrDelete = 5,

    UpdateOrDelete = 6,

    InsertOrUpdateOrDelete = 7,
}

public enum TriggerTimeEnum
{
    After = 1,

    InsteadOf = 2,
}
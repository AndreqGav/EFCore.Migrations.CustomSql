using System;
using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.SqlServer.Triggers;

public class SqlServerTriggerSqlGenerator : ISqlObjectGenerator<SqlServerTriggerObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public SqlServerTriggerSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(SqlServerTriggerObject trigger)
    {
        var name = _sqlGenerationHelper.DelimitIdentifier(trigger.Name);
        var tableName = _sqlGenerationHelper.DelimitIdentifier(trigger.Table, trigger.Schema);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR ALTER TRIGGER {name}");
        builder.AppendLine($"ON {tableName}");
        builder.AppendLine($"{TimeToSql(trigger.Time)} {OperationToSql(trigger.Operation)}");
        builder.AppendLine("AS");
        builder.AppendLine("BEGIN");
        builder.AppendLine("    SET NOCOUNT ON;");
        builder.AppendLine($"{trigger.Body}");
        builder.Append("END;");

        return builder.ToString();
    }

    public string GenerateDropSql(SqlServerTriggerObject trigger)
    {
        var name = _sqlGenerationHelper.DelimitIdentifier(trigger.Name);
        return $"DROP TRIGGER {name};";
    }

    private static string TimeToSql(TriggerTimeEnum time)
    {
        return time switch
        {
            TriggerTimeEnum.After => "AFTER",
            TriggerTimeEnum.InsteadOf => "INSTEAD OF",
            _ => throw new ArgumentOutOfRangeException(nameof(time), time, null)
        };
    }

    private static string OperationToSql(TriggerOperationEnum operation)
    {
        return operation switch
        {
            TriggerOperationEnum.Insert => "INSERT",
            TriggerOperationEnum.Update => "UPDATE",
            TriggerOperationEnum.Delete => "DELETE",
            TriggerOperationEnum.InsertOrUpdate => "INSERT, UPDATE",
            TriggerOperationEnum.InsertOrDelete => "INSERT, DELETE",
            TriggerOperationEnum.UpdateOrDelete => "UPDATE, DELETE",
            TriggerOperationEnum.InsertOrUpdateOrDelete => "INSERT, UPDATE, DELETE",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}

using System;
using System.Text;
using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Triggers;

internal class PostgreSqlTriggerSqlGenerator : ISqlObjectGenerator<PostgreSqlTriggerObject>
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public PostgreSqlTriggerSqlGenerator(ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    public string GenerateCreateSql(PostgreSqlTriggerObject trigger)
    {
        var name = _sqlGenerationHelper.DelimitIdentifier(trigger.Name);
        var tableName = _sqlGenerationHelper.DelimitIdentifier(trigger.Table, trigger.Schema);

        var isWrapped = trigger.Body.TrimStart().StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase) ||
                        trigger.Body.TrimStart().StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase);

        var builder = new StringBuilder();
        builder.AppendLine($"CREATE OR REPLACE FUNCTION {name}()");
        builder.AppendLine("RETURNS trigger");
        builder.AppendLine("LANGUAGE plpgsql");
        builder.AppendLine("AS $$");

        if (isWrapped)
        {
            builder.AppendLine(trigger.Body);
        }
        else
        {
            builder.AppendLine("BEGIN");
            builder.AppendLine(trigger.Body);
            builder.AppendLine(GetResultSql(trigger.Operation));
            builder.AppendLine("END;");
        }

        builder.AppendLine("$$;");

        builder.AppendLine();

        builder.Append(trigger.ConstraintType == null ? "CREATE TRIGGER " : "CREATE CONSTRAINT TRIGGER ").AppendLine(name);
        builder.AppendLine($"{TimeToSql(trigger.Time)} {OperationToSql(trigger.Operation)} ON {tableName}");

        switch (trigger.ConstraintType)
        {
            case ConstraintTriggerType.NotDeferrable:
                builder.AppendLine("NOT DEFERRABLE");

                break;

            case ConstraintTriggerType.DeferrableInitiallyImmediate:
                builder.AppendLine("DEFERRABLE INITIALLY IMMEDIATE");

                break;

            case ConstraintTriggerType.DeferrableInitiallyDeferred:
                builder.AppendLine("DEFERRABLE INITIALLY DEFERRED");

                break;
        }

        builder.AppendLine("FOR EACH ROW");
        builder.Append($"EXECUTE FUNCTION {name}();");

        return builder.ToString();
    }

    public string GenerateDropSql(PostgreSqlTriggerObject trigger)
    {
        var name = _sqlGenerationHelper.DelimitIdentifier(trigger.Name);

        return $"DROP FUNCTION IF EXISTS {name}() CASCADE;";
    }

    private static string GetResultSql(TriggerOperationEnum operation)
    {
        return operation switch
        {
            TriggerOperationEnum.Insert => "RETURN NEW;",
            TriggerOperationEnum.Update => "RETURN NEW;",
            TriggerOperationEnum.InsertOrUpdate => "RETURN NEW;",
            TriggerOperationEnum.Delete => "RETURN OLD;",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }

    private static string TimeToSql(TriggerTimeEnum time)
    {
        return time switch
        {
            TriggerTimeEnum.Before => "BEFORE",
            TriggerTimeEnum.After => "AFTER",
            TriggerTimeEnum.Instead => "INSTEAD OF",
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
            TriggerOperationEnum.InsertOrUpdate => "INSERT OR UPDATE",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}
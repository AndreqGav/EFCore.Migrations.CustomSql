using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Migrations.Abstractions;
using EFCore.Migrations.CustomSql.Helpers;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Migrations.CustomSql;

internal class CustomSqlMigrationOperationModifier : IMigrationOperationModifier
{
    public IReadOnlyList<MigrationOperation> ModifyOperations(IReadOnlyList<MigrationOperation> operations,
        IRelationalModel source, IRelationalModel target)
    {
        var (create, delete) = CreateCustomSqlOperations(source, target);

        return delete.Concat(operations).Concat(create).ToList();
    }

    private (List<SqlOperation> create, List<SqlOperation> delete) CreateCustomSqlOperations(
        IRelationalModel source,
        IRelationalModel target)
    {
        var createOperations = new List<SqlOperation>();
        var deleteOperations = new List<SqlOperation>();

        var sourceObjects = RelationalModelHelper.GetCustomSqlObjects(source);
        var targetObjects = RelationalModelHelper.GetCustomSqlObjects(target);

        var deletedObjects = sourceObjects.Where(sa =>
            !targetObjects.Select(ta => ta.Name).Contains(sa.Name)
        );

        foreach (var sqlObject in deletedObjects)
        {
            AddToDelete(sqlObject.SqlDown);
        }

        foreach (var targetSqlObject in targetObjects)
        {
            var sourceSqlObject = sourceObjects.SingleOrDefault(sa => sa.Name == targetSqlObject.Name);

            var targetSql = targetSqlObject.SqlUp;
            var sourceSql = sourceSqlObject?.SqlUp;

            if (sourceSqlObject is not null)
            {
                if (!MultilineEquals(targetSql, sourceSql))
                {
                    AddToDelete(sourceSqlObject.SqlDown);
                    AddToCreate(targetSql);
                }
            }
            else
            {
                AddToCreate(targetSql);
            }
        }

        return (createOperations, deleteOperations);

        void AddToCreate(string sql) => AddOperation(sql, createOperations);

        void AddToDelete(string sql) => AddOperation(sql, deleteOperations);

        void AddOperation(string sql, List<SqlOperation> operations)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                operations.Add(new SqlOperation
                {
                    Sql = sql,
                });
            }
        }
    }

    private static bool MultilineEquals(string sourceString, string targetString,
        StringComparison comparisonType = StringComparison.Ordinal)
        => ReferenceEquals(sourceString, targetString)
           || sourceString is not null
           && targetString is not null
           && string.Equals(sourceString.ReplaceLineEndings(), targetString.ReplaceLineEndings(), comparisonType);
}

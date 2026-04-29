using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Migrations.CustomSql.Constants;
using EFCore.Migrations.CustomSql.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Migrations.CustomSql.Helpers;

public static class RelationalModelHelper
{
    // т.к. для каждого SQL сущесвутет две операции (Up и Down) и для текстового представление в миграции
    // они помещены в разные аннотации.
    // В этом методе они комбинируются снова вместе 
    public static IReadOnlyList<CustomSqlAnnotation> GetCustomSqlAnnotations(IRelationalModel relationalModel)
    {
        var model = relationalModel?.Model;

        if (model is null)
        {
            return new List<CustomSqlAnnotation>(0);
        }

        var annotations = GetAllAnnotations(model)
            .Where(a => a.Name.StartsWith(CustomSqlConstants.Sql))
            .ToList();

        var sqlUpAnnotation = annotations
            .Where(a => a.Name.StartsWith(CustomSqlConstants.SqlUp))
            .Select(a => new
            {
                Prefix = CustomSqlConstants.SqlUp,
                Name = a.Name[CustomSqlConstants.SqlUp.Length..],
                Sql = a.Value?.ToString()
            })
            .ToList();

        var sqlDownAnnotation = annotations
            .Where(a => a.Name.StartsWith(CustomSqlConstants.SqlDown))
            .Select(a => new
            {
                Prefix = CustomSqlConstants.SqlDown,
                Name = a.Name[CustomSqlConstants.SqlDown.Length..],
                Sql = a.Value?.ToString()
            })
            .ToList();

        if (sqlUpAnnotation.Count != sqlDownAnnotation.Count)
        {
            var upNames = sqlUpAnnotation.Select(a => a.Name).ToHashSet();
            var downNames = sqlDownAnnotation.Select(a => a.Name).ToHashSet();
            var missingDown = upNames.Except(downNames);
            var missingUp = downNames.Except(upNames);
            var details = string.Join(", ", missingDown.Select(n => $"'{n}' missing SqlDown").Concat(missingUp.Select(n => $"'{n}' missing SqlUp")));
            throw new InvalidOperationException($"Mismatch between SqlUp and SqlDown annotations: {details}");
        }

        var combinedAnnotations = sqlUpAnnotation.Concat(sqlDownAnnotation)
            .GroupBy(x => x.Name)
            .Select(g => new
            {
                Name = g.Key,
                SqlUp = g.Single(x => x.Prefix == CustomSqlConstants.SqlUp).Sql,
                SqlDown = g.Single(x => x.Prefix == CustomSqlConstants.SqlDown).Sql,
            })
            .Select(x => new CustomSqlAnnotation(x.Name, x.SqlUp, x.SqlDown));

        return combinedAnnotations.ToList();
    }

    private static IEnumerable<IAnnotation> GetAllAnnotations(IModel model)
    {
        var annotations = model.GetAnnotations();

        foreach (var entityType in model.GetEntityTypes())
        {
            var entityAnnotations = entityType.GetAnnotations();

            annotations = annotations.Concat(entityAnnotations);
        }

        return annotations;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Migrations.CustomSql.Constants;
using EFCore.Migrations.CustomSql.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Migrations.CustomSql.Helpers;

static internal class RelationalModelHelper
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
            .Where(x => x.Name.StartsWith(CustomSqlAnnotationNames.SqlPrefix))
            .ToList();

        var sqlUpModels = annotations
            .Where(x => x.Name.EndsWith(CustomSqlAnnotationNames.SqlUpSuffix))
            .Select(x => new SqlUpModel(CustomSqlAnnotationNames.GetName(x.Name), x.Value as string))
            .Cast<ISqlModel>()
            .ToList();

        var sqlDownModels = annotations
            .Where(x => x.Name.EndsWith(CustomSqlAnnotationNames.SqlDownSuffix))
            .Select(x => new SqlDownModel(CustomSqlAnnotationNames.GetName(x.Name), x.Value as string))
            .Cast<ISqlModel>()
            .ToList();

        var upNames = sqlUpModels.Select(a => a.Name).ToHashSet();
        var downNames = sqlDownModels.Select(a => a.Name).ToHashSet();
        var missingDown = upNames.Except(downNames).ToList();
        var missingUp = downNames.Except(upNames).ToList();

        if (missingDown.Count != 0 || missingUp.Count != 0)
        {
            var details = string.Join(", ",
                missingDown.Select(n => $"'{n}' missing SqlDown").Concat(missingUp.Select(n => $"'{n}' missing SqlUp")));

            throw new InvalidOperationException($"Mismatch between SqlUp and SqlDown annotations: {details}");
        }

        var customSqlAnnotations = sqlUpModels.Concat(sqlDownModels)
            .GroupBy(x => x.Name)
            .Select(g => new
            {
                Name = g.Key,
                SqlUp = g.OfType<SqlUpModel>().Single(x => x.Name == g.Key).Sql,
                SqlDown = g.OfType<SqlDownModel>().Single(x => x.Name == g.Key).Sql,
            })
            .Select(x => new CustomSqlAnnotation(x.Name, x.SqlUp, x.SqlDown));

        return customSqlAnnotations.ToList();
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
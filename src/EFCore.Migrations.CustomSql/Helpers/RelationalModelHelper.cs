using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Migrations.CustomSql.Helpers;

internal static class RelationalModelHelper
{
    /// <summary>
    /// Up and Down are stored in different annotations — here they are combined.
    /// </summary>
    public static IReadOnlyList<CustomSqlObject> GetCustomSqlObjects(IRelationalModel relationalModel)
    {
        var model = relationalModel?.Model;

        if (model is null) return new List<CustomSqlObject>(0);

        var result = new List<CustomSqlObject>();

        result.AddRange(ExtractCustomSqlObjects(model.GetAnnotations(), entityName: null));

        foreach (var entityType in model.GetEntityTypes())
        {
            result.AddRange(ExtractCustomSqlObjects(entityType.GetAnnotations(), entityName: entityType.ShortName()));
        }

        return result;
    }

    private static IEnumerable<CustomSqlObject> ExtractCustomSqlObjects(IEnumerable<IAnnotation> annotations, string entityName)
    {
        var sqlAnnotations = annotations
            .Where(x => CustomSqlAnnotationBuilder.ParseName(x.Name) is not null)
            .ToList();

        var upModels = sqlAnnotations
            .Where(x => x.Name.EndsWith(CustomSqlAnnotationNames.UpSuffix))
            .Select(x => new SqlUpModel(BuildFullName(x.Name, entityName), x.Value as string))
            .Cast<ISqlModel>()
            .ToList();

        var downModels = sqlAnnotations
            .Where(x => x.Name.EndsWith(CustomSqlAnnotationNames.DownSuffix))
            .Select(x => new SqlDownModel(BuildFullName(x.Name, entityName), x.Value as string))
            .Cast<ISqlModel>()
            .ToList();

        var upNames = upModels.Select(a => a.Name).ToHashSet();
        var downNames = downModels.Select(a => a.Name).ToHashSet();
        var missingDown = upNames.Except(downNames).ToList();
        var missingUp = downNames.Except(upNames).ToList();

        if (missingDown.Count != 0 || missingUp.Count != 0)
        {
            var details = string.Join(", ",
                missingDown.Select(n => $"'{n}' missing Down")
                    .Concat(missingUp.Select(n => $"'{n}' missing Up")));

            throw new InvalidOperationException($"Mismatch between Up and Down annotations: {details}");
        }

        return upModels.Concat(downModels)
            .GroupBy(x => x.Name)
            .Select(g => new CustomSqlObject(
                g.Key,
                g.OfType<SqlUpModel>().Single().Sql,
                g.OfType<SqlDownModel>().Single().Sql)
            );
    }

    private static string BuildFullName(string annotationKey, string entityName)
    {
        var name = CustomSqlAnnotationBuilder.ParseName(annotationKey);

        return entityName is null ? name : $"{entityName}:{name}";
    }
}

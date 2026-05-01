using System.Text.RegularExpressions;
using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.CustomSql.Annotations;

public static class CustomSqlAnnotationBuilder
{
    public static string GetUpKey(string objectType, string name)
        => $"{CustomSqlAnnotationNames.Prefix}:{objectType}:{name}:{CustomSqlAnnotationNames.UpSuffix}";

    public static string GetDownKey(string objectType, string name)
        => $"{CustomSqlAnnotationNames.Prefix}:{objectType}:{name}:{CustomSqlAnnotationNames.DownSuffix}";

    public static string GetTempKey(string objectType, string name)
        => $"{CustomSqlAnnotationNames.Prefix}:{objectType}:{name}";

    public static readonly Regex NameRegex = new("^CustomSql:(?<name>.+):(Up|Down)$");

    public static string ParseName(string annotationKey)
    {
        if (string.IsNullOrEmpty(annotationKey))
            return null;

        var match = NameRegex.Match(annotationKey);

        if (match.Success)
        {
            return match.Groups["name"].Value;
        }

        return null;
    }

    public static void AddSqlObject(this IConventionAnnotatableBuilder builder, ISqlObject obj)
    {
        var name = GetTempKey(obj.ObjectType, obj.Name);

        builder.HasAnnotation(name, obj);
    }

    public static void AddRawSqlAnnotations(this IConventionAnnotatableBuilder builder, string name, string sqlUp, string sqlDown)
        => builder.AddSqlAnnotations(name, CustomSqlAnnotationNames.Raw, sqlUp, sqlDown);

    public static void AddSqlAnnotations(this IConventionAnnotatableBuilder builder, string name, string objectType, string sqlUp,
        string sqlDown)
    {
        builder.HasAnnotation(GetUpKey(objectType, name), sqlUp);
        builder.HasAnnotation(GetDownKey(objectType, name), sqlDown);
    }
}
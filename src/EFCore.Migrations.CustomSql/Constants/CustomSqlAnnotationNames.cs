namespace EFCore.Migrations.CustomSql.Constants;

public static class CustomSqlAnnotationNames
{
    public const string SqlPrefix = "Sql:Custom:";

    public const string SqlUpSuffix = ":Up";

    public const string SqlDownSuffix = ":Down";

    public const string Trigger = "CustomSql:Trigger";

    public const string Function = "CustomSql:Function";

    public const string View = "CustomSql:View";

    public static string GetUpName(string name)
    {
        return $"{SqlPrefix}{name}{SqlUpSuffix}";
    }

    public static string GetDownName(string name)
    {
        return $"{SqlPrefix}{name}{SqlDownSuffix}";
    }

    public static string GetName(string annotation)
    {
        if (string.IsNullOrEmpty(annotation) || !annotation.StartsWith(SqlPrefix))
        {
            return null;
        }

        var startIndex = SqlPrefix.Length;

        if (annotation.EndsWith(SqlUpSuffix))
        {
            var length = annotation.Length - startIndex - SqlUpSuffix.Length;

            return annotation.Substring(startIndex, length);
        }

        if (annotation.EndsWith(SqlDownSuffix))
        {
            var length = annotation.Length - startIndex - SqlDownSuffix.Length;

            return annotation.Substring(startIndex, length);
        }

        return null;
    }
}
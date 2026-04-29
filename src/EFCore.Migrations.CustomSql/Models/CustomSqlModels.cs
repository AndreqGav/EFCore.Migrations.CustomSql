namespace EFCore.Migrations.CustomSql.Models;

public interface ISqlModel
{
    string Name { get; }

    string Sql { get; }
}

public class SqlUpModel : ISqlModel
{
    public SqlUpModel(string name, string sql)
    {
        Name = name;
        Sql = sql;
    }

    public string Name { get; }

    public string Sql { get; }
}

public class SqlDownModel : ISqlModel
{
    public SqlDownModel(string name, string sql)
    {
        Name = name;
        Sql = sql;
    }

    public string Name { get; }

    public string Sql { get; }
}

public class CustomSqlAnnotation
{
    public string Name { get; }

    public string SqlUp { get; }

    public string SqlDown { get; }

    public CustomSqlAnnotation(string name, string sqlUp, string sqlDown)
    {
        Name = name;
        SqlUp = sqlUp;
        SqlDown = sqlDown;
    }
}
namespace EFCore.Migrations.CustomSql.Models;

internal interface ISqlModel
{
    string Name { get; }

    string Sql { get; }
}

internal record SqlUpModel(string Name, string Sql) : ISqlModel;

internal record SqlDownModel(string Name, string Sql) : ISqlModel;

internal record CustomSqlObject(string Name, string SqlUp, string SqlDown);
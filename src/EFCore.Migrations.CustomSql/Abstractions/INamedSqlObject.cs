namespace EFCore.Migrations.CustomSql.Abstractions;

public interface INamedSqlObject
{
    string Name { get; }
}

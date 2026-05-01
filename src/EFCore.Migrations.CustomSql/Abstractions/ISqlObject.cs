namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObject
{
    string Name { get; }

    string ObjectType { get; }
}

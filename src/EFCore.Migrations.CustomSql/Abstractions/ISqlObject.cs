namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObject
{
    public string ObjectType { get; }

    public string Name { get; }
}

namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObject
{
    public string Name { get; }

    public string ObjectType { get; }
}

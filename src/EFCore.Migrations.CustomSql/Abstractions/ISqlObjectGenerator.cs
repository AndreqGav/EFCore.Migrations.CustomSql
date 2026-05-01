namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObjectGenerator<in T> where T : ISqlObject
{
    public string GenerateCreateSql(T obj);

    public string GenerateDropSql(T obj);
}

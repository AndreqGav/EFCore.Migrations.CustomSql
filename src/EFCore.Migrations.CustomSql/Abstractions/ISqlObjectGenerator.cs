namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObjectGenerator<in T> where T : ISqlObject
{
    string GenerateCreateSql(T obj);

    string GenerateDropSql(T obj);
}

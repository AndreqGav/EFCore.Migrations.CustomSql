namespace EFCore.Migrations.CustomSql.Abstractions;

public interface ISqlObjectGenerator<in T> where T : INamedSqlObject
{
    string GenerateCreateSql(T obj);

    string GenerateDropSql(T obj);
}

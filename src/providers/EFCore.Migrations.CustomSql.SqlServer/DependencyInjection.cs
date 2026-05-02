using EFCore.Migrations.CustomSql.SqlServer;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class SqlServerTriggersDependencyInjection
{
    /// <summary>
    /// Enables SQL Server-specific CustomSql services.
    /// </summary>
    public static CustomSqlOptionsBuilder UseSqlServer(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new SqlServerProviderExtension());

        return customSqlOptionsBuilder;
    }
}

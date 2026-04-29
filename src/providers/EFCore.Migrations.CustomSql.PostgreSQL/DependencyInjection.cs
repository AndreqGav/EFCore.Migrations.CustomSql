using EFCore.Migrations.CustomSql.PostgreSQL;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.CustomSql;

public static class DependencyInjection
{
    public static CustomSqlOptionsBuilder UseNpgsql(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new PostgreSqlProviderExtension());

        return customSqlOptionsBuilder;
    }
}
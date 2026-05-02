using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.PostgreSQL.Functions;
using EFCore.Migrations.CustomSql.PostgreSQL.Triggers;
using EFCore.Migrations.CustomSql.PostgreSQL.Views;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using EFCore.Migrations.CustomSql.SqlObjects.Triggers;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql.PostgreSQL;

public class PostgreSqlProviderExtension : CustomSqlProviderExtension
{
    /// <summary>
    /// Registers PostgreSQL conventions and SQL generators for CustomSql objects.
    /// </summary>
    public override void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, TriggerSetPlugin<PostgreSqlTriggerObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, FunctionSetPlugin<FunctionObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ViewSetPlugin<ViewObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
            {
                serviceMap.TryAddSingleton<ISqlObjectGenerator<PostgreSqlTriggerObject>, PostgreSqlTriggerSqlGenerator>();
                serviceMap.TryAddSingleton<ISqlObjectGenerator<FunctionObject>, PostgreSqlFunctionSqlGenerator>();
                serviceMap.TryAddSingleton<ISqlObjectGenerator<ViewObject>, PostgreSqlViewSqlGenerator>();
            });
    }
}

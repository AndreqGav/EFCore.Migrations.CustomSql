using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using EFCore.Migrations.CustomSql.SqlObjects.Triggers;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using EFCore.Migrations.CustomSql.SqlServer.Functions;
using EFCore.Migrations.CustomSql.SqlServer.Triggers;
using EFCore.Migrations.CustomSql.SqlServer.Views;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql.SqlServer;

public class SqlServerProviderExtension : CustomSqlProviderExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, TriggerSetPlugin<SqlServerTriggerObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ViewSetPlugin<ViewObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, FunctionSetPlugin<FunctionObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
            {
                serviceMap.TryAddSingleton<ISqlObjectGenerator<SqlServerTriggerObject>, SqlServerTriggerSqlGenerator>();
                serviceMap.TryAddSingleton<ISqlObjectGenerator<FunctionObject>, SqlServerFunctionSqlGenerator>();
                serviceMap.TryAddSingleton<ISqlObjectGenerator<ViewObject>, SqlServerViewSqlGenerator>();
            });
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql.Abstractions;

public abstract class CustomSqlProviderExtension : IDbContextOptionsExtension
{
    protected CustomSqlProviderExtension()
    {
        Info = new CustomSqlProviderExtensionInfo(this);
    }

    public DbContextOptionsExtensionInfo Info { get; }

    public abstract void ApplyServices(IServiceCollection services);

    public virtual void Validate(IDbContextOptions options)
    {
    }
}

public class CustomSqlProviderExtensionInfo : DbContextOptionsExtensionInfo
{
    public CustomSqlProviderExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    {
    }

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is CustomSqlProviderExtensionInfo;

    public override int GetServiceProviderHashCode() => 0;

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["TriggerExtension"] = "1";

    public override bool IsDatabaseProvider => false;

    public override string LogFragment => "TriggerExtension";
}

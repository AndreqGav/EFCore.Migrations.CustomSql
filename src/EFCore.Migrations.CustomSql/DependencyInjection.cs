using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.Migrations.CustomSql;

public static class DependencyInjection
{
    /// <summary>
    /// Enables CustomSql services with default configuration.
    /// </summary>
    public static TBuilder UseCustomSql<TBuilder>([NotNull] this TBuilder optionsBuilder) where TBuilder : DbContextOptionsBuilder
        => optionsBuilder.UseCustomSql(_ => { });

    /// <summary>
    /// Enables CustomSql services and applies additional configuration.
    /// </summary>
    public static TBuilder UseCustomSql<TBuilder>([NotNull] this TBuilder optionsBuilder, Action<CustomSqlOptionsBuilder> configure)
        where TBuilder : DbContextOptionsBuilder
    {
        configure.Invoke(new CustomSqlOptionsBuilder(optionsBuilder));

        var extension = optionsBuilder.Options.FindExtension<CustomSqlOptionsExtension>() ??
                        new CustomSqlOptionsExtension(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}

public interface ICustomSqlOptionsBuilder
{
    public DbContextOptionsBuilder OptionsBuilder { get; }
}

public class CustomSqlOptionsBuilder : ICustomSqlOptionsBuilder
{
    public CustomSqlOptionsBuilder(DbContextOptionsBuilder optionsBuilder) => OptionsBuilder = optionsBuilder;

    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    DbContextOptionsBuilder ICustomSqlOptionsBuilder.OptionsBuilder => OptionsBuilder;
}

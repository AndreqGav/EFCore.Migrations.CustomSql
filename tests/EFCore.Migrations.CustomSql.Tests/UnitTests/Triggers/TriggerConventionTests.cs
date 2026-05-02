using System.Linq;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.Helpers;
using EFCore.Migrations.CustomSql.SqlObjects.Triggers;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Triggers;

/// <summary>
/// Tests verify that the trigger convention correctly converts TriggerObject annotations into CustomSql SQL annotations.
/// </summary>
public class TriggerConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseCustomSql(o => o.UseFakeProvider());

        return builder.Options;
    }

    private static string GetSingleSqlUp(DbContext context)
        => RelationalModelHelper.GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context)).Single().SqlUp;

    private static string GetSingleSqlDown(DbContext context)
        => RelationalModelHelper.GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context)).Single().SqlDown;

    [Fact]
    public void SingleTrigger_Should_ProduceOneCustomSqlObject()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Single(customSqlObjects);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlUp_FromGenerator()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var sqlUp = GetSingleSqlUp(context);

        // Assert
        Assert.NotNull(sqlUp);
        Assert.NotEmpty(sqlUp);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlDown_FromGenerator()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var sqlDown = GetSingleSqlDown(context);

        // Assert
        Assert.NotNull(sqlDown);
        Assert.NotEmpty(sqlDown);
    }

    [Fact]
    public void MultipleTriggers_Should_ProduceMultipleCustomSqlObjects()
    {
        // Arrange
        using var context = new TwoTriggersContext(BuildOptions<TwoTriggersContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
    }

    [Fact]
    public void MultipleTriggers_Should_HaveSeparateCustomSqlObjects_WithDifferentNames()
    {
        // Arrange
        using var context = new TwoTriggersContext(BuildOptions<TwoTriggersContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }

    [Fact]
    public void TriggerConvention_Should_CustomSqlObject_WithTriggerTypePrefixInName()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Contains(CustomSqlAnnotationNames.Trigger, customSqlObject.Name);
    }

    [Fact]
    public void TriggerConvention_Should_StoreAnnotationKey_WithoutEntityName()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());
        var expectedKey = CustomSqlAnnotationBuilder.GetUpKey(CustomSqlAnnotationNames.Trigger, SingleTriggerContext.TriggerName);

        // Act
        var entityType = ModelAccessor.GetModel(context).GetEntityTypes()
            .Single(e => e.ClrType == typeof(Order));

        var annotation = entityType.GetAnnotations()
            .SingleOrDefault(a => a.Name == expectedKey);

        // Assert
        Assert.NotNull(annotation);
        Assert.DoesNotContain(entityType.ShortName(), annotation.Name);
    }

    [Fact]
    public void TriggerConvention_Should_CustomSqlObject_CorrectName()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());
        const string expectedName = $"Order:{CustomSqlAnnotationNames.Trigger}:{SingleTriggerContext.TriggerName}";

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Equal(expectedName, customSqlObject.Name);
    }

    [Fact]
    public void TriggerConvention_SameNameOnDifferentEntities_Should_ProduceSeparateCustomSqlObjects()
    {
        // Arrange
        using var context = new SameNameTriggerContext(BuildOptions<SameNameTriggerContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }
}

internal sealed class SingleTriggerContext : DbContext
{
    public const string TriggerName = "order_set_defaults";

    public const string TriggerBody = "PERFORM 1;";

    public DbSet<Order> Orders { get; set; }

    public SingleTriggerContext(DbContextOptions<SingleTriggerContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddTriggerObject(new FakeTriggerObject
            {
                Name = TriggerName, Table = "Orders", Body = TriggerBody,
            });
        });
    }
}

internal sealed class TwoTriggersContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public TwoTriggersContext(DbContextOptions<TwoTriggersContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddTriggerObject(new FakeTriggerObject
            {
                Name = "order_on_insert", Table = "Orders", Body = "body_a"
            });

            entity.AddTriggerObject(new FakeTriggerObject
            {
                Name = "order_on_update", Table = "Orders", Body = "body_b"
            });
        });
    }
}

internal sealed class SameNameTriggerContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public SameNameTriggerContext(DbContextOptions<SameNameTriggerContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddTriggerObject(new FakeTriggerObject
            {
                Name = "on_change", Table = "Orders", Body = "ORDER_BODY"
            });
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.AddTriggerObject(new FakeTriggerObject
            {
                Name = "on_change", Table = "Blogs", Body = "BLOG_BODY"
            });
        });
    }
}

public static class FakeDependencyInjection
{
    public static CustomSqlOptionsBuilder UseFakeProvider(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeTriggerProviderExtension());

        return customSqlOptionsBuilder;
    }
}

internal sealed class FakeTriggerProviderExtension : CustomSqlProviderExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, TriggerSetPlugin<FakeTriggerObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ISqlObjectGenerator<FakeTriggerObject>, FakeTriggerSqlGenerator>());
    }
}

internal sealed class FakeTriggerSqlGenerator : ISqlObjectGenerator<FakeTriggerObject>
{
    public string GenerateCreateSql(FakeTriggerObject obj) => "FAKE_CREATE";

    public string GenerateDeleteSql(FakeTriggerObject obj) => "FAKE_DROP";
}

internal sealed record FakeTriggerObject : TriggerObject
{
}

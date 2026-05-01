using System.Linq;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.Helpers;
using EFCore.Migrations.CustomSql.SqlObjects.Views;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Views;

/// <summary>
/// Тесты проверяют, что конвенция представлений правильно преобразует ViewObject в SQL-аннотации CustomSql.
/// </summary>
public class ViewConventionTests
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
    public void SingleView_Should_ProduceOneCustomSqlObject()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Single(customSqlObjects);
    }

    [Fact]
    public void ViewConvention_Should_StoreSqlUp_FromGenerator()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());

        // Act
        var sqlUp = GetSingleSqlUp(context);

        // Assert
        Assert.NotNull(sqlUp);
        Assert.NotEmpty(sqlUp);
    }

    [Fact]
    public void ViewConvention_Should_StoreSqlDown_FromGenerator()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());

        // Act
        var sqlDown = GetSingleSqlDown(context);

        // Assert
        Assert.NotNull(sqlDown);
        Assert.NotEmpty(sqlDown);
    }

    [Fact]
    public void MultipleViews_Should_ProduceMultipleCustomSqlObjects()
    {
        // Arrange
        using var context = new TwoViewsContext(BuildOptions<TwoViewsContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
    }

    [Fact]
    public void MultipleViews_Should_HaveSeparateCustomSqlObjects_WithDifferentNames()
    {
        // Arrange
        using var context = new TwoViewsContext(BuildOptions<TwoViewsContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }

    [Fact]
    public void ViewConvention_Should_CustomSqlObject_WithViewTypePrefixInName()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Contains(CustomSqlAnnotationNames.View, customSqlObject.Name);
    }

    [Fact]
    public void ViewConvention_Should_StoreAnnotationKey_WithoutEntityName()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());
        var expectedKey = CustomSqlAnnotationBuilder.GetUpKey(CustomSqlAnnotationNames.View, SingleViewContext.ViewName);

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
    public void ViewConvention_Should_CustomSqlObject_CorrectName()
    {
        // Arrange
        using var context = new SingleViewContext(BuildOptions<SingleViewContext>());
        const string expectedName = $"Order:{CustomSqlAnnotationNames.View}:{SingleViewContext.ViewName}";

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Equal(expectedName, customSqlObject.Name);
    }

    [Fact]
    public void ViewConvention_SameNameOnDifferentEntities_Should_ProduceSeparateCustomSqlObjects()
    {
        // Arrange
        using var context = new SameNameViewContext(BuildOptions<SameNameViewContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }
}

internal sealed class SingleViewContext : DbContext
{
    public const string ViewName = "my_view";

    public const string ViewBody = "SELECT 1;";

    public DbSet<Order> Orders { get; set; }

    public SingleViewContext(DbContextOptions<SingleViewContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddViewObject(new ViewObject
            {
                Name = ViewName,
                Body = ViewBody,
            });
        });
    }
}

internal sealed class TwoViewsContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public TwoViewsContext(DbContextOptions<TwoViewsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddViewObject(new ViewObject
            {
                Name = "my_view_a",
                Body = "body_a"
            });

            entity.AddViewObject(new ViewObject
            {
                Name = "my_view_b",
                Body = "body_b"
            });
        });
    }
}

internal sealed class SameNameViewContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public SameNameViewContext(DbContextOptions<SameNameViewContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddViewObject(new ViewObject
            {
                Name = "my_view",
                Schema = "schema_a",
                Body = "ORDER_BODY",
            });
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.AddViewObject(new ViewObject
            {
                Name = "my_view",
                Schema = "schema_b",
                Body = "BLOG_BODY"
            });
        });
    }
}

public static class FakeDependencyInjection
{
    public static CustomSqlOptionsBuilder UseFakeProvider(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeViewProviderExtension());

        return customSqlOptionsBuilder;
    }
}

internal sealed class FakeViewProviderExtension : CustomSqlProviderExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ViewSetPlugin<ViewObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ISqlObjectGenerator<ViewObject>, FakeViewSqlGenerator>());
    }
}

internal sealed class FakeViewSqlGenerator : ISqlObjectGenerator<ViewObject>
{
    public string GenerateCreateSql(ViewObject obj) => "FAKE_CREATE";

    public string GenerateDropSql(ViewObject obj) => "FAKE_DROP";
}
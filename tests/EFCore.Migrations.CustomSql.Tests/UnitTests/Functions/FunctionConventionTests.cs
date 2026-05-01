using System.Linq;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.Helpers;
using EFCore.Migrations.CustomSql.SqlObjects.Functions;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests.Functions;

/// <summary>
/// Тесты проверяют, что конвенция функций правильно преобразует FunctionObject в SQL-аннотации CustomSql.
/// </summary>
public class FunctionConventionTests
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
    public void SingleFunction_Should_ProduceOneCustomSqlObject()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Single(customSqlObjects);
    }

    [Fact]
    public void FunctionConvention_Should_StoreSqlUp_FromGenerator()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());

        // Act
        var sqlUp = GetSingleSqlUp(context);

        // Assert
        Assert.NotNull(sqlUp);
        Assert.NotEmpty(sqlUp);
    }

    [Fact]
    public void FunctionConvention_Should_StoreSqlDown_FromGenerator()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());

        // Act
        var sqlDown = GetSingleSqlDown(context);

        // Assert
        Assert.NotNull(sqlDown);
        Assert.NotEmpty(sqlDown);
    }

    [Fact]
    public void MultipleFunctions_Should_ProduceMultipleCustomSqlObjects()
    {
        // Arrange
        using var context = new TwoFunctionsContext(BuildOptions<TwoFunctionsContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
    }

    [Fact]
    public void MultipleFunctions_Should_HaveSeparateCustomSqlObjects_WithDifferentNames()
    {
        // Arrange
        using var context = new TwoFunctionsContext(BuildOptions<TwoFunctionsContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }

    [Fact]
    public void FunctionConvention_Should_CustomSqlObject_WithFunctionTypePrefixInName()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Contains(CustomSqlAnnotationNames.Function, customSqlObject.Name);
    }

    [Fact]
    public void FunctionConvention_Should_StoreAnnotationKey_WithoutEntityName()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());
        var expectedKey = CustomSqlAnnotationBuilder.GetUpKey(CustomSqlAnnotationNames.Function, SingleFunctionContext.FunctionName);

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
    public void FunctionConvention_Should_CustomSqlObject_CorrectName()
    {
        // Arrange
        using var context = new SingleFunctionContext(BuildOptions<SingleFunctionContext>());
        const string expectedName = $"Order:{CustomSqlAnnotationNames.Function}:{SingleFunctionContext.FunctionName}";

        // Act
        var customSqlObject = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.Equal(expectedName, customSqlObject.Name);
    }

    [Fact]
    public void FunctionConvention_SameNameOnDifferentEntities_Should_ProduceSeparateCustomSqlObjects()
    {
        // Arrange
        using var context = new SameNameFunctionContext(BuildOptions<SameNameFunctionContext>());

        // Act
        var customSqlObjects = RelationalModelHelper
            .GetCustomSqlObjects(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, customSqlObjects.Count);
        Assert.NotEqual(customSqlObjects[0].Name, customSqlObjects[1].Name);
    }
}

internal sealed class SingleFunctionContext : DbContext
{
    public const string FunctionName = "my_function";

    public const string FunctionBody = "RETURN 1;";

    public DbSet<Order> Orders { get; set; }

    public SingleFunctionContext(DbContextOptions<SingleFunctionContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddFunctionObject(new FunctionObject
            {
                Name = FunctionName, Body = FunctionBody,
            });
        });
    }
}

internal sealed class TwoFunctionsContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public TwoFunctionsContext(DbContextOptions<TwoFunctionsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddFunctionObject(new FunctionObject
            {
                Name = "my_function_a", Body = "body_a"
            });

            entity.AddFunctionObject(new FunctionObject
            {
                Name = "my_function_b", Body = "body_b"
            });
        });
    }
}

internal sealed class SameNameFunctionContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public SameNameFunctionContext(DbContextOptions<SameNameFunctionContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddFunctionObject(new FunctionObject
            {
                Name = "my_function", Schema = "schema_a", Body = "ORDER_BODY",
            });
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.AddFunctionObject(new FunctionObject
            {
                Name = "my_function", Schema = "schema_b", Body = "BLOG_BODY"
            });
        });
    }
}

public static class FakeDependencyInjection
{
    public static CustomSqlOptionsBuilder UseFakeProvider(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeFunctionProviderExtension());

        return customSqlOptionsBuilder;
    }
}

internal sealed class FakeFunctionProviderExtension : CustomSqlProviderExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, FunctionSetPlugin<FunctionObject>>();

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ISqlObjectGenerator<FunctionObject>, FakeFunctionSqlGenerator>());
    }
}

internal sealed class FakeFunctionSqlGenerator : ISqlObjectGenerator<FunctionObject>
{
    public string GenerateCreateSql(FunctionObject obj) => "FAKE_CREATE";

    public string GenerateDeleteSql(FunctionObject obj) => "FAKE_DROP";
}

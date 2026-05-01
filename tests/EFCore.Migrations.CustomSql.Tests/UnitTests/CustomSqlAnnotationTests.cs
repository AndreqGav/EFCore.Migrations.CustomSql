using System.Linq;
using EFCore.Migrations.CustomSql.Annotations;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.UnitTests;

public class CustomSqlObjectTests
{
    internal const string SqlName = "orders_summary";

    internal const string SqlUp =
        "CREATE VIEW orders_summary AS SELECT id, number, total_amount FROM \"Orders\";";

    internal const string SqlDown = "DROP VIEW IF EXISTS orders_summary;";

    internal const string ChangedSqlUp =
        "CREATE VIEW orders_summary AS SELECT id, number, total_amount, 'v2' AS version FROM \"Orders\";";

    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>();
        builder.UseSqlite("Data Source=unit_tests.db").UseCustomSql();

        return builder.Options;
    }

    [Fact]
    public void HasCustomSql_Should_StoreRawSqlUpAnnotation_WithCorrectScript()
    {
        using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

        var annotation = ModelAccessor.GetModel(context).GetAnnotations()
            .SingleOrDefault(a => a.Name == CustomSqlAnnotationBuilder.GetUpKey(CustomSqlAnnotationNames.Raw, SqlName));

        Assert.NotNull(annotation);
        Assert.Equal(SqlUp, annotation.Value?.ToString());
    }

    [Fact]
    public void HasCustomSql_Should_StoreRawSqlDownAnnotation_WithCorrectScript()
    {
        using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

        var annotation = ModelAccessor.GetModel(context).GetAnnotations()
            .SingleOrDefault(a => a.Name == CustomSqlAnnotationBuilder.GetDownKey(CustomSqlAnnotationNames.Raw, SqlName));

        Assert.NotNull(annotation);
        Assert.Equal(SqlDown, annotation.Value?.ToString());
    }

    [Theory]
    [InlineData(CustomSqlAnnotationNames.View, "my_view", "CustomSql:View:my_view:Up")]
    [InlineData(CustomSqlAnnotationNames.Function, "get_count", "CustomSql:Function:get_count:Up")]
    [InlineData(CustomSqlAnnotationNames.Trigger, "set_defaults", "CustomSql:Trigger:set_defaults:Up")]
    [InlineData(CustomSqlAnnotationNames.Raw, "my_raw", "CustomSql:Raw:my_raw:Up")]
    public void GetUpKey_Should_ProduceCorrectFormat(string objectType, string name, string expected)
    {
        Assert.Equal(expected, CustomSqlAnnotationBuilder.GetUpKey(objectType, name));
    }

    [Theory]
    [InlineData(CustomSqlAnnotationNames.View, "my_view", "CustomSql:View:my_view:Down")]
    [InlineData(CustomSqlAnnotationNames.Function, "get_count", "CustomSql:Function:get_count:Down")]
    [InlineData(CustomSqlAnnotationNames.Trigger, "set_defaults", "CustomSql:Trigger:set_defaults:Down")]
    [InlineData(CustomSqlAnnotationNames.Raw, "my_raw", "CustomSql:Raw:my_raw:Down")]
    public void GetDownKey_Should_ProduceCorrectFormat(string objectType, string name, string expected)
    {
        Assert.Equal(expected, CustomSqlAnnotationBuilder.GetDownKey(objectType, name));
    }

    [Theory]
    [InlineData(CustomSqlAnnotationNames.View, "my_view", "CustomSql:View:my_view")]
    [InlineData(CustomSqlAnnotationNames.Trigger, "set_defaults", "CustomSql:Trigger:set_defaults")]
    [InlineData(CustomSqlAnnotationNames.Raw, "my_raw", "CustomSql:Raw:my_raw")]
    public void GetTempKey_Should_ProduceCorrectFormat(string objectType, string name, string expected)
    {
        Assert.Equal(expected, CustomSqlAnnotationBuilder.GetTempKey(objectType, name));
    }

    [Theory]
    [InlineData("CustomSql:View:my_view:Up", "View:my_view")]
    [InlineData("CustomSql:Function:get_count:Up", "Function:get_count")]
    [InlineData("CustomSql:Trigger:Orders:set_defaults:Up", "Trigger:Orders:set_defaults")]
    [InlineData("CustomSql:Raw:my_raw:Up", "Raw:my_raw")]
    [InlineData("CustomSql:View:my_view:Down", "View:my_view")]
    [InlineData("CustomSql:Trigger:Orders:set_defaults:Down", "Trigger:Orders:set_defaults")]
    public void ParseName_Should_ExtractNameFromAnnotationKey(string annotationKey, string expected)
    {
        Assert.Equal(expected, CustomSqlAnnotationBuilder.ParseName(annotationKey));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SomeOtherAnnotation:foo:Up")]
    public void ParseName_Should_ReturnNull_ForInvalidAnnotation(string annotationKey)
    {
        Assert.Null(CustomSqlAnnotationBuilder.ParseName(annotationKey));
    }

    internal sealed class CustomSqlContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public CustomSqlContext(DbContextOptions<CustomSqlContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCustomSql(SqlName, SqlUp, SqlDown);
        }
    }
}

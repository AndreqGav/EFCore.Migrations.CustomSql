using System;
using EFCore.Migrations.CustomSql.SqlServer;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.IntegrationTests.SqlServer;

[Collection("SqlServer Database tests")]
public class SqlServerIntegrationTests : IDisposable
{
    private readonly SqlServerTestDbContext _context;

    public SqlServerIntegrationTests()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerTestDbContext>()
            .UseSqlServer(SqlServerTestDatabase.ConnectionString)
            .UseCustomSql(o => o.UseSqlServer());

        _context = new SqlServerTestDbContext(optionsBuilder.Options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateView()
    {
        // Arrange
        var script = _context.Database.GenerateCreateScript();

        // Act & Assert
        Assert.Contains("CREATE VIEW blog_view", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrAlterProcedure()
    {
        // Arrange
        var script = _context.Database.GenerateCreateScript();

        // Act & Assert
        Assert.Contains("CREATE OR ALTER PROCEDURE", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrAlterTrigger()
    {
        // Arrange
        var script = _context.Database.GenerateCreateScript();

        // Act & Assert
        Assert.Contains("CREATE OR ALTER TRIGGER", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrAlterView_ViaViewSql()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR ALTER VIEW [blog_view]", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_ScalarFunction_ViaDbFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR ALTER FUNCTION [get_one]", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_ParameterizedFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR ALTER FUNCTION [get_blog_url]", script);
    }

    [Fact]
    public void View_Should_Exist_InDatabase()
    {
        // Arrange
        var count = ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.objects WHERE name = 'blog_view' AND type = 'V'");

        // Act & Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void Procedure_Should_Exist_InDatabase()
    {
        // Arrange
        var count = ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.objects WHERE name = 'get_blog_name' AND type = 'P'");

        // Act & Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void Trigger_Should_Exist_InDatabase()
    {
        // Arrange
        var count = ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.triggers WHERE name = 'trg_order_set_confirmed'");

        // Act & Assert
        Assert.Equal(1, count);
    }

    private T ExecuteScalar<T>(string sql)
    {
        var conn = _context.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) conn.Open();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            return (T)cmd.ExecuteScalar();
        }
        finally
        {
            if (!wasOpen) conn.Close();
        }
    }
}

internal class SqlServerTestDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public SqlServerTestDbContext(DbContextOptions<SqlServerTestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCustomSql(
            "blog_view",
            "CREATE VIEW blog_view AS SELECT * FROM [Blogs]",
            "DROP VIEW IF EXISTS blog_view");

        modelBuilder.HasCustomSql(
            "get_blog_name",
            "CREATE OR ALTER PROCEDURE [get_blog_name] @id INT AS SELECT [Name] FROM [Blogs] WHERE [Id] = @id",
            "DROP PROCEDURE IF EXISTS [get_blog_name]");

        modelBuilder
            .HasDbFunction(typeof(SqlServerFunctionsSql).GetMethod(nameof(SqlServerFunctionsSql.GetOne))!)
            .HasName("get_one")
            .HasBodySql("RETURN 1;");

        modelBuilder
            .HasDbFunction(typeof(SqlServerFunctionsSql).GetMethod(nameof(SqlServerFunctionsSql.GetBlogUrl))!)
            .HasName("get_blog_url")
            .HasBodySql("RETURN 'text';");

        modelBuilder
            .HasDbFunction(typeof(SqlServerFunctionsSql).GetMethod(nameof(SqlServerFunctionsSql.Func1))!)
            .HasName("func_1")
            .HasCreateSql("CREATE FUNCTION func_1() RETURNS integer AS BEGIN RETURN 1; END;");

        modelBuilder.Entity<Order>(entity =>
        {
            // Sets IsConfirmed = false after every insert
            entity.AfterInsert(
                "trg_order_set_confirmed",
                "UPDATE [Orders] SET [IsConfirmed] = 0 WHERE [Id] IN (SELECT [Id] FROM inserted)");

            entity.AfterInsert(
                "trg_order_prevent_negative_amount",
                "IF EXISTS (SELECT 1 FROM inserted WHERE [TotalAmount] < 0)\r\n    THROW 50001, 'Amount must not be negative', 1;");

            entity.AfterUpdate(
                "trg_order_on_update",
                "UPDATE [Orders] SET [Status] = [Status] WHERE [Id] IN (SELECT [Id] FROM inserted)");

            entity.AfterDelete(
                "trg_order_on_delete",
                "DECLARE @cnt INT; SET @cnt = (SELECT COUNT(*) FROM deleted)");
        });

        modelBuilder.Entity<BlogView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("blog_view", o =>
                o.HasQuerySql("SELECT * FROM [Blogs]")
            );
        });


        modelBuilder.Entity<OrderCatalogView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("order_catalog_view", o =>
                o.HasCreateSql("CREATE VIEW [order_catalog_view] AS SELECT * FROM [Orders]")
            );
        });
    }
}

internal static class SqlServerFunctionsSql
{
    public static int GetOne() => throw new InvalidOperationException();

    public static string GetBlogUrl(int id, string data) => throw new InvalidOperationException();

    public static int Func1() => throw new InvalidOperationException();
}

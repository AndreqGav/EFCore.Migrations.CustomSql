using System;
using EFCore.Migrations.CustomSql.PostgreSQL;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.IntegrationTests.PostgreSQL;

/// <summary>
/// Интеграционные тесты PostgreSQL.
/// </summary>
[Collection("PostgreSQL Database tests")]
public class PostgreSqlIntegrationTests : IDisposable
{
    private readonly PostgreSqlTestDbContext _context;

    public PostgreSqlIntegrationTests()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlTestDbContext>()
            .UseNpgsql(PostgreSqlDatabase.ConnectionString)
            .UseCustomSql(o => o.UseNpgsql());

        _context = new PostgreSqlTestDbContext(optionsBuilder.Options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrReplaceFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION get_blog_name", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_StringFunction_WithSingleParameter()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION func_string_id_int(id integer)", script);
        Assert.Contains("BEGIN RETURN 'text'; END;", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_IntFunction_WithoutParameters()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION func_int()", script);
        Assert.Contains("PERFORM 1;", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_ByteaFunction_WithMixedParameters()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION func_bytea(a integer, b text, c smallint, d text[])", script);
        Assert.Contains("PERFORM 1;", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_BoolFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION func_bool()", script);
        Assert.Contains("RETURN true;", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_RawFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE FUNCTION func_raw()", script);
        Assert.Contains("RETURN 'text';", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrReplaceView()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE VIEW blog_view", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateTrigger()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION", script);
        Assert.Contains("CREATE TRIGGER", script);
    }

    [Fact]
    public void Function_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_proc WHERE proname = 'get_blog_name'");

        Assert.Equal(1, count);
    }

    [Fact]
    public void View_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_views WHERE viewname = 'blog_view'");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Trigger_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_trigger WHERE tgname = 'trg_order_set_defaults'");

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

internal class PostgreSqlTestDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public PostgreSqlTestDbContext(DbContextOptions<PostgreSqlTestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCustomSql(
            "custom_blog_view",
            "CREATE VIEW custom_blog_view AS SELECT * FROM \"Blogs\"",
            "DROP VIEW IF EXISTS custom_blog_view"
        );

        modelBuilder.HasCustomSql("get_blog_name",
            "CREATE OR REPLACE FUNCTION get_blog_name(id integer)\nRETURNS text AS $$\nBEGIN\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n END;\n$$ LANGUAGE plpgsql;",
            "DROP FUNCTION IF EXISTS get_blog_name"
        );

        modelBuilder.Entity<Order>(entity =>
        {
            entity
                .BeforeInsert(
                    "trg_order_set_defaults",
                    "NEW.\"IsConfirmed\" := false;");

            entity.BeforeInsert(
                "trg_order_prevent_negative_amount",
                "IF NEW.\"TotalAmount\" < 0 THEN RAISE EXCEPTION 'amount must not be negative'; END IF;");

            entity.AfterInsert(
                "trg_order_after_insert",
                "PERFORM 1;");

            entity.AfterUpdate(
                "trg_order_after_update",
                "PERFORM 1;");
        });

        modelBuilder.HasCustomSql("order_view",
            "CREATE VIEW order_view as SELECT \"Id\", \"TotalAmount\" FROM \"Orders\"",
            "DROP VIEW order_view"
        );

        modelBuilder.Entity<OrderCatalogView>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("OrderCatalogView", o =>
                o.HasCreateSql("CREATE VIEW \"OrderCatalogView\" AS SELECT * FROM \"Orders\"")
            );
        });

        modelBuilder.Entity<BlogView>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("blog_view", o => o.HasQuerySql("SELECT * FROM \"Blogs\""));
        });

        modelBuilder
            .HasDbFunction(typeof(FunctionsSql).GetMethod(nameof(FunctionsSql.Func1))!)
            .HasName("func_string_id_int")
            .HasBodySql("BEGIN RETURN 'text'; END;");

        modelBuilder
            .HasDbFunction(typeof(FunctionsSql).GetMethod(nameof(FunctionsSql.Func2))!)
            .HasName("func_int")
            .HasBodySql("PERFORM 1;");

        modelBuilder
            .HasDbFunction(typeof(FunctionsSql).GetMethod(nameof(FunctionsSql.Func3))!)
            .HasName("func_bytea")
            .HasBodySql("PERFORM 1;");

        modelBuilder
            .HasDbFunction(typeof(FunctionsSql).GetMethod(nameof(FunctionsSql.Func4))!)
            .HasName("func_bool")
            .HasBodySql("RETURN true;");

        modelBuilder
            .HasDbFunction(typeof(FunctionsSql).GetMethod(nameof(FunctionsSql.Func5))!)
            .HasName("func_raw")
            .HasCreateSql("CREATE FUNCTION func_raw()\nRETURNS text\nLANGUAGE plpgsql AS $func$\nBEGIN\nRETURN 'text';\nEND;\n$func$\n;");
    }
}

public static class FunctionsSql
{
    public static string Func1(int id) => throw new InvalidOperationException();

    public static int Func2() => throw new InvalidOperationException();

    public static byte[] Func3(int a, string b, byte c, string[] d) => throw new InvalidOperationException();

    public static bool Func4() => throw new InvalidOperationException();

    public static string Func5() => throw new InvalidOperationException();
}

using System;
using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.CustomSql.PostgreSQL;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using EFCore.Migrations.CustomSql.Tests.Models;
using EFCore.Migrations.CustomSql.Tests.Models.Inheritance;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.CustomSql.Tests.MigrationTests.PostgreSQL;

public class PostgreSqlMigrationDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    public DbSet<BlogView> BlogViews { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(PostgreSqlDatabase.ConnectionString)
            .UseCustomSql(o => o.UseNpgsql());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureViews(modelBuilder);
        ConfigureBlogEntities(modelBuilder);
        ConfigureOrderTriggers(modelBuilder);
        ConfigureDbFunctions(modelBuilder);
        ConfigureTphInheritance(modelBuilder);
        ConfigureTptInheritance(modelBuilder);
        ConfigureTpcInheritance(modelBuilder);
    }

    private void ConfigureViews(ModelBuilder modelBuilder)
    {
        var viewSqlGenerator = new BlogViewSqlGenerator(this, modelBuilder);
        modelBuilder.HasCustomSql("blog_names", viewSqlGenerator.Create(), viewSqlGenerator.Drop());

        modelBuilder.Entity<BlogView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("blog_view")
                .HasQuerySql("SELECT * FROM \"Blogs\"");
        });
    }

    private void ConfigureBlogEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            var triggerGenerator = new BlogTriggerSqlGenerator(this, modelBuilder);

            entity.BeforeInsertOrUpdate(
                "before_insert_or_update_blog",
                triggerGenerator.GenerateTriggerBody());
        });
    }

    private static void ConfigureOrderTriggers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.BeforeInsert("set_order_defaults", "NEW.is_confirmed = false;");

            entity.BeforeUpdate(
                "prevent_update_negative_amount",
                "IF NEW.total_amount < 0 THEN RAISE EXCEPTION 'amount negative'; END IF;");
        });
    }

    private static void ConfigureDbFunctions(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDbFunction(typeof(BlogFunctionSql).GetMethod(nameof(BlogFunctionSql.GetName))!)
            .HasName("get_blog_url")
            .HasBodySql("RETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);");

        modelBuilder
            .HasCustomSql("get_blog_name", BlogFunctionSql.Up(), BlogFunctionSql.Down())
            .HasDbFunction(typeof(BlogFunctionSql).GetMethod(nameof(BlogFunctionSql.GetName))!)
            .HasName("get_blog_name");
    }

    private static void ConfigureTphInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostBase>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.UseTphMappingStrategy();
        });

        modelBuilder.Entity<PostA>(builder => builder.HasBaseType<PostBase>());
        modelBuilder.Entity<PostB>(builder => builder.HasBaseType<PostBase>());
    }

    private static void ConfigureTptInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleBase>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.UseTptMappingStrategy();
        });

        modelBuilder.Entity<ArticleA>();
        modelBuilder.Entity<ArticleB>();
    }

    private static void ConfigureTpcInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogBase>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.UseTpcMappingStrategy();
        });

        modelBuilder.Entity<BlogA>();
        modelBuilder.Entity<BlogB>();
    }
}

public static class BlogFunctionSql
{
    public static string GetName(int id) => throw new InvalidOperationException();

    public static string Up() =>
        "CREATE OR REPLACE FUNCTION GetName(id integer)\n" +
        "RETURNS text AS $$\n" +
        "BEGIN\n" +
        "RETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n" +
        " END;\n" +
        "$$ LANGUAGE plpgsql;";

    public static string Down() => "DROP FUNCTION IF EXISTS GetName";
}

public class BlogTriggerSqlGenerator : CustomSqlGenerator
{
    public BlogTriggerSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder) : base(dbContext, modelBuilder)
    {
    }

    public string GenerateTriggerBody()
    {
        var blogTable = GetTableName<Blog>();
        var nameColumn = GetColumnName<Blog>(x => x.Name);
        var urlColumn = GetColumnName<Blog>(x => x.Url);

        return
            $"IF NEW.{urlColumn} IS NOT NULL AND NEW.{urlColumn} IS DISTINCT FROM OLD.{urlColumn} THEN\n" +
            $"    RAISE EXCEPTION 'Нельзя менять URL';\n" +
            $"END IF;\n" +
            $"IF NEW.{nameColumn} IS NOT NULL THEN\n" +
            $"    UPDATE {blogTable} SET {urlColumn} = NEW.{urlColumn}\n" +
            $"    WHERE {nameColumn} = NEW.{nameColumn};\n" +
            $"END IF;";
    }
}

public class BlogViewSqlGenerator : CustomSqlGenerator
{
    public BlogViewSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder) : base(dbContext, modelBuilder)
    {
    }

    public string Create()
    {
        var blogTable = GetTableName<Blog>();
        var idColumn = GetColumnName<Blog>(e => e.Id);
        var nameColumn = GetColumnName<Blog>(e => e.Name);

        return
            "CREATE OR REPLACE VIEW public.blog_names\n" +
            $"AS SELECT {idColumn}, {nameColumn} FROM {blogTable}";
    }

    public string Drop() => "DROP VIEW IF EXISTS public.blog_names";
}

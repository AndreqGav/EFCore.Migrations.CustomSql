using System.Linq;
using EFCore.Migrations.CustomSql.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.Migrations.CustomSql.Tests.MigrationTests.PostgreSQL;

[Collection("PostgreSQL Database tests")]
public class PostgreSqlMigrationTests
{
    [Fact]
    public void Migrations_Should_Apply_Successfully_And_Database_Be_Queryable()
    {
        // Arrange
        using var context = new PostgreSqlMigrationDbContext();
        context.Database.EnsureDeleted();

        // Act
        var exception = Record.Exception(() => context.Database.Migrate());

        // Assert
        Assert.Null(exception);
        Assert.Empty(context.Database.GetPendingMigrations());
        Assert.NotEmpty(context.Database.GetAppliedMigrations());
        Assert.True(context.Database.CanConnect(), "Failed to connect to the database after migrations.");
        Assert.Equal(0, context.Blogs.Count());
        Assert.Equal(0, context.BlogViews.Count());
    }

    [Fact]
    public void Model_Should_Not_Have_Pending_Changes()
    {
        // Arrange
        using var context = new PostgreSqlMigrationDbContext();

        var differ = context.GetService<IMigrationsModelDiffer>();

        var sourceModel = GetSourceRelationalModel(context);
        var targetModel = ModelAccessor.GetRelationalModel(context);

        // Act
        var hasDifferences = differ.HasDifferences(sourceModel, targetModel);
        var differences = differ.GetDifferences(sourceModel, targetModel);

        var diffMessage = string.Empty;
        if (hasDifferences)
        {
            var diffs = differences.Select(d =>
            {
                // If this is a SqlOperation, include the SQL text itself.
                if (d is Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation sqlOp)
                {
                    return $"SqlOperation: \n{sqlOp.Sql}\n(SuppressTransaction: {sqlOp.SuppressTransaction})";
                }

                return d.GetType().Name;
            });

            diffMessage = string.Join("\n\n", diffs);
        }

        // Assert
        Assert.False(hasDifferences,
            $"Detected changes ({differences.Count}) in DbContext models without a corresponding migration.\nDetails:\n{diffMessage}\nRun 'dotnet ef migrations add'.");
    }

    private IRelationalModel GetSourceRelationalModel(PostgreSqlMigrationDbContext context)
    {
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;

        if (snapshotModel is null) return null;

        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
        snapshotModel = modelRuntimeInitializer.Initialize(snapshotModel);

        return snapshotModel.GetRelationalModel();
    }
}

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFCore.Migrations.CustomSql.Abstractions;

public class SqlObjectConvention<TSqlObject> : IModelFinalizingConvention where TSqlObject : class, INamedSqlObject
{
    private readonly ISqlObjectGenerator<TSqlObject> _generator;

    public SqlObjectConvention(ISqlObjectGenerator<TSqlObject> generator)
    {
        _generator = generator;
    }

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var modelAnnotations = modelBuilder.Metadata.GetAnnotations()
            .Where(a => a.Value is TSqlObject)
            .ToList();

        foreach (var annotation in modelAnnotations)
        {
            if (annotation.Value is not TSqlObject obj) continue;

            var sqlUp = _generator.GenerateCreateSql(obj);
            var sqlDown = _generator.GenerateDropSql(obj);

            modelBuilder.Metadata.RemoveAnnotation(annotation.Name);
            modelBuilder.HasCustomSql(obj.Name, sqlUp, sqlDown);
        }

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var entityAnnotations = entityType.GetAnnotations()
                .Where(a => a.Value is TSqlObject)
                .ToList();

            foreach (var annotation in entityAnnotations)
            {
                if (annotation.Value is not TSqlObject obj) continue;

                var sqlUp = _generator.GenerateCreateSql(obj);
                var sqlDown = _generator.GenerateDropSql(obj);

                entityType.RemoveAnnotation(annotation.Name);
                entityType.Builder.HasCustomSql(obj.Name, sqlUp, sqlDown);
            }
        }
    }
}
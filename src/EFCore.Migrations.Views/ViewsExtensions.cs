using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Views;

public static class ViewsExtensions
{
    public static ModelBuilder AddView(this ModelBuilder modelBuilder, string name, string definition, string schema = null)
    {
        var view = new ViewObject
        {
            Name = name,
            Schema = schema,
            Body = definition,
        };

        modelBuilder.HasAnnotation($"{CustomSqlAnnotationNames.View}:{view.Name}", view);

        return modelBuilder;
    }
}
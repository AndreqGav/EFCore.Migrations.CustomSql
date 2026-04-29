using EFCore.Migrations.CustomSql.Constants;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Functions;

public static class FunctionsExtensions
{
    public static ModelBuilder AddFunction(this ModelBuilder modelBuilder, string name, string body,
        string returnType = "void", string args = null, string language = "plpgsql", string schema = null)
    {
        var function = new FunctionObject
        {
            Name = name,
            Schema = schema,
            Args = args,
            ReturnType = returnType,
            Body = body,
            Language = language,
        };

        modelBuilder.AddFunction(function);

        return modelBuilder;
    }

    public static void AddFunction(this ModelBuilder modelBuilder, FunctionObject function)
    {
        modelBuilder.HasAnnotation($"{CustomSqlAnnotationNames.Trigger}:{function.Name}", function);
    }
}
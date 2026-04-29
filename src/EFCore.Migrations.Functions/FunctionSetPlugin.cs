using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.Functions;

public class FunctionSetPlugin<T> : IConventionSetPlugin where T : FunctionObject
{
    private readonly ISqlObjectGenerator<T> _sqlGenerator;

    public FunctionSetPlugin(ISqlObjectGenerator<T> sqlGenerator)
    {
        _sqlGenerator = sqlGenerator;
    }

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new SqlObjectConvention<T>(_sqlGenerator));

        return conventionSet;
    }
}
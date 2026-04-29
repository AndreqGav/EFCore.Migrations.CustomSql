using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.Views;

public class ViewSetPlugin<T> : IConventionSetPlugin where T : ViewObject
{
    private readonly ISqlObjectGenerator<T> _sqlGenerator;

    public ViewSetPlugin(ISqlObjectGenerator<T> sqlGenerator)
    {
        _sqlGenerator = sqlGenerator;
    }

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new SqlObjectConvention<T>(_sqlGenerator));

        return conventionSet;
    }
}
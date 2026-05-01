using EFCore.Migrations.CustomSql.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.CustomSql.SqlObjects.Triggers;

public class TriggerSetPlugin<T> : IConventionSetPlugin where T : TriggerObject
{
    private readonly ISqlObjectGenerator<T> _sqlGenerator;

    public TriggerSetPlugin(ISqlObjectGenerator<T> sqlGenerator) => _sqlGenerator = sqlGenerator;

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new SqlObjectConvention<T>(_sqlGenerator));

        return conventionSet;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Migrations.CustomSql.Tests.Helpers;

internal static class ModelAccessor
{
    public static IModel GetModel(DbContext context) => context.GetService<IDesignTimeModel>().Model;

    public static IRelationalModel GetRelationalModel(DbContext context) => GetModel(context).GetRelationalModel();
}

namespace EFCore.Migrations.CustomSql.Tests.Models.Inheritance;

/// <summary>
/// Abstract base type in TPC inheritance.
/// </summary>
public abstract class BlogBase
{
    /// <summary>
    /// Identifier.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Derived type A in TPC.
/// </summary>
public class BlogA : BlogBase
{
    /// <summary>
    /// Name A.
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Derived type B in TPC.
/// </summary>
public class BlogB : BlogBase
{
    /// <summary>
    /// Name B.
    /// </summary>
    public string Name { get; set; }
}

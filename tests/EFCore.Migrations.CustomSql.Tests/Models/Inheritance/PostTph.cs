namespace EFCore.Migrations.CustomSql.Tests.Models.Inheritance;

/// <summary>
/// Base type in TPH inheritance.
/// </summary>
public class PostBase
{
    /// <summary>
    /// Identifier.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Derived type A.
/// </summary>
public class PostA : PostBase
{
    /// <summary>
    /// Text A.
    /// </summary>
    public string TextA { get; set; }
}

/// <summary>
/// Derived type B.
/// </summary>
public class PostB : PostBase
{
    /// <summary>
    /// Text B.
    /// </summary>
    public string TextB { get; set; }
}

namespace EFCore.Migrations.CustomSql.Tests.Models.Inheritance;

/// <summary>
/// Base type in TPT inheritance.
/// </summary>
public class ArticleBase
{
    /// <summary>
    /// Identifier.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Derived type A in TPT.
/// </summary>
public class ArticleA : ArticleBase
{
    /// <summary>
    /// Specific content A.
    /// </summary>
    public string ContentA { get; set; }
}

/// <summary>
/// Derived type B in TPT.
/// </summary>
public class ArticleB : ArticleBase
{
    /// <summary>
    /// Specific content B.
    /// </summary>
    public string ContentB { get; set; }
}

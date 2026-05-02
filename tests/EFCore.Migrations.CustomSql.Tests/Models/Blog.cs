namespace EFCore.Migrations.CustomSql.Tests.Models;

/// <summary>
/// Blog.
/// </summary>
public class Blog
{
    /// <summary>
    /// Blog identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Blog name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Blog URL.
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// Blog view.
/// </summary>
public class BlogView
{
    /// <summary>
    /// Identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// URL.
    /// </summary>
    public string Url { get; set; }
}

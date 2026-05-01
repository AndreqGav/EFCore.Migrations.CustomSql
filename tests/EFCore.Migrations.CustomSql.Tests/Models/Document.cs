namespace EFCore.Migrations.CustomSql.Tests.Models;

/// <summary>
/// Базовая сущность.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Документ.
/// </summary>
public class Document : EntityBase
{
    /// <summary>
    /// Заголовок.
    /// </summary>
    public string Title { get; set; }
}

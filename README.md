# EFCore.Migrations.CustomSql

EF Core extension for tracking custom SQL (views, functions, triggers, or any raw SQL) in the model and auto-generating `Up`/`Down` migration code.

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| `EFCore.Migrations.CustomSql` | Core — raw SQL in migrations. No provider needed. | [![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.CustomSql)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql) |
| `EFCore.Migrations.CustomSql.PostgreSQL` | Provider for views, functions, triggers on PostgreSQL | [![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.CustomSql.PostgreSQL)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql.PostgreSQL) |
| `EFCore.Migrations.CustomSql.SqlServer` | Provider for views, functions, triggers on SQL Server | [![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.CustomSql.SqlServer)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql.SqlServer) |

---

## How it works

Custom SQL entries are stored as annotations on the EF model. When running `dotnet ef migrations add`, the differ detects changes and generates:

- **Up SQL** — executed at end of `Up`, after schema changes
- **Down SQL** — executed at start of `Down`, before schema rollback

Model snapshot stores annotation names and SQL bodies, so changes are detected on next migration.

---

## EFCore.Migrations.CustomSql

Core package. Tracks raw SQL entries in the EF model.

### Installation

```
dotnet add package EFCore.Migrations.CustomSql
```

### Registration

No provider needed. Works with any database.

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(...)
        .UseCustomSql());
```

For views, functions, and triggers — register provider inside `UseCustomSql`:

```csharp
// PostgreSQL
options.UseNpgsql(...).UseCustomSql(o => o.UseNpgsql());

// SQL Server
options.UseSqlServer(...).UseCustomSql(o => o.UseSqlServer());
```

### Basic usage

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasCustomSql(
        name: "animals_view",
        sqlUp: "CREATE VIEW animals_view AS SELECT * FROM \"Animals\"",
        sqlDown: "DROP VIEW IF EXISTS animals_view");
}
```

### Generated migration

```csharp
public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Animals", ...);

        // custom SQL runs after schema
        migrationBuilder.Sql("CREATE VIEW animals_view AS SELECT * FROM \"Animals\"");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // custom SQL runs before schema rollback
        migrationBuilder.Sql("DROP VIEW IF EXISTS animals_view");

        migrationBuilder.DropTable(name: "Animals");
    }
}
```

### Snapshot

```csharp
modelBuilder.HasAnnotation("CustomSql:Raw:animals_view:Down", "DROP VIEW IF EXISTS ...");
modelBuilder.HasAnnotation("CustomSql:Raw:animals_view:Up", "CREATE VIEW ...");
```

---

## Views

### Register view with `HasCreateSql`

`HasCreateSql(...)` expects full view creation SQL (`CREATE VIEW ...`).
The corresponding `DROP VIEW` script for `Down` is generated automatically.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AnimalView>(entity =>
    {
        entity.HasNoKey();
        entity.ToView("animals_view", o =>
            o.HasCreateSql("CREATE VIEW animals_view AS SELECT id, name FROM \"Animals\" WHERE \"IsActive\" = true")
        );
    });
}
```

### Register view with `HasQuerySql`

Use `HasQuerySql()` to define the view query body:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AnimalView>(entity =>
    {
        entity.HasNoKey();

        entity.ToView("animals_view", o =>
            o.HasQuerySql("SELECT id, name FROM \"Animals\" WHERE \"IsActive\" = true")
        );
    });
}
```

---

## Functions

### Register function with `HasCreateSql`

When you register object creation via `HasCreateSql(...)`, the package automatically generates the corresponding drop script for `Down`.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder
        .HasDbFunction(typeof(AppDbContext).GetMethod(nameof(GetAnimalCount))!)
        .HasName("get_active_count")
        .HasCreateSql("""
            CREATE FUNCTION get_active_count(type integer, is_active boolean)
            RETURNS integer
            LANGUAGE plpgsql
            AS $$
            BEGIN
                RETURN (SELECT COUNT(*) FROM "Animals" WHERE "Type" = type AND "IsActive" = is_active);
            END;
            $$;
            """);
}
```

Where the CLR method:

```csharp
public static int GetAnimalCount(int type, bool is_active) => throw new NotSupportedException();
```

### Register function with `HasBodySql`

`HasBodySql` attaches function body SQL and uses function metadata (name/args/return type) from `DbFunctionBuilder`:

The body can start with just statements — `BEGIN`/`END` is added automatically.
Or provide a full block starting with `BEGIN` or `DECLARE` to skip auto-wrapping.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // short body — BEGIN/END auto-added
    modelBuilder
        .HasDbFunction(typeof(AppDbContext).GetMethod(nameof(GetAnimalCount))!)
        .HasName("get_active_count")
        .HasBodySql("RETURN (SELECT COUNT(*) FROM \"Animals\" WHERE \"Type\" = type AND \"IsActive\" = is_active);");

    // full block — used as-is
    modelBuilder
        .HasDbFunction(typeof(AppDbContext).GetMethod(nameof(GetAnimalCount))!)
        .HasName("get_active_count")
        .HasBodySql("""
            DECLARE
                cnt integer;
            BEGIN
                SELECT COUNT(*) INTO cnt FROM "Animals" WHERE "IsActive" = true;
                RETURN cnt;
            END;
            """);
}
```

---

## Triggers

### Registration

Same as for views and functions — register provider inside `UseCustomSql`:

```csharp
// PostgreSQL
options.UseNpgsql(...).UseCustomSql(o => o.UseNpgsql());

// SQL Server
options.UseSqlServer(...).UseCustomSql(o => o.UseSqlServer());
```

### Usage (PostgreSQL)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Figure>(entity =>
    {
        entity.BeforeInsert(
            name: "set_square",
            body: "new.square = 0;");

        entity.BeforeUpdate(
            name: "prevent_negative_square",
            body: "IF new.square < 0 THEN RAISE EXCEPTION 'square must be non-negative'; END IF;");

        entity.AfterInsert(
            name: "audit_insert",
            body: "INSERT INTO audit_log(table_name, action) VALUES ('Figures', 'INSERT');");
    });
}
```

### Generated migration (PostgreSQL)

```csharp
migrationBuilder.Sql("""
    CREATE OR REPLACE FUNCTION set_square()
    RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
        new.square = 0;
        RETURN NEW;
    END;
    $$;

    CREATE TRIGGER set_square
    BEFORE INSERT ON "Figures"
    FOR EACH ROW
    EXECUTE FUNCTION set_square();
    """);
```

Rollback:

```sql
DROP FUNCTION IF EXISTS set_square() CASCADE;
```

### SQL Server triggers

Generated SQL:

```sql
CREATE OR ALTER TRIGGER [trigger_name]
ON [TableName]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- body here
END;
```

Rollback:

```sql
DROP TRIGGER IF EXISTS [trigger_name];
```

---

## Dynamic SQL with `CustomSqlGenerator`

`CustomSqlGenerator` resolves actual table/column names from EF model. Renames in model auto-update generated SQL.

```csharp
public class MyCustomSqlGenerator : CustomSqlGenerator
{
    public MyCustomSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder)
        : base(dbContext, modelBuilder)
    {
    }

    public string Up()
    {
        var table = GetTableName<Animal>();
        var species = GetColumnName<Animal>(x => x.Species);
        var type = GetColumnName<Animal>(x => x.AnimalType);

        return $"CREATE VIEW animals_species_view AS SELECT {species}, {type} FROM {table}";
    }

    public string Down() => "DROP VIEW IF EXISTS animals_species_view";
}
```

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var gen = new MyCustomSqlGenerator(this, modelBuilder);

    modelBuilder.HasCustomSql(
        name: "animals_species_view",
        sqlUp: gen.Up(),
        sqlDown: gen.Down());
}
```

Works for trigger bodies too:

```csharp
public class TriggersGenerator : CustomSqlGenerator
{
    public TriggersGenerator(DbContext dbContext, ModelBuilder modelBuilder)
        : base(dbContext, modelBuilder)
    {
    }

    public string SyncBody()
    {
        var table = GetTableName<Animal>();
        var species = GetColumnName<Animal>(x => x.Species);
        var animalType = GetColumnName<Animal>(x => x.AnimalType);

        return $"""
            IF NEW.{species} IS NOT NULL AND NEW.{species} IS DISTINCT FROM OLD.{species} THEN
                RAISE EXCEPTION 'Species cannot be changed';
            END IF;
            IF NEW.{species} IS NOT NULL THEN
                UPDATE {table} SET {animalType} = NEW.{animalType} WHERE {species} = NEW.{species};
            END IF;
            """;
    }
}
```

```csharp
entity.BeforeInsertOrUpdate(name: "sync_animal_type", body: gen.SyncBody());
```

---

## License

MIT © Andrey Gavrilov 2026

---

> Migrated from [AndreqGav/EF.Toolkits](https://github.com/AndreqGav/EF.Toolkits)

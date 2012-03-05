using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql
{
  /// <summary>
  /// A task for <see cref="Extractor"/>
  /// </summary>
  public sealed class SqlExtractionTask
  {
    public string Catalog { get; private set; }

    public string Schema { get; private set; }

    public bool AllSchemas { get { return Schema==null; } }

    // Constructors

    public SqlExtractionTask(string catalog)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog, "catalog");

      Catalog = catalog;
    }

    public SqlExtractionTask(string catalog, string schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog, "catalog");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");

      Catalog = catalog;
      Schema = schema;
    }
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.Common;

namespace Xtensive.Sql.Dom.Database.Extractor
{
  /// <summary>
  /// Builds <see cref="Model"/> by extracting the metadata from existing database.
  /// </summary>
  public abstract class SqlExtractor
  {
    private readonly SqlDriver driver;

    /// <summary>
    /// Gets the driver.
    /// </summary>
    /// <value>The driver.</value>
    protected SqlDriver Driver
    {
      get { return driver; }
    }

    /// <summary>
    /// Creates the extractor context.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="model">The model.</param>
    /// <returns></returns>
    public virtual SqlExtractorContext CreateContext(Model model, SqlConnection connection, DbTransaction transaction)
    {
      return new SqlExtractorContext(model, connection, transaction);
    }

    /// <summary>
    /// Extracts the servers.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="model">The model.</param>
    public virtual void ExtractServers(SqlExtractorContext context, Model model)
    {
      model.CreateServer(context.Connection.ConnectionInfo.Host);
    }

    /// <summary>
    /// Extracts the users.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="server">The server.</param>
    public virtual void ExtractUsers(SqlExtractorContext context, Server server)
    {
    }

    /// <summary>
    /// Extracts the catalogs.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="server">The server.</param>
    public virtual void ExtractCatalogs(SqlExtractorContext context, Server server)
    {
      server.CreateCatalog(context.Connection.ConnectionInfo.Database);
    }

    /// <summary>
    /// Extracts the schemas.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="catalog">The catalog.</param>
    public virtual void ExtractSchemas(SqlExtractorContext context, Catalog catalog)
    {
    }

    /// <summary>
    /// Extracts the assertions.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractAssertions(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the character sets.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractCharacterSets(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the collations.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractCollations(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the translations.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractTranslations(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the domains.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractDomains(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the tables.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractTables(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the indexes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractIndexes(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the views.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractViews(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the sequences.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractSequences(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the stored procedures.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractStoredProcedures(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the foreign keys.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractForeignKeys(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the foreign keys.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="catalog">The catalog.</param>
    public virtual void ExtractForeignKeys(SqlExtractorContext context, Catalog catalog)
    {
    }

    /// <summary>
    /// Extracts the unique constraints.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractUniqueConstraints(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the columns.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public virtual void ExtractColumns(SqlExtractorContext context, Schema schema)
    {
    }

    /// <summary>
    /// Extracts the default constraints.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="table">The table.</param>
    public virtual void ExtractDefaultConstraints(SqlExtractorContext context, Schema schema, Table table)
    {
    }

    /// <summary>
    /// Initialize the model
    /// </summary>
    /// <param name="context">The context.</param>
    public virtual void Initialize(SqlExtractorContext context)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlExtractor"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlExtractor(SqlDriver driver)
    {
      this.driver = driver;
    }
  }
}

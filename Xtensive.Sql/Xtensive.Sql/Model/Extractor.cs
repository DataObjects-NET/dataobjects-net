// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Builds <see cref="Catalog"/> by extracting the metadata from existing database.
  /// </summary>
  public abstract class Extractor
  {
    /// <summary>
    /// Gets the driver.
    /// </summary>
    protected SqlDriver Driver { get; private set; }

    /// <summary>
    /// Gets the connection.
    /// </summary>
    protected SqlConnection Connection { get; private set; }

    /// <summary>
    /// Gets or sets the catalog.
    /// </summary>
    protected Catalog Catalog { get; private set; }

    /// <summary>
    /// Gets or sets the schema.
    /// </summary>
    protected Schema Schema { get; private set; }

    /// <summary>
    /// Extracts all schemes from the database.
    /// </summary>
    /// <returns><see cref="Catalog"/> that holds all schemes in the database.</returns>
    public abstract Catalog ExtractCatalog();

    /// <summary>
    /// Extracts the specified schema from the database.
    /// </summary>
    /// <returns>Extracted <see cref="Schema"/> instance.</returns>
    public Schema ExtractSchema(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      Schema = Catalog.CreateSchema(name);
      Catalog.DefaultSchema = Schema;
      var result = ExtractSchema();
      Schema = null;
      return result;
    }

    /// <summary>
    /// Extracts the schema.
    /// </summary>
    protected abstract Schema ExtractSchema();

    /// <summary>
    /// Initializes the translator with specified <see cref="SqlConnection"/> and <see cref="DbTransaction"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void Initialize(SqlConnection connection)
    {
      Connection = connection;
      var url = connection.Url;
      Catalog = new Catalog(url.GetDatabase());
      Initialize();
    }

    /// <summary>
    /// Performs custom initialization.
    /// Called within <see cref="Initialize(SqlConnection, DbTransaction)"/>.
    /// </summary>
    protected virtual void Initialize()
    {
    }

    #region Helper methods

    /// <summary>
    /// Executes the reader againts the command created from the specified <paramref name="statement"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Executed reader.</returns>
    protected DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      using (var command = Connection.CreateCommand(statement))
        return command.ExecuteReader();
    }
    
    /// <summary>
    /// Executes the reader againts the command created from the specified <paramref name="commandText"/>.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <returns>Executed reader.</returns>
    protected DbDataReader ExecuteReader(string commandText)
    {
      using (var command = Connection.CreateCommand(commandText))
        return command.ExecuteReader();
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected Extractor(SqlDriver driver)
    {
      Driver = driver;
    }
  }
}
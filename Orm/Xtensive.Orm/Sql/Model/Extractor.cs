// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using Xtensive.Core;


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
    /// Extracts all schemes from the database.
    /// </summary>
    /// <param name="catalogName">Catalog to extract.</param>
    /// <returns><see cref="Catalog"/> that holds all schemes in the database.</returns>
    public abstract Catalog ExtractCatalog(string catalogName);

    /// <summary>
    /// Extracts the specified schema from the database.
    /// </summary>
    /// <returns>Extracted <see cref="Schema"/> instance.</returns>
    public abstract Schema ExtractSchema(string catalogName, string schemaName);

    /// <summary>
    /// Initializes the translator with specified <see cref="SqlConnection"/> and <see cref="DbTransaction"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void Initialize(SqlConnection connection)
    {
      Connection = connection;
      Initialize();
    }

    /// <summary>
    /// Performs custom initialization.
    /// Called within <see cref="Initialize(SqlConnection)"/>.
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
    protected virtual DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      using (var command = Connection.CreateCommand(statement))
        return command.ExecuteReader();
    }
    
    /// <summary>
    /// Executes the reader againts the command created from the specified <paramref name="commandText"/>.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <returns>Executed reader.</returns>
    protected virtual DbDataReader ExecuteReader(string commandText)
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
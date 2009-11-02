// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.Common;
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
    /// Gets the transaction.
    /// </summary>
    protected DbTransaction Transaction { get; private set; }

    /// <summary>
    /// Extracts the database catalog from server.
    /// </summary>
    /// <returns>The extracted catalog.</returns>
    public abstract Catalog Extract();

    /// <summary>
    /// Initializes the translator with specified <see cref="SqlConnection"/> and <see cref="DbTransaction"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    public void Initialize(SqlConnection connection, DbTransaction transaction)
    {
      Connection = connection;
      Transaction = transaction;
      Initialize();
    }

    /// <summary>
    /// Performs custom initialization.
    /// Called within <see cref="Initialize(SqlConnection, DbTransaction)"/>.
    /// </summary>
    protected virtual void Initialize()
    {
    }

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
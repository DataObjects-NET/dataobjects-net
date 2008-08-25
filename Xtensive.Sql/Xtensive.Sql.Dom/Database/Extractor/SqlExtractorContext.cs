// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.Common;

namespace Xtensive.Sql.Dom.Database.Extractor
{
  public class SqlExtractorContext
  {
    private readonly Model model;
    private readonly SqlConnection connection;
    private readonly DbTransaction transaction;

    /// <summary>
    /// Gets the model.
    /// </summary>
    /// <value>The model.</value>
    public Model Model
    {
      get { return model; }
    }

    /// <summary>
    /// Gets the connection.
    /// </summary>
    /// <value>The connection.</value>
    public SqlConnection Connection
    {
      get { return connection; }
    }

    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public DbTransaction Transaction
    {
      get { return transaction; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlExtractorContext"/> class.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    public SqlExtractorContext(Model model, SqlConnection connection, DbTransaction transaction)
    {
      this.connection = connection;
      this.model = model;
      this.transaction = transaction;
    }
  }
}

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Database.Extractor
{
  public class SqlExtractorContext
  {
    private readonly Model model;
    private readonly SqlConnection connection;

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
    /// Initializes a new instance of the <see cref="SqlExtractorContext"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="model">The model.</param>
    public SqlExtractorContext(SqlConnection connection, Model model)
    {
      this.connection = connection;
      this.model = model;
    }
  }
}

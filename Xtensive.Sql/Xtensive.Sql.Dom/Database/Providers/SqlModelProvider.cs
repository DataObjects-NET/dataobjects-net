// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using Xtensive.Core;
using Xtensive.Sql.Dom.Database.Extractor;

namespace Xtensive.Sql.Dom.Database.Providers
{
  /// <summary>
  /// Represents a database model provider that builds database model from database.
  /// </summary>
  public class SqlModelProvider : IModelProvider
  {
    private SqlConnection connection;

    /// <summary>
    /// Gets or sets the connection.
    /// </summary>
    /// <value>The connection.</value>
    public SqlConnection Connection
    {
      get { return connection; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        connection = value;
      }
    }

    #region IModelProvider Members

    /// <summary>
    /// Builds the database model.
    /// </summary>
    public Model Build()
    {
      SqlExtractor extractor = ((SqlDriver)connection.Driver).Extractor;
      Model model = new Model();
      SqlExtractorContext context = extractor.CreateContext(connection, model);
      bool isClosed = connection.State==ConnectionState.Closed;
      try {
        if (isClosed)
          connection.Open();
        extractor.Initialize(connection);
        extractor.ExtractServers(context, model);
        foreach (Server server in model.Servers) {
          extractor.ExtractUsers(context, server);
          extractor.ExtractCatalogs(context, server);
          foreach (Catalog catalog in server.Catalogs) {
            extractor.ExtractSchemas(context, catalog);
            foreach (Schema schema in catalog.Schemas) {
              extractor.ExtractAssertions(context, schema);
              extractor.ExtractCharacterSets(context, schema);
              extractor.ExtractCollations(context, schema);
              extractor.ExtractTranslations(context, schema);
              extractor.ExtractDomains(context, schema);
              extractor.ExtractSequences(context, schema);
              extractor.ExtractTables(context, schema);
              extractor.ExtractViews(context, schema);
              extractor.ExtractColumns(context, schema);
              extractor.ExtractUniqueConstraints(context, schema);
              extractor.ExtractIndexes(context, schema);
              extractor.ExtractStoredProcedures(context, schema);
            }
            extractor.ExtractForeignKeys(context, catalog);
          }
        }
      }
      finally {
//        model.Lock(true);
        if (isClosed && connection!=null)
          connection.Close();
      }
      return model;
    }

    /// <summary>
    /// Saves the specified model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <remarks>This method is not suppported by this type.</remarks>
    public void Save(Model model)
    {
      throw new NotSupportedException();
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlModelProvider"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public SqlModelProvider(SqlConnection connection)
    {
      Connection = connection;
    }
  }
}

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Resources;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Sql
{
  /// <summary>
  /// Declares a base functionality of any <see cref="SqlDriver"/>.
  /// </summary>
  public abstract partial class SqlDriver
  {
    /// <summary>
    /// Gets an instance that provides complete information about underlying RDBMS.
    /// <seealso cref="ServerInfo"/>
    /// </summary>
    public ServerInfo ServerInfo { get; private set; }

    /// <summary>
    /// Gets the type mappings.
    /// </summary>
    public TypeMappingCollection TypeMappings { get; private set; }

    /// <summary>
    /// Gets the <see cref="SqlTranslator"/>.
    /// </summary>
    public SqlTranslator Translator { get; private set; }

    /// <summary>
    /// Gets the data access handler.
    /// </summary>
    public TypeMapper TypeMapper { get; private set; }

    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="statement">The Sql.Dom statement.</param>
    /// <returns>Result of compilation.</returns>
    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");
      return CreateCompiler().Compile(statement, new SqlCompilerConfiguration());
    }

    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="statement">The Sql.Dom statement.</param>
    /// <param name="configuration">The options of compilation.</param>
    /// <returns>Result of compilation.</returns>
    public SqlCompilationResult Compile(ISqlCompileUnit statement, SqlCompilerConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "options");
      return CreateCompiler().Compile(statement, configuration);
    }

    /// <summary>
    /// Extracts all schemas from the database.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>
    /// <see cref="Catalog"/> that holds all schemas in the database.
    /// </returns>
    public Catalog ExtractCatalog(SqlConnection connection)
    {
      return BuildExtractor(connection).ExtractCatalog();
    }

    /// <summary>
    /// Extracts the default schema from the database.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>
    /// <see cref="Catalog"/> that holds just the default schema in the database.
    /// </returns>
    public Schema ExtractDefaultSchema(SqlConnection connection)
    {
      var url = connection.Url;
      return ExtractSchema(connection, GetDefaultSchemaName(url));
    }

    /// <summary>
    /// Extracts the specified schema from the database.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>
    /// Extracted <see cref="Schema"/> instance.
    /// </returns>
    public Schema ExtractSchema(SqlConnection connection, string schemaName)
    {
      return BuildExtractor(connection).ExtractSchema(schemaName);
    }

    /// <summary>
    /// Creates the connection from the specified connection info.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created connection.</returns>
    public abstract SqlConnection CreateConnection(UrlInfo url);

    /// <summary>
    /// Creates the connection from the specified url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created connection</returns>
    public SqlConnection CreateConnection(string url)
    {
      return CreateConnection(UrlInfo.Parse(url));
    }

    /// <summary>
    /// Gets the type of the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>Type of the exception.</returns>
    public virtual SqlExceptionType GetExceptionType(Exception exception)
    {
      return SqlExceptionType.Unknown;
    }
    
    /// <summary>
    /// Creates the SQL DOM compiler.
    /// </summary>
    /// <returns>Created compiler.</returns>
    protected abstract SqlCompiler CreateCompiler();

    /// <summary>
    /// Creates the SQL DOM translator.
    /// </summary>
    /// <returns>Created translator</returns>
    protected abstract SqlTranslator CreateTranslator();

    /// <summary>
    /// Creates the extractor.
    /// </summary>
    /// <returns>Created extractor.</returns>
    protected abstract Extractor CreateExtractor();

    /// <summary>
    /// Creates the data access handler.
    /// </summary>
    /// <returns>Created data access handler.</returns>
    protected abstract TypeMapper CreateTypeMapper();

    /// <summary>
    /// Gets the name of the default schema.
    /// </summary>
    /// <param name="url">The URL.</param>
    protected virtual string GetDefaultSchemaName(UrlInfo url)
    {
      return url.GetSchema(url.User);
    }

    #region Private / internal methods

    private void Initialize()
    {
      Translator = CreateTranslator();
      Translator.Initialize();

      TypeMapper = CreateTypeMapper();
      TypeMapper.Initialize();

      TypeMappings = new TypeMappingCollection(TypeMapper);
    }

    private Extractor BuildExtractor(SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      if (connection.Driver != this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      var extractor = CreateExtractor();
      extractor.Initialize(connection);
      return extractor;
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serverInfoProvider">The server info provider.</param>
    protected SqlDriver(ServerInfoProvider serverInfoProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(serverInfoProvider, "serverInfoProvider");
      ServerInfo = ServerInfo.Build(serverInfoProvider);
    }
  }
}
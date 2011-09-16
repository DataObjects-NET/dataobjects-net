// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Resources;
using Xtensive.Sql;

namespace Xtensive.Sql
{
  /// <summary>
  /// Declares a base functionality of any <see cref="SqlDriver"/>.
  /// </summary>
  public abstract partial class SqlDriver
  {
    private SqlDriverFactory factory;

    /// <summary>
    /// Gets an instance that provides the most essential information about underlying RDBMS.
    /// </summary>
    public CoreServerInfo CoreServerInfo { get; private set; }

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
      return BuildExtractor(connection).ExtractSchema(CoreServerInfo.DefaultSchemaName);
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
    /// Creates the connection using default connection information
    /// </summary>
    public SqlConnection CreateConnection()
    {
      return CreateConnection(CoreServerInfo.ConnectionString);
    }

    /// <summary>
    /// Creates the connection using specified connection information.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <returns></returns>
    public SqlConnection CreateConnection(ConnectionInfo connectionInfo)
    {
      var connectionString = connectionInfo.ConnectionString
        ?? factory.BuildConnectionString(connectionInfo.ConnectionUrl);
      return CreateConnection(connectionString);
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
    /// Creates the type mapper.
    /// </summary>
    /// <returns>Created type mapper.</returns>
    protected abstract TypeMapper CreateTypeMapper();

    /// <summary>
    /// Creates the server info provider.
    /// </summary>
    /// <returns>Created server info provider.</returns>
    protected abstract ServerInfoProvider CreateServerInfoProvider();

    /// <summary>
    /// Creates connection from the specified <paramref name="connectionString"/>.
    /// </summary>
    /// <param name="connectionString">Connection string</param>
    /// <returns>Created connection.</returns>
    protected abstract SqlConnection CreateConnection(string connectionString);

    protected virtual TypeMappingCollection CreateTypeMappingCollection(TypeMapper mapper)
    {
      return new TypeMappingCollection(mapper);
    }

    #region Private / internal methods

    private void Initialize()
    {
      var serverInfoProvider = CreateServerInfoProvider();
      ServerInfo = ServerInfo.Build(serverInfoProvider);

      var typeMapper = CreateTypeMapper();
      typeMapper.Initialize();
      TypeMappings = CreateTypeMappingCollection(typeMapper);

      Translator = CreateTranslator();
      Translator.Initialize();
    }

    private Extractor BuildExtractor(SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      if (connection.Driver!=this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      var extractor = CreateExtractor();
      extractor.Initialize(connection);
      return extractor;
    }

    #endregion

    protected SqlDriver(CoreServerInfo coreServerInfo)
    {
      coreServerInfo.Lock();
      CoreServerInfo = coreServerInfo;
    }
  }
}
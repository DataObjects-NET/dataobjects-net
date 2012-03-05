// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Sql
{
  /// <summary>
  /// Declares a base functionality of any <see cref="SqlDriver"/>.
  /// </summary>
  public abstract class SqlDriver
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
      ValidateCompilerConfiguration(configuration);
      return CreateCompiler().Compile(statement, configuration);
    }

    /// <summary>
    /// Extracts catalog/schemas according to the specified <paramref name="tasks"/>.
    /// </summary>
    /// <param name="connection">Extraction tasks.</param>
    /// <param name="tasks">Connection to use.</param>
    /// <returns>Extracted catalogs.</returns>
    public IEnumerable<Catalog> Extract(SqlConnection connection, IEnumerable<SqlExtractionTask> tasks)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      ArgumentValidator.EnsureArgumentNotNull(tasks, "tasks");

      if (connection.Driver!=this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);

      var taskGroups = tasks
        .GroupBy(t => t.Catalog)
        .ToDictionary(g => g.Key, g => g.ToList());

      var result = new List<Catalog>();

      foreach (var taskGroup in taskGroups) {
        var catalogName = taskGroup.Key;
        var tasksForCatalog = taskGroup.Value;
        var extractSingle = tasksForCatalog.Count==1 && !tasksForCatalog[0].AllSchemas;

        if (extractSingle) {
          // Extracting only specified schema
          var schemaName = tasksForCatalog[0].Schema;
          var schema = BuildExtractor(connection).ExtractSchema(catalogName, schemaName);
          var catalog = schema.Catalog;
          CleanSchemas(catalog, new[]{schemaName}); // Remove the rest, if any
          result.Add(catalog);
        }
        else {
          // Extracting all schemas
          var catalog = BuildExtractor(connection).ExtractCatalog(catalogName);
          var needClean = tasksForCatalog.All(t => !t.AllSchemas);
          if (needClean)
            CleanSchemas(catalog, tasksForCatalog.Select(t => t.Schema));
          result.Add(catalog);
        }
      }

      return result;
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
      var task = new SqlExtractionTask(CoreServerInfo.DatabaseName);
      return Extract(connection, new[] {task}).FirstOrDefault();
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
      return ExtractSchema(connection, CoreServerInfo.DefaultSchemaName);
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
      var task = new SqlExtractionTask(CoreServerInfo.DatabaseName, schemaName);
      return Extract(connection, new[] {task}).SelectMany(catalog => catalog.Schemas).FirstOrDefault();
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
      return CreateConnection(factory.GetConnectionString(connectionInfo));
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
    /// Gets information about exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>Information about exception.</returns>
    public virtual SqlExceptionInfo GetExceptionInfo(Exception exception)
    {
      return SqlExceptionInfo.Create(GetExceptionType(exception));
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

    internal void Initialize(SqlDriverFactory creator)
    {
      factory = creator;

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
      var extractor = CreateExtractor();
      extractor.Initialize(connection);
      return extractor;
    }

    private void ValidateCompilerConfiguration(SqlCompilerConfiguration configuration)
    {
      var supported = ServerInfo.Query.Features.Supports(QueryFeatures.MultidatabaseQueries);
      var requested = configuration.DatabaseQualifiedObjects;
      if (requested && !supported)
        throw SqlHelper.NotSupported(QueryFeatures.MultidatabaseQueries);
    }

    private void CleanSchemas(Catalog catalog, IEnumerable<string> allowedSchemas)
    {
      // We allow extractors to extract schemas that were not requested
      // After extraction is complete, this methods removes not-necessary parts

      var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var schema in allowedSchemas)
        allowed.Add(schema);

      var schemasToRemove = catalog.Schemas
        .Where(s => !allowed.Contains(s.Name))
        .ToList();

      foreach (var schema in schemasToRemove)
        catalog.Schemas.Remove(schema);
    }

    #endregion

    protected SqlDriver(CoreServerInfo coreServerInfo)
    {
      coreServerInfo.Lock();
      CoreServerInfo = coreServerInfo;
    }
  }
}
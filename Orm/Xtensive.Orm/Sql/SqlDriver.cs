// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
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
    private SqlDriverFactory origin;
    private ConnectionInfo originConnectionInfo;

    /// <summary>
    /// Gets an instance that provides the most essential information about underlying RDBMS.
    /// </summary>
    public CoreServerInfo CoreServerInfo { get; }

    /// <summary>
    /// Gets an instance that provides complete information about underlying RDBMS.
    /// <seealso cref="ServerInfo"/>
    /// </summary>
    public ServerInfo ServerInfo { get; private set; }

    /// <summary>
    /// Gets the type mappings.
    /// </summary>
    public TypeMappingRegistry TypeMappings { get; private set; }

    /// <summary>
    /// Gets the <see cref="SqlTranslator"/>.
    /// </summary>
    public SqlTranslator Translator { get; private set; }

    /// <summary>
    /// Gets connection string for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo"><see cref="ConnectionInfo"/> to convert.</param>
    /// <returns>Connection string.</returns>
    public string GetConnectionString(ConnectionInfo connectionInfo) => origin.GetConnectionString(connectionInfo);

    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="statement">The Sql.Dom statement.</param>
    /// <returns>Result of compilation.</returns>
    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, nameof(statement));
      return CreateCompiler().Compile(statement, new SqlCompilerConfiguration(), null);
    }

    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="statement">The Sql.Dom statement.</param>
    /// <param name="configuration">The options of compilation.</param>
    /// <param name="typeIdRegistry">TypeId registry.</param>
    /// <returns>Result of compilation.</returns>
    public SqlCompilationResult Compile(ISqlCompileUnit statement, SqlCompilerConfiguration configuration, TypeIdRegistry typeIdRegistry = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, nameof(statement));
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
      ValidateCompilerConfiguration(configuration);
      return CreateCompiler().Compile(statement, configuration, typeIdRegistry);
    }

    /// <summary>
    /// Gets <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection"><see cref="SqlConnection"/> to use.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    public DefaultSchemaInfo GetDefaultSchema(SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      if (connection.Driver != this) {
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      }

      return origin.GetDefaultSchema(connection.UnderlyingConnection, connection.ActiveTransaction);
    }

    /// <summary>
    /// Gets <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection"><see cref="SqlConnection"/> to use.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    public Task<DefaultSchemaInfo> GetDefaultSchemaAsync(SqlConnection connection, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      if (connection.Driver != this) {
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      }

      return origin.GetDefaultSchemaAsync(connection.UnderlyingConnection, connection.ActiveTransaction, token);
    }

    /// <summary>
    /// Extracts catalogs/schemas according to the specified <paramref name="tasks"/>.
    /// </summary>
    /// <param name="connection">Extraction tasks.</param>
    /// <param name="tasks">Connection to use.</param>
    /// <returns>Extracted catalogs.</returns>
    public SqlExtractionResult Extract(SqlConnection connection, IEnumerable<SqlExtractionTask> tasks)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      ArgumentValidator.EnsureArgumentNotNull(tasks, nameof(tasks));

      if (connection.Driver != this) {
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      }

      var taskGroups = tasks
        .GroupBy(t => t.Catalog)
        .ToDictionary(g => g.Key, g => g.ToList());

      var result = new SqlExtractionResult();

      foreach (var taskGroup in taskGroups) {
        var catalogName = taskGroup.Key;
        var tasksForCatalog = taskGroup.Value;

        if (tasksForCatalog.All(t => !t.AllSchemas)) {
          // extracting all the schemes we need
          var schemasToExtract = tasksForCatalog.Select(t => t.Schema).ToArray();
          var catalog = BuildExtractor(connection)
            .ExtractSchemes(catalogName, schemasToExtract);
          CleanSchemas(catalog, schemasToExtract);
          result.Catalogs.Add(catalog);
        }
        else {
          // Extracting whole catalog
          var catalog = BuildExtractor(connection).ExtractCatalog(catalogName);
          result.Catalogs.Add(catalog);
        }
      }

      return result;
    }

    /// <summary>
    /// Asynchronously extracts catalogs/schemas according to the specified <paramref name="tasks"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">Extraction tasks.</param>
    /// <param name="tasks">Connection to use.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Extracted catalogs.</returns>
    public async Task<SqlExtractionResult> ExtractAsync(SqlConnection connection, IEnumerable<SqlExtractionTask> tasks,
      CancellationToken token = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      ArgumentValidator.EnsureArgumentNotNull(tasks, nameof(tasks));

      if (connection.Driver != this) {
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      }

      var taskGroups = tasks
        .GroupBy(t => t.Catalog)
        .ToDictionary(g => g.Key, g => g.ToList());

      var result = new SqlExtractionResult();

      foreach (var (catalogName, sqlExtractionTasks) in taskGroups) {
        var extractor = await BuildExtractorAsync(connection, token).ConfigureAwait(false);
        if (sqlExtractionTasks.All(t => !t.AllSchemas)) {
          // extracting all the schemes we need
          var schemasToExtract = sqlExtractionTasks.Select(t => t.Schema).ToArray();
          var catalog = await extractor.ExtractSchemesAsync(catalogName, schemasToExtract, token).ConfigureAwait(false);
          CleanSchemas(catalog, schemasToExtract);
          result.Catalogs.Add(catalog);
        }
        else {
          // Extracting whole catalog
          var catalog = await extractor.ExtractCatalogAsync(catalogName, token).ConfigureAwait(false);
          result.Catalogs.Add(catalog);
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
      var defaultSchema = GetDefaultSchema(connection);
      var task = new SqlExtractionTask(defaultSchema.Database);
      return Extract(connection, new[] {task}).Catalogs.Single();
    }

    /// <summary>
    /// Asynchronously extracts all schemas from the database.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">The connection.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>
    /// <see cref="Catalog"/> that holds all schemas in the database.
    /// </returns>
    public async Task<Catalog> ExtractCatalogAsync(SqlConnection connection, CancellationToken token = default)
    {
      var defaultSchema = await GetDefaultSchemaAsync(connection, token).ConfigureAwait(false);
      var task = new SqlExtractionTask(defaultSchema.Database);
      return (await ExtractAsync(connection, new[] {task}, token).ConfigureAwait(false)).Catalogs.Single();
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
      var defaultSchema = GetDefaultSchema(connection);
      return ExtractSchema(connection, defaultSchema.Database, defaultSchema.Schema);
    }

    /// <summary>
    /// Asynchronously extracts the default schema from the database.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">The connection.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>
    /// <see cref="Catalog"/> that holds just the default schema in the database.
    /// </returns>
    public async Task<Schema> ExtractDefaultSchemaAsync(SqlConnection connection, CancellationToken token = default)
    {
      var defaultSchema = await GetDefaultSchemaAsync(connection, token).ConfigureAwait(false);
      return await ExtractSchemaAsync(connection, defaultSchema.Database, defaultSchema.Schema, token)
        .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts the specified schema from the database.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">A name of the schema to be extracted.</param>
    /// <returns>
    /// Extracted <see cref="Schema"/> instance.
    /// </returns>
    public Schema ExtractSchema(SqlConnection connection, string schemaName)
    {
      var defaultSchema = GetDefaultSchema(connection);
      return ExtractSchema(connection, defaultSchema.Database, schemaName);
    }

    /// <summary>
    /// Asynchronously extracts the specified schema from the database.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">A name of the schema to be extracted.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>
    /// Extracted <see cref="Schema"/> instance.
    /// </returns>
    public async Task<Schema> ExtractSchemaAsync(
      SqlConnection connection, string schemaName, CancellationToken token = default)
    {
      var defaultSchema = await GetDefaultSchemaAsync(connection, token).ConfigureAwait(false);
      return await ExtractSchemaAsync(connection, defaultSchema.Database, schemaName, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the connection using default connection information.
    /// </summary>
    public SqlConnection CreateConnection()
    {
      var result = DoCreateConnection();
      result.ConnectionInfo = originConnectionInfo;
      return result;
    }

    /// <summary>
    /// Creates the connection using specified connection information.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <returns></returns>
    public SqlConnection CreateConnection(ConnectionInfo connectionInfo)
    {
      var result = DoCreateConnection();
      result.ConnectionInfo = connectionInfo;
      return result;
    }

    /// <summary>
    /// Gets the type of the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>Type of the exception.</returns>
    public virtual SqlExceptionType GetExceptionType(Exception exception) => SqlExceptionType.Unknown;

    /// <summary>
    /// Gets information about exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>Information about exception.</returns>
    public virtual SqlExceptionInfo GetExceptionInfo(Exception exception) =>
      SqlExceptionInfo.Create(GetExceptionType(exception));

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
    /// Creates connection.
    /// </summary>
    /// <returns>Created connection.</returns>
    protected abstract SqlConnection DoCreateConnection();

    protected virtual void RegisterCustomMappings(TypeMappingRegistryBuilder builder)
    {
    }

    protected virtual void RegisterCustomReverseMappings(TypeMappingRegistryBuilder builder)
    {
    }

    #region Private / internal methods

    internal void Initialize(SqlDriverFactory creator, ConnectionInfo creatorConnectionInfo)
    {
      origin = creator;
      originConnectionInfo = creatorConnectionInfo;

      var serverInfoProvider = CreateServerInfoProvider();
      ServerInfo = ServerInfo.Build(serverInfoProvider);

      var typeMapper = CreateTypeMapper();
      typeMapper.Initialize();
      TypeMappings = CreateTypeMappingCollection(typeMapper);

      Translator = CreateTranslator();
      Translator.Initialize();
    }

    private TypeMappingRegistry CreateTypeMappingCollection(TypeMapper mapper)
    {
      var builder = new TypeMappingRegistryBuilder(mapper);
      RegisterStandardMappings(builder);
      RegisterStandardReverseMappings(builder);
      RegisterCustomMappings(builder);
      RegisterCustomReverseMappings(builder);
      return builder.Build();
    }

    private static void RegisterStandardMappings(TypeMappingRegistryBuilder builder)
    {
      var mapper = builder.Mapper;

      builder.Add(WellKnownTypes.Bool, mapper.ReadBoolean, mapper.BindBoolean, mapper.MapBoolean);
      builder.Add(WellKnownTypes.Char, mapper.ReadChar, mapper.BindChar, mapper.MapChar);
      builder.Add(WellKnownTypes.String, mapper.ReadString, mapper.BindString, mapper.MapString);
      builder.Add(WellKnownTypes.Byte, mapper.ReadByte, mapper.BindByte, mapper.MapByte);
      builder.Add(WellKnownTypes.SByte, mapper.ReadSByte, mapper.BindSByte, mapper.MapSByte);
      builder.Add(WellKnownTypes.Int16, mapper.ReadShort, mapper.BindShort, mapper.MapShort);
      builder.Add(WellKnownTypes.UInt16, mapper.ReadUShort, mapper.BindUShort, mapper.MapUShort);
      builder.Add(WellKnownTypes.Int32, mapper.ReadInt, mapper.BindInt, mapper.MapInt);
      builder.Add(WellKnownTypes.UInt32, mapper.ReadUInt, mapper.BindUInt, mapper.MapUInt);
      builder.Add(WellKnownTypes.Int64, mapper.ReadLong, mapper.BindLong, mapper.MapLong);
      builder.Add(WellKnownTypes.UInt64, mapper.ReadULong, mapper.BindULong, mapper.MapULong);
      builder.Add(WellKnownTypes.Single, mapper.ReadFloat, mapper.BindFloat, mapper.MapFloat);
      builder.Add(WellKnownTypes.Double, mapper.ReadDouble, mapper.BindDouble, mapper.MapDouble);
      builder.Add(WellKnownTypes.Decimal, mapper.ReadDecimal, mapper.BindDecimal, mapper.MapDecimal);
      builder.Add(WellKnownTypes.DateTime, mapper.ReadDateTime, mapper.BindDateTime, mapper.MapDateTime);
      builder.Add(WellKnownTypes.TimeSpan, mapper.ReadTimeSpan, mapper.BindTimeSpan, mapper.MapTimeSpan);
      builder.Add(WellKnownTypes.Guid, mapper.ReadGuid, mapper.BindGuid, mapper.MapGuid);
      builder.Add(WellKnownTypes.ByteArray, mapper.ReadByteArray, mapper.BindByteArray, mapper.MapByteArray);
#if DO_DATEONLY
      builder.Add(WellKnownTypes.DateOnly, mapper.ReadDateOnly, mapper.BindDateOnly, mapper.MapDateOnly);
      builder.Add(WellKnownTypes.TimeOnly, mapper.ReadTimeOnly, mapper.BindTimeOnly, mapper.MapTimeOnly);
#endif
    }

    private static void RegisterStandardReverseMappings(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverse(SqlType.Boolean, WellKnownTypes.Bool);
      builder.AddReverse(SqlType.Int8, WellKnownTypes.SByte);
      builder.AddReverse(SqlType.UInt8, WellKnownTypes.Byte);
      builder.AddReverse(SqlType.Int16, WellKnownTypes.Int16);
      builder.AddReverse(SqlType.UInt16, WellKnownTypes.UInt16);
      builder.AddReverse(SqlType.Int32, WellKnownTypes.Int32);
      builder.AddReverse(SqlType.UInt32, WellKnownTypes.UInt32);
      builder.AddReverse(SqlType.Int64, WellKnownTypes.Int64);
      builder.AddReverse(SqlType.UInt64, WellKnownTypes.UInt64);
      builder.AddReverse(SqlType.Decimal, WellKnownTypes.Decimal);
      builder.AddReverse(SqlType.Float, WellKnownTypes.Single);
      builder.AddReverse(SqlType.Double, WellKnownTypes.Double);
      builder.AddReverse(SqlType.DateTime, WellKnownTypes.DateTime);
      builder.AddReverse(SqlType.Interval, WellKnownTypes.TimeSpan);
      builder.AddReverse(SqlType.Char, WellKnownTypes.String);
      builder.AddReverse(SqlType.VarChar, WellKnownTypes.String);
      builder.AddReverse(SqlType.VarCharMax, WellKnownTypes.String);
      builder.AddReverse(SqlType.Binary, WellKnownTypes.ByteArray);
      builder.AddReverse(SqlType.VarBinary, WellKnownTypes.ByteArray);
      builder.AddReverse(SqlType.VarBinaryMax, WellKnownTypes.ByteArray);
      builder.AddReverse(SqlType.Guid, WellKnownTypes.Guid);
#if DO_DATEONLY
      builder.AddReverse(SqlType.Date, WellKnownTypes.DateOnly);
      builder.AddReverse(SqlType.Time, WellKnownTypes.TimeOnly);
#endif
    }

    private Extractor BuildExtractor(SqlConnection connection)
    {
      var extractor = CreateExtractor();
      extractor.Initialize(connection);
      return extractor;
    }

    private async Task<Extractor> BuildExtractorAsync(SqlConnection connection, CancellationToken token = default)
    {
      var extractor = CreateExtractor();
      await extractor.InitializeAsync(connection, token).ConfigureAwait(false);
      return extractor;
    }

    private void ValidateCompilerConfiguration(SqlCompilerConfiguration configuration)
    {
      var supported = ServerInfo.Query.Features.Supports(QueryFeatures.MultidatabaseQueries);
      var requested = configuration.DatabaseQualifiedObjects;
      if (requested && !supported) {
        throw SqlHelper.NotSupported(QueryFeatures.MultidatabaseQueries);
      }
    }

    private Schema ExtractSchema(SqlConnection connection, string databaseName, string schemaName)
    {
      var task = new SqlExtractionTask(databaseName, schemaName);
      return Extract(connection, new[] {task})
        .Catalogs[databaseName].Schemas.FirstOrDefault(el => el.Name == schemaName);
    }

    private async Task<Schema> ExtractSchemaAsync(SqlConnection connection, string databaseName, string schemaName,
      CancellationToken token = default)
    {
      var task = new SqlExtractionTask(databaseName, schemaName);
      return (await ExtractAsync(connection, new[] {task}, token).ConfigureAwait(false))
        .Catalogs[databaseName].Schemas.FirstOrDefault(el => el.Name == schemaName);
    }

    private static void CleanSchemas(Catalog catalog, IEnumerable<string> allowedSchemas)
    {
      // We allow extractors to extract schemas that were not requested
      // After extraction is complete, this methods removes not-necessary parts

      var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var schema in allowedSchemas) {
        allowed.Add(schema);
      }

      var schemasToRemove = catalog.Schemas
        .Where(s => !allowed.Contains(s.Name))
        .ToList();

      foreach (var schema in schemasToRemove) {
        catalog.Schemas.Remove(schema);
      }
    }

    #endregion

    protected SqlDriver(CoreServerInfo coreServerInfo)
    {
      coreServerInfo.Lock();
      CoreServerInfo = coreServerInfo;
    }
  }
}

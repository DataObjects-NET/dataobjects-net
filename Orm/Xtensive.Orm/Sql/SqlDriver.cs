// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm;
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
    public CoreServerInfo CoreServerInfo { get; private set; }

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
    public string GetConnectionString(ConnectionInfo connectionInfo)
    {
      return origin.GetConnectionString(connectionInfo);
    }

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
    /// Gets <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection"><see cref="SqlConnection"/> to use.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    public DefaultSchemaInfo GetDefaultSchema(SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      if (connection.Driver!=this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);

      return origin.GetDefaultSchema(connection.UnderlyingConnection, connection.ActiveTransaction);
    }

    /// <summary>
    /// Extracts catalogs/schemas according to the specified <paramref name="tasks"/>.
    /// </summary>
    /// <param name="connection">Extraction tasks.</param>
    /// <param name="tasks">Connection to use.</param>
    /// <returns>Extracted catalogs.</returns>
    public SqlExtractionResult Extract(SqlConnection connection, IEnumerable<SqlExtractionTask> tasks)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      ArgumentValidator.EnsureArgumentNotNull(tasks, "tasks");

      if (connection.Driver!=this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);

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
    /// Extracts the specified schema from the database.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>
    /// Extracted <see cref="Schema"/> instance.
    /// </returns>
    public Schema ExtractSchema(SqlConnection connection, string schemaName)
    {
      var defaultSchema = GetDefaultSchema(connection);
      return ExtractSchema(connection, defaultSchema.Database, schemaName);
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

      builder.Add(typeof (bool), mapper.ReadBoolean, mapper.BindBoolean, mapper.MapBoolean);
      builder.Add(typeof (char), mapper.ReadChar, mapper.BindChar, mapper.MapChar);
      builder.Add(typeof (string), mapper.ReadString, mapper.BindString, mapper.MapString);
      builder.Add(typeof (byte), mapper.ReadByte, mapper.BindByte, mapper.MapByte);
      builder.Add(typeof (sbyte), mapper.ReadSByte, mapper.BindSByte, mapper.MapSByte);
      builder.Add(typeof (short), mapper.ReadShort, mapper.BindShort, mapper.MapShort);
      builder.Add(typeof (ushort), mapper.ReadUShort, mapper.BindUShort, mapper.MapUShort);
      builder.Add(typeof (int), mapper.ReadInt, mapper.BindInt, mapper.MapInt);
      builder.Add(typeof (uint), mapper.ReadUInt, mapper.BindUInt, mapper.MapUInt);
      builder.Add(typeof (long), mapper.ReadLong, mapper.BindLong, mapper.MapLong);
      builder.Add(typeof (ulong), mapper.ReadULong, mapper.BindULong, mapper.MapULong);
      builder.Add(typeof (float), mapper.ReadFloat, mapper.BindFloat, mapper.MapFloat);
      builder.Add(typeof (double), mapper.ReadDouble, mapper.BindDouble, mapper.MapDouble);
      builder.Add(typeof (decimal), mapper.ReadDecimal, mapper.BindDecimal, mapper.MapDecimal);
      builder.Add(typeof (DateTime), mapper.ReadDateTime, mapper.BindDateTime, mapper.MapDateTime);
      builder.Add(typeof (TimeSpan), mapper.ReadTimeSpan, mapper.BindTimeSpan, mapper.MapTimeSpan);
      builder.Add(typeof (Guid), mapper.ReadGuid, mapper.BindGuid, mapper.MapGuid);
      builder.Add(typeof (byte[]), mapper.ReadByteArray, mapper.BindByteArray, mapper.MapByteArray);
    }

    private static void RegisterStandardReverseMappings(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverse(SqlType.Boolean, typeof(bool));
      builder.AddReverse(SqlType.Int8, typeof(sbyte));
      builder.AddReverse(SqlType.UInt8, typeof(byte));
      builder.AddReverse(SqlType.Int16, typeof(short));
      builder.AddReverse(SqlType.UInt16, typeof(ushort));
      builder.AddReverse(SqlType.Int32, typeof(int));
      builder.AddReverse(SqlType.UInt32, typeof(uint));
      builder.AddReverse(SqlType.Int64, typeof(long));
      builder.AddReverse(SqlType.UInt64, typeof(ulong));
      builder.AddReverse(SqlType.Decimal, typeof(decimal));
      builder.AddReverse(SqlType.Float, typeof(float));
      builder.AddReverse(SqlType.Double, typeof(double));
      builder.AddReverse(SqlType.DateTime, typeof(DateTime));
      builder.AddReverse(SqlType.Interval, typeof(TimeSpan));
      builder.AddReverse(SqlType.Char, typeof(string));
      builder.AddReverse(SqlType.VarChar, typeof(string));
      builder.AddReverse(SqlType.VarCharMax, typeof(string));
      builder.AddReverse(SqlType.Binary, typeof(byte[]));
      builder.AddReverse(SqlType.VarBinary, typeof(byte[]));
      builder.AddReverse(SqlType.VarBinaryMax, typeof(byte[]));
      builder.AddReverse(SqlType.Guid, typeof(Guid));
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

    private Schema ExtractSchema(SqlConnection connection, string databaseName, string schemaName)
    {
      var task = new SqlExtractionTask(databaseName, schemaName);
      return Extract(connection, new[] {task}).Catalogs[databaseName].Schemas.FirstOrDefault(el=>el.Name==schemaName);
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
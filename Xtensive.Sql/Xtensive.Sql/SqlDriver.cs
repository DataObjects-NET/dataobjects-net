// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Info;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Xtensive.Sql.Resources;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Sql
{
  /// <summary>
  /// Declares a base functionality of any <see cref="SqlDriver"/>.
  /// </summary>
  public abstract class SqlDriver
  {
    private const string DriverAssemblyFormat = "Xtensive.Sql.{0}";

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
    public DataAccessHandler DataAccessHandler { get; private set; }

    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="unit">The Sql.Dom statement.</param>
    /// <returns>Results of compilation.</returns>
    public SqlCompilationResult Compile(ISqlCompileUnit unit)
    {
      ArgumentValidator.EnsureArgumentNotNull(unit, "unit");
      return CreateCompiler().Compile(unit);
    }

    /// <summary>
    /// Extracts the model from server.
    /// </summary>
    /// <param name="connection">The connection to work within.</param>
    /// <param name="transaction">The transaction to work within.</param>
    /// <returns>Extracted model.</returns>
    public Catalog ExtractModel(SqlConnection connection, DbTransaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      if (connection.Driver != this)
        throw new ArgumentException(Strings.ExSpecifiedConnectionDoesNotBelongToThisDriver);
      var extractor = CreateExtractor();
      extractor.Initialize(connection, transaction);
      return extractor.Extract();
    }

    /// <summary>
    /// Creates the connection from the specified url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created connection</returns>
    public SqlConnection CreateConnection(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      return CreateConnectionInternal(new SqlConnectionUrl(url));
    }

    /// <summary>
    /// Creates the connection from the specified connection info.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created connection.</returns>
    public SqlConnection CreateConnection(SqlConnectionUrl url)
    {
      ArgumentValidator.EnsureArgumentNotNull(url, "url");
      return CreateConnectionInternal(url);
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
    protected abstract DataAccessHandler CreateDataAccessHandler();

    /// <summary>
    /// Creates the db connection from the specified connection info.
    /// </summary>
    /// <param name="sqlConnectionUrl">The connection info.</param>
    /// <returns>Created connection.</returns>
    protected abstract DbConnection CreateNativeConnection(SqlConnectionUrl sqlConnectionUrl);
    
    /// <summary>
    /// Creates the driver from the specified connection url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(SqlConnectionUrl url)
    {
      ArgumentValidator.EnsureArgumentNotNull(url, "url");
      return CreateDriverInternal(url);
    }

    /// <summary>
    /// Creates the driver from the specified connection url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      return CreateDriverInternal(new SqlConnectionUrl(url));
    }

    #region Private / internal methods

    private void Initialize()
    {
      Translator = CreateTranslator();
      Translator.Initialize();
      DataAccessHandler = CreateDataAccessHandler();
      DataAccessHandler.Initialize();
      TypeMappings = new TypeMappingCollection(DataAccessHandler);
    }
    
    private SqlConnection CreateConnectionInternal(SqlConnectionUrl url)
    {
      return new SqlConnection(this, CreateNativeConnection(url), url);
    }

    private static SqlDriver CreateDriverInternal(SqlConnectionUrl url)
    {
      var thisAssemblyName = Assembly.GetExecutingAssembly().GetName();
      string driverAssemblyShortName = string.Format(DriverAssemblyFormat, url.Protocol);
      string driverAssemblyFullName = thisAssemblyName.FullName.Replace(thisAssemblyName.Name, driverAssemblyShortName);
      var assembly = Assembly.Load(driverAssemblyFullName);
      var factoryType = assembly.GetTypes()
        .Single(type => type.IsPublicNonAbstractInheritorOf(typeof (SqlDriverFactory)));
      var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
      var driver = factory.CreateDriver(url);
      driver.Initialize();
      return driver;
    }

    #endregion

    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serverInfoProvider">The server info provider.</param>
    protected SqlDriver(ServerInfoProvider serverInfoProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(serverInfoProvider, "serverInfoProvider");
      ServerInfo = ServerInfo.Build(serverInfoProvider);
    }
  }
}
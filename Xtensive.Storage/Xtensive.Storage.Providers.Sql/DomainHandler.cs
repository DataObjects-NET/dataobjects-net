// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor> accessorCache;

    /// <summary>
    /// Gets the storage schema.
    /// </summary>
    public Schema Schema { get; internal set; }

    /// <summary>
    /// Gets the model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets the SQL request builder.
    /// </summary>
    public SqlRequestBuilder RequestBuilder { get; private set; }

    /// <summary>
    /// Gets the value type mapper.
    /// </summary>
    public SqlValueTypeMapper ValueTypeMapper { get; private set; }

    /// <summary>
    /// Gets the SQL request cache.
    /// </summary>
    public ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest> RequestCache { get; private set; }

    /// <summary>
    /// Gets the connection provider.
    /// </summary>
    internal SqlConnectionProvider ConnectionProvider { get; private set; }

    /// <summary>
    /// Gets the SQL driver.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    /// <inheritdoc/>
    protected override IEnumerable<Type> GetCompilerProviderContainerTypes()
    {
      return new[] {
        typeof (NullableMappings),
        typeof (StringMappings),
        typeof (DateTimeMappings),
        typeof (TimeSpanMappings),
        typeof (MathMappings),
        typeof (NumericMappings),
        typeof (DecimalMappings)
      };
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new SqlCompiler(Handlers, compiledSources);
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler()
    {
      return new SqlOrderbyCorrector(Handlers);
    }

    /// <summary>
    /// Creates (or retrieves from cache) <see cref="DbDataReaderAccessor"/> 
    /// for the specified <see cref="TupleDescriptor"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="DbDataReaderAccessor"/> 
    /// for the specified <see cref="TupleDescriptor"/></returns>
    public DbDataReaderAccessor GetDataReaderAccessor(TupleDescriptor descriptor)
    {
      return accessorCache.GetValue(descriptor, BuildDataReaderAccessor);
    }

    /// <summary>
    /// Creates <see cref="ProviderOrderingDescriptor"/> for specified 
    /// <see cref="CompilableProvider"/>.
    /// </summary>
    /// <param name="provider">The provider for which <see cref="ProviderOrderingDescriptor"/> 
    /// should be created.</param>
    /// <returns>A newly created <see cref="ProviderOrderingDescriptor"/>.</returns>
    protected static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      bool isOrderSensitive = provider.Type==ProviderType.Skip || provider.Type==ProviderType.Take
        || provider.Type==ProviderType.Seek || provider.Type==ProviderType.Range
        || provider.Type == ProviderType.RowNumber;
      bool preservesOrder = provider.Type==ProviderType.Take
        || provider.Type==ProviderType.Seek || provider.Type==ProviderType.Range
        || provider.Type == ProviderType.RowNumber || provider.Type == ProviderType.Reindex
        || provider.Type == ProviderType.Sort || provider.Type == ProviderType.Range
        || provider.Type == ProviderType.Seek;
      bool isOrderBreaker = provider.Type==ProviderType.Except
        || provider.Type==ProviderType.Intersect || provider.Type==ProviderType.Union
        || provider.Type==ProviderType.Concat || provider.Type==ProviderType.Existence
        || provider.Type==ProviderType.Distinct;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type==ProviderType.Reindex;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker,
        isSorter);
    }
    
    /// <summary>
    /// Builds <see cref="DbDataReaderAccessor"/> from specified <see cref="TupleDescriptor"/>.
    /// You should not use this method directly since it does not provide caching.
    /// Use <see cref="GetDataReaderAccessor"/>.
    /// </summary>
    /// <param name="descriptor">The tuple descriptor.</param>
    /// <returns></returns>
    protected virtual DbDataReaderAccessor BuildDataReaderAccessor(TupleDescriptor descriptor)
    {
      var readers = new List<Func<DbDataReader, int, object>>(descriptor.Count);
      var converters = new List<Func<object, object>>(descriptor.Count);

      foreach (var item in descriptor) {
        var typeMapping = ValueTypeMapper.GetTypeMapping(item, null);
        readers.Add(typeMapping.DataReaderAccessor);
        converters.Add(typeMapping.FromSqlValue);
      }

      return new DbDataReaderAccessor(readers, converters);
    }

    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException">Somethig went wrong.</exception>
    public override void BuildMapping()
    {
//      var sessionHandler = ((SessionHandler) BuildingScope.Context.SystemSessionHandler);
//      var modelProvider = new SqlModelProvider(sessionHandler.Connection, sessionHandler.Transaction);
//      var storageModel = SqlModel.Build(modelProvider);
      Schema = UpgradeContext.Demand().NativeExtractedSchema as Schema; // storageModel.DefaultServer.DefaultCatalog.DefaultSchema;
      var domainModel = Handlers.Domain.Model;

      foreach (var type in domainModel.Types) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || Mapping[primaryIndex]!=null)
          continue;
        var storageTableName = primaryIndex.ReflectedType.MappingName;
        var storageTable = Schema.Tables[storageTableName];
        if (storageTable==null)
          throw new DomainBuilderException(string.Format(Strings.ExTableXIsNotFound, storageTableName));
        var mapping = Mapping.RegisterMapping(primaryIndex, storageTable);
        foreach (var column in primaryIndex.Columns) {
          var storageColumnName = Domain.NameBuilder.BuildTableColumnName(column);
          var storageColumn = storageTable.TableColumns
            .FirstOrDefault(dataTableColumn => dataTableColumn.Name==storageColumnName);
          if (storageColumn==null)
            throw new DomainBuilderException(
              string.Format(Strings.ExColumnXIsNotFoundInTableY, storageColumnName, storageTableName));
          mapping.RegisterMapping(
            column,
            storageColumn,
            ValueTypeMapper.GetTypeMapping(column));
        }
        foreach (var secondaryIndex in type.Indexes.Find(IndexAttributes.Real).Where(i => !i.IsPrimary)) {
          var storageIndexName = secondaryIndex.MappingName;
          var storageIndex = storageTable.Indexes
            .FirstOrDefault(i => i.Name==storageIndexName);
          if (storageIndex==null)
            throw new DomainBuilderException(
              string.Format(Strings.ExIndexXIsNotFound, storageIndexName));
          mapping.RegisterMapping(secondaryIndex, storageIndex);
        }
      }
    }

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());
      ConnectionProvider = new SqlConnectionProvider();
      Mapping = new ModelMapping();
      RequestCache = ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest>.Create(new object());
      RequestBuilder = Handlers.HandlerFactory.CreateHandler<SqlRequestBuilder>();
      RequestBuilder.Initialize();
    }

    /// <inheritdoc/>
    public override void InitializeFirstSession()
    {
      base.InitializeFirstSession();

      Driver = ((SessionHandler) BuildingContext.Current.SystemSessionHandler).Connection.Driver;
      ValueTypeMapper = Handlers.HandlerFactory.CreateHandler<SqlValueTypeMapper>();
      ValueTypeMapper.Initialize();
    }
  }
}
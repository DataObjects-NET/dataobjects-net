// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Mappings;
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
    public Schema Schema { get; private set; }

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
    /// Gets the SQL driver.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    /// <inheritdoc/>
    protected override IEnumerable<Type> GetCompilerProviderContainerTypes()
    {
      return new[] {
        typeof (NullableCompilers),
        typeof (StringCompilers),
        typeof (DateTimeCompilers),
        typeof (TimeSpanCompilers),
        typeof (MathCompilers),
        typeof (NumericCompilers),
        typeof (DecimalCompilers),
        typeof (GuidCompilers)
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
    protected DbDataReaderAccessor BuildDataReaderAccessor(TupleDescriptor descriptor)
    {
      return new DbDataReaderAccessor(
        descriptor.Select(type => ValueTypeMapper.GetTypeMapping(type)));
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

    protected override ProviderInfo CreateProviderInfo()
    {
      var result = new ProviderInfo();
      var serverInfo = Driver.ServerInfo;

      // TODO: add corresponding features to Sql.Info and read them here
      result.EmptyBlobIsNull = false;
      result.EmptyStringIsNull = false;
      result.SupportsEnlist = false;

      result.DatabaseNameLength = serverInfo.Database.MaxIdentifierLength;
      result.MaxIndexColumnsCount = serverInfo.Index.MaxColumnAmount;
      result.MaxIndexKeyLength = serverInfo.Index.MaxLength;
      result.MaxIndexNameLength = serverInfo.Index.MaxIdentifierLength;
      result.MaxTableNameLength = serverInfo.Table.MaxIdentifierLength;
      result.NamedParameters = (serverInfo.Query.Features & QueryFeatures.NamedParameters)
        ==QueryFeatures.NamedParameters;
      result.ParameterPrefix = serverInfo.Query.ParameterPrefix;
      result.MaxComparisonOperations = serverInfo.Query.MaxComparisonOperations;
      result.MaxQueryLength = serverInfo.Query.MaxLength;
      result.SupportsBatches = (serverInfo.Query.Features & QueryFeatures.Batches)==QueryFeatures.Batches;
      result.SupportsClusteredIndexes = (serverInfo.Index.Features & IndexFeatures.Clustered)
        ==IndexFeatures.Clustered;
      result.SupportsCollations = serverInfo.Collation!=null;
      result.SupportsForeignKeyConstraints = serverInfo.ForeignKey!=null;
      result.SupportsIncludedColumns = (serverInfo.Index.Features & IndexFeatures.NonKeyColumns)
        ==IndexFeatures.NonKeyColumns;
      result.SupportKeyColumnSortOrder = (serverInfo.Index.Features & IndexFeatures.SortOrder)
        ==IndexFeatures.SortOrder;
      result.SupportSequences = serverInfo.Sequence.Features!=SequenceFeatures.None;
      result.SupportsRealTimeSpan = serverInfo.DataTypes.Interval!=null;
      result.Version = (Version) serverInfo.Version.ProductVersion.Clone();
      result.Lock();
      return result;
    }

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      Driver = SqlDriver.Create(Handlers.Domain.Configuration.ConnectionInfo.ToString());
      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());
      ValueTypeMapper = new SqlValueTypeMapper(Driver);
      Mapping = new ModelMapping();
      RequestCache = ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest>.Create(new object());
      RequestBuilder = Handlers.HandlerFactory.CreateHandler<SqlRequestBuilder>();
      RequestBuilder.Initialize();
    }
  }
}
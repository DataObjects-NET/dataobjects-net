// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using Xtensive.Threading;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Orm.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor> accessorCache;
    private ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>> requestCache;

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
    public PersistRequestBuilder PersistRequestBuilder { get; private set; }

    /// <summary>
    /// Gets the temporary table manager.
    /// </summary>
    public TemporaryTableManager TemporaryTableManager { get; private set; }

    /// <summary>
    /// Gets the command processor factory.
    /// </summary>
    public CommandProcessorFactory CommandProcessorFactory { get; private set; }
    
    /// <summary>
    /// Gets the SQL driver.
    /// </summary>
    public Driver Driver { get; private set; }

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
    protected override ICompiler CreateCompiler()
    {
      return new SqlCompiler(Handlers);
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler(ICompiler compiler)
    {
      return new EmptySelectCorrector();
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
      return accessorCache.GetValue(descriptor,
        (_descriptor, _driver) =>
          new DbDataReaderAccessor(_descriptor.Select(type => _driver.GetTypeMapping(type))),
        Driver);
    }

    /// <summary>
    /// Gets the persist request for the specified <paramref name="task"/>.
    /// </summary>
    /// <param name="task">The task to get request from.</param>
    /// <returns>A <see cref="PersistRequest"/> that represents <paramref name="task"/>.</returns>
    public IEnumerable<PersistRequest> GetPersistRequest(PersistRequestBuilderTask task)
    {
      return requestCache.GetValue(task, PersistRequestBuilder.Build);
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
        || provider.Type == ProviderType.Seek
        || provider.Type==ProviderType.Distinct;
      bool isOrderBreaker = provider.Type==ProviderType.Except
        || provider.Type==ProviderType.Intersect || provider.Type==ProviderType.Union
        || provider.Type==ProviderType.Concat || provider.Type==ProviderType.Existence
        /*|| provider.Type==ProviderType.Distinct*/;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type==ProviderType.Reindex;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker,
        isSorter);
    }
    
    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    public override void BuildMapping()
    {
      var context = UpgradeContext.Demand();
      Schema = (Schema) Handlers.SchemaUpgradeHandler.GetNativeExtractedSchema(); 
      var domainModel = Handlers.Domain.Model;

      foreach (var type in domainModel.Types) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex == null || Mapping[primaryIndex] != null)
          continue;
        if (primaryIndex.IsAbstract)
          continue;
        if (context.Configuration.UpgradeMode.IsLegacy() && type.IsSystem)
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
            Driver.GetTypeMapping(column));
        }
      }
    }

    protected override ProviderInfo CreateProviderInfo()
    {
      return Driver.BuildProviderInfo();
    }

    /// <inheritdoc/>
    protected override IPreCompiler CreatePreCompiler()
    {
      var applyCorrector = new ApplyProviderCorrector(
        !ProviderInfo.Supports(ProviderFeatures.Apply));
      var skipTakeCorrector = new SkipTakeCorrector(
        ProviderInfo.Supports(ProviderFeatures.NativeTake),
        ProviderInfo.Supports(ProviderFeatures.NativeSkip));
      return new CompositePreCompiler(
        applyCorrector,
        skipTakeCorrector,
        new OrderingCorrector(ResolveOrderingDescriptor, false),
        new RedundantColumnOptimizer(),
        new OrderingCorrector(ResolveOrderingDescriptor, true));
    }

    /// <inheritdoc/>
    protected override void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
      registrations.Add(new ServiceRegistration(typeof (ICachingKeyGeneratorService), new CachingKeyGeneratorService(Handlers)));
    }

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      Driver = new Driver(Handlers.Domain);

      base.Initialize();

      StorageLocation = Driver.StorageLocation;

      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());
      requestCache = ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>>.Create(new object());
      Mapping = new ModelMapping();

      PersistRequestBuilder = Handlers.HandlerFactory.CreateHandler<PersistRequestBuilder>();
      TemporaryTableManager = Handlers.HandlerFactory.CreateHandler<TemporaryTableManager>();
      CommandProcessorFactory = Handlers.HandlerFactory.CreateHandler<CommandProcessorFactory>();

      TemporaryTableManager.Initialize();
      PersistRequestBuilder.Initialize();
      CommandProcessorFactory.Initialize();
    }
  }
}
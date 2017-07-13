// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.14

using System;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class UpgradeServiceAccessor : LockableBase, IDisposable
  {
    private readonly object resourcesSyncRoot = new object();
    private readonly DisposableSet resources = new DisposableSet();
    private readonly DisposableSet temporaryResources = new DisposableSet();

    private DomainConfiguration configuration;
    private StorageDriver storageDriver;
    private NameBuilder nameBuilder;
    private MappingResolver mappingResolver;
    private HandlerFactory handlerFactory;
    private SqlConnection connection;
    private PartialIndexFilterCompiler indexFilterCompiler;

    private ReadOnlyList<IModule> modules;
    private ReadOnlyList<IUpgradeHandler> orderedUpgradeHandlers;
    private ReadOnlyDictionary<Assembly, IUpgradeHandler> upgradeHandlers;
    private IFullTextCatalogResolver catalogResolver;

    public DomainConfiguration Configuration
    {
      get { return configuration; }
      set
      {
        this.EnsureNotLocked();
        configuration = value;
      }
    }

    public HandlerFactory HandlerFactory
    {
      get { return handlerFactory; }
      set
      {
        this.EnsureNotLocked();
        handlerFactory = value;
      }
    }

    public StorageDriver StorageDriver
    {
      get { return storageDriver; }
      set
      {
        this.EnsureNotLocked();
        storageDriver = value;
      }
    }

    public ProviderInfo ProviderInfo { get { return storageDriver.ProviderInfo; } }

    public NameBuilder NameBuilder
    {
      get { return nameBuilder; }
      set
      {
        this.EnsureNotLocked();
        nameBuilder = value;
      }
    }

    public MappingResolver MappingResolver
    {
      get { return mappingResolver; }
      set
      {
        this.EnsureNotLocked();
        mappingResolver = value;
      }
    }

    public PartialIndexFilterCompiler IndexFilterCompiler
    {
      get { return indexFilterCompiler; }
      set
      {
        this.EnsureNotLocked();
        indexFilterCompiler = value;
      }
    }

    public ReadOnlyList<IModule> Modules
    {
      get { return modules; }
      set
      {
        this.EnsureNotLocked();
        modules = value;
      }
    }

    public ReadOnlyList<IUpgradeHandler> OrderedUpgradeHandlers
    {
      get { return orderedUpgradeHandlers; }
      set
      {
        this.EnsureNotLocked();
        orderedUpgradeHandlers = value;
      }
    }

    public ReadOnlyDictionary<Assembly, IUpgradeHandler> UpgradeHandlers
    {
      get { return upgradeHandlers; }
      set
      {
        this.EnsureNotLocked();
        upgradeHandlers = value;
      }
    }

    public IFullTextCatalogResolver FulltextCatalogResolver
    {
      get { return catalogResolver; }
      set
      {
        this.EnsureNotLocked();
        catalogResolver = value;
      }
    }

    public SqlConnection Connection
    {
      get { return connection; }
      set
      {
        this.EnsureNotLocked();
        connection = value;
      }
    }

    public void ClearTemporaryResources()
    {
      lock (resourcesSyncRoot) {
        temporaryResources.Clear();
      }
    }

    public void RegisterTemporaryResource(IDisposable resource)
    {
      ArgumentValidator.EnsureArgumentNotNull(resource, "resource");
      lock (resourcesSyncRoot) {
        temporaryResources.Add(resource);
      }
    }

    public void RegisterResource(IDisposable resource)
    {
      ArgumentValidator.EnsureArgumentNotNull(resource, "resource");
      lock (resourcesSyncRoot) {
        resources.Add(resource);
      }
    }

    public void Dispose()
    {
      lock (resourcesSyncRoot) {
        resources.DisposeSafely();
        temporaryResources.DisposeSafely();
      }
    }
  }
}
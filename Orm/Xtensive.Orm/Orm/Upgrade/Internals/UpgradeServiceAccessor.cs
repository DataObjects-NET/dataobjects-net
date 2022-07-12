// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.14

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
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

    private IReadOnlyList<IModule> modules;
    private IReadOnlyList<IUpgradeHandler> orderedUpgradeHandlers;
    private IReadOnlyDictionary<Assembly, IUpgradeHandler> upgradeHandlers;
    private IFullTextCatalogNameBuilder catalogNameBuilder;

    public DomainConfiguration Configuration
    {
      get { return configuration; }
      set
      {
        EnsureNotLocked();
        configuration = value;
      }
    }

    public HandlerFactory HandlerFactory
    {
      get { return handlerFactory; }
      set
      {
        EnsureNotLocked();
        handlerFactory = value;
      }
    }

    public StorageDriver StorageDriver
    {
      get { return storageDriver; }
      set
      {
        EnsureNotLocked();
        storageDriver = value;
      }
    }

    public ProviderInfo ProviderInfo { get { return storageDriver.ProviderInfo; } }

    public NameBuilder NameBuilder
    {
      get { return nameBuilder; }
      set
      {
        EnsureNotLocked();
        nameBuilder = value;
      }
    }

    public MappingResolver MappingResolver
    {
      get { return mappingResolver; }
      set
      {
        EnsureNotLocked();
        mappingResolver = value;
      }
    }

    public PartialIndexFilterCompiler IndexFilterCompiler
    {
      get { return indexFilterCompiler; }
      set
      {
        EnsureNotLocked();
        indexFilterCompiler = value;
      }
    }

    public IReadOnlyList<IModule> Modules
    {
      get { return modules; }
      set
      {
        EnsureNotLocked();
        modules = value;
      }
    }

    public IReadOnlyList<IUpgradeHandler> OrderedUpgradeHandlers
    {
      get { return orderedUpgradeHandlers; }
      set
      {
        EnsureNotLocked();
        orderedUpgradeHandlers = value;
      }
    }

    public IReadOnlyDictionary<Assembly, IUpgradeHandler> UpgradeHandlers
    {
      get { return upgradeHandlers; }
      set
      {
        EnsureNotLocked();
        upgradeHandlers = value;
      }
    }

    public IFullTextCatalogNameBuilder FulltextCatalogNameBuilder
    {
      get { return catalogNameBuilder; }
      set
      {
        EnsureNotLocked();
        catalogNameBuilder = value;
      }
    }

    public SqlConnection Connection
    {
      get { return connection; }
      set
      {
        EnsureNotLocked();
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
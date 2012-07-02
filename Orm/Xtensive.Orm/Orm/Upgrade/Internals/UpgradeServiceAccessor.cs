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

namespace Xtensive.Orm.Upgrade
{
  internal sealed class UpgradeServiceAccessor : LockableBase, IDisposable
  {
    private readonly DisposableSet resources = new DisposableSet();

    private DomainConfiguration configuration;
    private StorageDriver driver;
    private NameBuilder nameBuilder;
    private MappingResolver resolver;
    private PartialIndexFilterNormalizer normalizer;
    private HandlerFactory handlerFactory;
    private ReadOnlyList<IModule> modules;
    private ReadOnlyList<IUpgradeHandler> orderedUpgradeHandlers;
    private ReadOnlyDictionary<Assembly, IUpgradeHandler> upgradeHandlers;

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

    public StorageDriver Driver
    {
      get { return driver; }
      set
      {
        this.EnsureNotLocked();
        driver = value;
      }
    }

    public ProviderInfo ProviderInfo { get { return driver.ProviderInfo; } }

    public NameBuilder NameBuilder
    {
      get { return nameBuilder; }
      set
      {
        this.EnsureNotLocked();
        nameBuilder = value;
      }
    }

    public MappingResolver Resolver
    {
      get { return resolver; }
      set
      {
        this.EnsureNotLocked();
        resolver = value;
      }
    }

    public PartialIndexFilterNormalizer Normalizer
    {
      get { return normalizer; }
      set
      {
        this.EnsureNotLocked();
        normalizer = value;
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

    public void RegisterResource(IDisposable resource)
    {
      ArgumentValidator.EnsureArgumentNotNull(resource, "resource");
      this.EnsureNotLocked();
      resources.Add(resource);
    }

    public void Dispose()
    {
      resources.DisposeSafely();
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Building.Builders
{
  public sealed class DomainBuilderConfiguration : LockableBase
  {
    private UpgradeServiceAccessor services;
    private DomainConfiguration domainConfiguration;
    private UpgradeStage stage;
    private IModelFilter modelFilter;

    /// <summary>
    /// Gets <see cref="DomainConfiguration"/> for domain.
    /// </summary>
    public DomainConfiguration DomainConfiguration
    {
      get { return domainConfiguration; }
      set
      {
        this.EnsureNotLocked();
        domainConfiguration = value;
      }
    }

    /// <summary>
    /// Gets <see cref="UpgradeStage"/> for domain.
    /// </summary>
    public UpgradeStage Stage
    {
      get { return stage; }
      set
      {
        this.EnsureNotLocked();
        stage = value;
      }
    }

    internal IModelFilter ModelFilter
    {
      get { return modelFilter; }
      set
      {
        this.EnsureNotLocked();
        modelFilter = value;
      }
    }

    internal UpgradeServiceAccessor Services
    {
      get { return services; }
      set
      {
        this.EnsureNotLocked();
        services = value;
      }
    }

    // Constructors

    internal DomainBuilderConfiguration()
    {
    }
  }
}
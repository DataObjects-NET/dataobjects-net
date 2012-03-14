// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Building.Builders
{
  public sealed class DomainBuilderConfiguration : LockableBase
  {
    private Func<Type, bool> typeFilter;
    private Func<PropertyInfo, bool> fieldFilter;
    private Func<Type, int> typeIdProvider;
    private UpgradeServiceAccessor services;
    private DomainConfiguration domainConfiguration;
    private UpgradeStage stage;

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

    /// <summary>
    /// Gets type filter for domain.
    /// </summary>
    public Func<Type, bool> TypeFilter
    {
      get { return typeFilter; }
      set
      {
        this.EnsureNotLocked();
        typeFilter = value;
      }
    }

    /// <summary>
    /// Gets field filter for domain.
    /// </summary>
    public Func<PropertyInfo, bool> FieldFilter
    {
      get { return fieldFilter; }
      set
      {
        this.EnsureNotLocked();
        fieldFilter = value;
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
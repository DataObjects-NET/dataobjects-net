// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Building.Builders
{
  /// <summary>
  /// Additional domain build process configuration 
  /// used by <see cref="DomainBuilder"/>.
  /// </summary>
  public class DomainBuilderConfiguration
  {
    /// <summary>
    /// Gets or sets the schema upgrade mode.
    /// </summary>
    public SchemaUpgradeMode SchemaUpgradeMode { get; private set; }

    /// <summary>
    /// Gets current <see cref="UpgradeContext"/>.
    /// </summary>
    public Upgrade.UpgradeContext UpgradeContext { get; private set; }

    /// <summary>
    /// Gets or sets the type filter.
    /// </summary>
    public Func<Type, bool> TypeFilter { get; set; }

    /// <summary>
    /// Gets or sets the property filter.
    /// </summary>
    public Func<PropertyInfo, bool> FieldFilter { get; set; }

    /// <summary>
    /// Gets or sets the "schema ready" handler.
    /// </summary>
    public Func<StorageModel, StorageModel, HintSet> SchemaReadyHandler { get; set; }

    /// <summary>
    /// Gets or sets the "upgrade actions ready" handler.
    /// </summary>
    public Action<NodeDifference, ActionSequence> UpgradeActionsReadyHandler { get; set; }

    /// <summary>
    /// Gets or sets the upgrade handler.
    /// </summary>
    public Action UpgradeHandler { get; set; }

    /// <summary>
    /// Gets or sets the type id provider.
    /// </summary>
    public Func<Type, int> TypeIdProvider { get; set; }

    // Constructors

    internal DomainBuilderConfiguration(SchemaUpgradeMode schemaUpgradeMode, Upgrade.UpgradeContext upgradeContext)
    {
      SchemaUpgradeMode = schemaUpgradeMode;
      UpgradeContext = upgradeContext;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.StorageModel;

namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Additional domain build process configuration 
  /// used by <see cref="DomainBuilder"/>.
  /// </summary>
  [Serializable]
  public class DomainBuilderConfiguration
  {
    /// <summary>
    /// Gets or sets the schema upgrade mode.
    /// </summary>
    public SchemaUpgradeMode SchemaUpgradeMode { get; set; }

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
    public Func<StorageInfo, StorageInfo, HintSet> SchemaReadyHandler { get; set; }

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

    /// <summary>
    /// Gets the collection of extension modules.
    /// </summary>
    public ReadOnlyList<IModule> Modules { get; private set; }

    /// <summary>
    /// Gets the collection of services related to building or upgrade.
    /// </summary>
    public IServiceContainer Services { get; private set; }

    
    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="schemaUpgradeMode">The schema upgrade mode.</param>
    /// <param name="modules">The collection of modules.</param>
    /// <param name="services">The collection of services.</param>
    public DomainBuilderConfiguration(SchemaUpgradeMode schemaUpgradeMode, 
      ReadOnlyList<IModule> modules,
      IServiceContainer services)
    {
      SchemaUpgradeMode = schemaUpgradeMode;
      Modules = modules;
      Services = services;
    }
  }
}
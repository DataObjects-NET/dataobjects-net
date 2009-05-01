// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Indexing.Model;

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
    /// Gets or sets the type name provider.
    /// </summary>
    public Func<Type, string> TypeNameProvider { get; set; }

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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="schemaUpgradeMode">The schema upgrade mode.</param>
    public DomainBuilderConfiguration(SchemaUpgradeMode schemaUpgradeMode)
    {
      SchemaUpgradeMode = schemaUpgradeMode;
    }
  }
}
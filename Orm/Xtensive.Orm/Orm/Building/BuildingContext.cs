// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// Domain building context.
  /// </summary>
  public sealed class BuildingContext
  {
    internal List<Pair<AssociationInfo, string>> PairedAssociations { get; private set; }
    internal HashSet<TypeInfo> TypesWithProcessedInheritedAssociations { get; private set; }
    internal Dictionary<TypeInfo,List<Pair<AssociationInfo, string>>> PairedAssociationsToReverse { get; private set; }
    internal HashSet<AssociationInfo> DiscardedAssociations { get; private set; }
    internal ModelInspectionResult ModelInspectionResult { get; private set; }
    internal Graph<TypeDef> DependencyGraph { get; private set; }
    internal HashSet<TypeDef> Interfaces { get; private set; }

    #region Current property & Demand() method

    /// <summary>
    /// Gets the current <see cref="BuildingContext"/>.
    /// </summary>
    public static BuildingContext Current { get { return BuildingScope.Context; } }

    /// <summary>
    /// Gets the current <see cref="BuildingContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="BuildingContext.Current"/> <see cref="BuildingContext"/> is <see langword="null" />.</exception>
    public static BuildingContext Demand()
    {
      var current = Current;
      if (current==null)
        throw Exceptions.ContextRequired<BuildingContext, BuildingScope>();
      return current;
    }

    #endregion

    /// <summary>
    /// Gets the configuration of the building <see cref="Orm.Domain"/>.
    /// </summary>
    public DomainConfiguration Configuration { get { return BuilderConfiguration.DomainConfiguration; } }

    /// <summary>
    /// Gets the building configuration.
    /// </summary>
    public DomainBuilderConfiguration BuilderConfiguration { get; private set; }

    /// <summary>
    /// Gets the <see cref="Orm.Domain"/> object.
    /// </summary>
    public Domain Domain { get; internal set; }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { get { return Domain.Handlers.NameBuilder; } }

    /// <summary>
    /// Gets the <see cref="Orm.Domain"/> model definition.
    /// </summary>
    public DomainModelDef ModelDef { get; internal set; }

    /// <summary>
    /// Gets domain  model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    internal ModelDefBuilder ModelDefBuilder { get; set; }

    // Constructors

    internal BuildingContext(DomainBuilderConfiguration builderConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, "builderConfiguration");

      BuilderConfiguration = builderConfiguration;
      PairedAssociations = new List<Pair<AssociationInfo, string>>();
      PairedAssociationsToReverse = new Dictionary<TypeInfo, List<Pair<AssociationInfo, string>>>();
      TypesWithProcessedInheritedAssociations = new HashSet<TypeInfo>();
      DiscardedAssociations = new HashSet<AssociationInfo>();
      ModelInspectionResult = new ModelInspectionResult();
      DependencyGraph = new Graph<TypeDef>();
      Interfaces = new HashSet<TypeDef>();
    }
  }
}
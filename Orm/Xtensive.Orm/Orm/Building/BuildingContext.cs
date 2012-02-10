// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
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
    internal Dictionary<Type, int> SystemTypeIds { get; private set; }
    internal ModelInspectionResult ModelInspectionResult { get; private set; }
    internal Graph<TypeDef> DependencyGraph { get; private set; }
    internal HashSet<TypeDef> Interfaces { get; private set; }
    internal HashSet<IndexInfo> UntypedIndexes { get; private set; }
    internal Dictionary<KeyGenerator, KeyInfo> KeyGenerators { get; private set; }
    internal Queue<Type> Types { get; private set; }

    #region Current property & Demand() method

    /// <summary>
    /// Gets the current <see cref="BuildingContext"/>.
    /// </summary>
    public static BuildingContext Current {
      get { return BuildingScope.Context; }
    }

    /// <summary>
    /// Gets the current <see cref="BuildingContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="BuildingContext.Current"/> <see cref="BuildingContext"/> is <see langword="null" />.</exception>
    public static BuildingContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)        
        throw Exceptions.ContextRequired<BuildingContext,BuildingScope>();
      return currentContext;
    }

    #endregion

    /// <summary>
    /// Gets the configuration of the building <see cref="Orm.Domain"/>.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the building configuration.
    /// </summary>
    public DomainBuilderConfiguration BuilderConfiguration { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Orm.Domain"/> object.
    /// </summary>
    public Domain Domain { get; internal set; }

    /// <summary>
    /// Gets the handler factory.
    /// </summary>
    public HandlerFactory HandlerFactory {
      get { return Domain.Handlers.HandlerFactory; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { 
      get { return Domain.Handlers.NameBuilder; }
    }

    /// <summary>
    /// Gets the system session handler.
    /// </summary>
    public SessionHandler SystemSessionHandler { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Orm.Domain"/> model definition.
    /// </summary>
    public DomainModelDef ModelDef { get; internal set; }

    /// <summary>
    /// Gets domain  model.
    /// </summary>
    public DomainModel Model { get; internal set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    internal BuildingContext(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      Configuration = configuration;
      PairedAssociations = new List<Pair<AssociationInfo, string>>();
      PairedAssociationsToReverse = new Dictionary<TypeInfo, List<Pair<AssociationInfo, string>>>();
      TypesWithProcessedInheritedAssociations = new HashSet<TypeInfo>();
      DiscardedAssociations = new HashSet<  AssociationInfo>();
      SystemTypeIds = new Dictionary<Type, int>();
      ModelInspectionResult = new ModelInspectionResult();
      DependencyGraph = new Graph<TypeDef>();
      Interfaces = new HashSet<TypeDef>();
      UntypedIndexes = new HashSet<IndexInfo>();
      KeyGenerators = new Dictionary<KeyGenerator, KeyInfo>();
      Types = new Queue<Type>(configuration.Types);
    }
  }
}
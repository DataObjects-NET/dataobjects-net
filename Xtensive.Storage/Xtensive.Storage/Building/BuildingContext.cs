// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.Internals;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Domain building context.
  /// </summary>
  public sealed class BuildingContext
  {
    private readonly List<Exception> errors = new List<Exception>();
    internal HashSet<Type> SkippedTypes { get; private set; }
    internal List<Pair<AssociationInfo, string>> PairedAssociations { get; private set; }
    internal CircularReferenceFinder<Type> CircularReferenceFinder { get; private set; }
    internal HashSet<AssociationInfo> DiscardedAssociations { get; private set; }
    internal Dictionary<Type, int> SystemTypeIds { get; private set; }
    internal Domain SystemDomain {get; set;}
    internal object ModelUnlockKey { get; set;}

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
    /// Gets the building stage.
    /// </summary>
    public BuildingStage Stage { get; internal set; }

    /// <summary>
    /// Gets the log used by this builder.
    /// </summary>
    public ILog Log { get; private set; }

    /// <summary>
    /// Gets the configuration of the building <see cref="Storage.Domain"/>.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the building configuration.
    /// </summary>
    public DomainBuilderConfiguration BuilderConfiguration { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Storage.Domain"/> object.
    /// </summary>
    public Domain Domain { get; internal set; }

    /// <summary>
    /// Gets the handler factory.
    /// </summary>
    public HandlerFactory HandlerFactory {
      get { return Domain.HandlerFactory; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { 
      get { return Domain.NameBuilder; }
    }

    /// <summary>
    /// Gets the system session handler.
    /// </summary>
    public SessionHandler SystemSessionHandler { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="Storage.Domain"/> model definition.
    /// </summary>
    public DomainModelDef Definition { get; internal set; }

    /// <summary>
    /// Gets the model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    #region Internal methods

    internal void RegisterError(DomainBuilderException exception)
    {
      Log.Error(exception);
      errors.Add(exception);
    }

    internal void EnsureBuildSucceed()
    {
      if (errors.Count != 0)
        throw new AggregateException(Strings.ExErrorsDuringStorageBuild, errors);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    internal BuildingContext(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      Log = StringLog.Create("DomainBuilder");
      Configuration = configuration;
      SkippedTypes = new HashSet<Type> {typeof (Entity), typeof (IEntity), typeof (Structure)};
      PairedAssociations = new List<Pair<AssociationInfo, string>>();
      CircularReferenceFinder = new CircularReferenceFinder<Type>(TypeHelper.GetShortName);
      DiscardedAssociations = new HashSet<  AssociationInfo>();
      SystemTypeIds = new Dictionary<Type, int>();
    }
  }
}
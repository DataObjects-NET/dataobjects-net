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
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.Internals;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// State keeper for <see cref="DomainBuilder"/> building process.
  /// </summary>
  public sealed class BuildingContext
  {
    private readonly List<Exception> errors = new List<Exception>();

    /// <summary>
    /// Gets the current <see cref="BuildingContext"/>.
    /// </summary>
    public static BuildingContext Current
    {
      get { return BuildingScope.Context; }
    }

    /// <summary>
    /// Gets the configuration of the building <see cref="Storage.Domain"/>.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the log used by this builder.
    /// </summary>
    public ILog Log { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="Storage.Domain"/> model definition.
    /// </summary>
    public DomainDef Definition { get; set; }

    /// <summary>
    /// Gets the name provider.
    /// </summary>
    public NameProvider NameProvider { get; set; }

    #region Private \ internal properties and methods

    internal DomainInfo Model { get; set; }

    internal Domain Domain { get; set; }

    internal HashSet<Type> SkippedTypes { get; private set; }

    internal List<Pair<AssociationInfo, string>> PairedAssociations { get; private set; }

    internal CircularReferenceFinder<Type> CircularReferenceFinder { get; private set; }

    internal HashSet<AssociationInfo> DiscardedAssociations { get; private set; }

    internal void RegisterError(DomainBuilderException exception)
    {
      Building.Log.Error(exception);
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
    public BuildingContext(DomainConfiguration configuration)
    {
      Configuration = configuration;
      SkippedTypes = new HashSet<Type>();
      SkippedTypes.Add(typeof (Entity));
      SkippedTypes.Add(typeof (IEntity));
      SkippedTypes.Add(typeof (Structure));
      PairedAssociations = new List<Pair<AssociationInfo, string>>();
      Log = StringLog.Create("DomainBuilder");
      CircularReferenceFinder = new CircularReferenceFinder<Type>(TypeHelper.GetShortName);
      DiscardedAssociations = new HashSet<AssociationInfo>();
    }
  }
}
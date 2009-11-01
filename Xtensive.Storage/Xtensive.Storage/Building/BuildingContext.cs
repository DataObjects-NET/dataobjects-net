// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// State keeper for <see cref="DomainBuilder"/> building process.
  /// </summary>
  public sealed class BuildingContext
  {
    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public ILog Logger { get; private set; }

    /// <summary>
    /// Gets or sets the storage definition.
    /// </summary>
    public DomainDef Definition { get; set; }

    /// <summary>
    /// Gets the name provider.
    /// </summary>
    public NameProvider NameProvider { get; set; }

    /// <summary>
    /// Gets or sets the storage model.
    /// </summary>
    public DomainInfo Model { get; internal set; }

    internal HashSet<Type> SkippedTypes { get; private set; }

    internal Domain Domain { get; set; }

    internal List<FieldInfo> ComplexFields { get; private set; }

    internal List<Pair<AssociationInfo, string>> PairedAssociations { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildingContext"/> class.
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
      ComplexFields = new List<FieldInfo>();
      Logger = StringLog.Create("DomainBuilder");
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// Domain building context.
  /// </summary>
  public sealed class BuildingContext
  {
    internal List<Pair<AssociationInfo, string>> PairedAssociations { get; private set; }
    internal HashSet<AssociationInfo> DiscardedAssociations { get; private set; }
    internal ModelInspectionResult ModelInspectionResult { get; private set; }
    internal Graph<TypeDef> DependencyGraph { get; private set; }
    internal Validator Validator { get; private set; }

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
    /// Gets domain model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    /// <summary>
    /// Gets all available <see cref="IModule"/> implementations.
    /// </summary>
    public ICollection<IModule> Modules { get; private set; }

    /// <summary>
    /// Gets all available <see cref="IModule2"/> implementations.
    /// </summary>
    public ICollection<IModule2> Modules2 { get; private set; }

    internal ModelDefBuilder ModelDefBuilder { get; set; }

    internal DefaultSchemaInfo DefaultSchemaInfo { get; private set; }

    // Constructors

    internal BuildingContext(DomainBuilderConfiguration builderConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, "builderConfiguration");

      BuilderConfiguration = builderConfiguration;
      PairedAssociations = new List<Pair<AssociationInfo, string>>();
      DiscardedAssociations = new HashSet<AssociationInfo>();
      ModelInspectionResult = new ModelInspectionResult();
      DependencyGraph = new Graph<TypeDef>();

      Modules = BuilderConfiguration.Services.Modules.ToList().AsReadOnly();
      Modules2 = Modules.OfType<IModule2>().ToList().AsReadOnly();
      Validator = new Validator(builderConfiguration.Services.ProviderInfo.SupportedTypes);
      DefaultSchemaInfo = builderConfiguration.DefaultSchemaInfo;
    }
  }
}
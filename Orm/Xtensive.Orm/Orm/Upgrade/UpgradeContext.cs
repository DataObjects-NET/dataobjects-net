// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrade context.
  /// </summary>
  public sealed class UpgradeContext : Context<UpgradeScope>
  {
    #region IContext members

    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>.
    /// </summary>
    public static UpgradeContext Current { get { return UpgradeScope.CurrentContext; } }

    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="UpgradeContext.Current"/> <see cref="UpgradeContext"/> is <see langword="null" />.</exception>
    public static UpgradeContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)
        throw Exceptions.ContextRequired<UpgradeContext, UpgradeScope>();
      return currentContext;
    }

    /// <inheritdoc/>
    public override bool IsActive { get { return UpgradeScope.CurrentContext==this; } }

    /// <inheritdoc/>
    protected override UpgradeScope CreateActiveScope()
    {
      return new UpgradeScope(this);
    }

    #endregion

    /// <summary>
    /// Gets the current upgrade stage.
    /// </summary>
    public UpgradeStage Stage { get; internal set; }

    /// <summary>
    /// Gets the original <see cref="DomainConfiguration"/>.
    /// </summary>
    public DomainConfiguration OriginalConfiguration { get; private set; }

    /// <summary>
    /// Gets the <see cref="DomainConfiguration"/>
    /// at the current upgrade stage.
    /// </summary>
    public DomainConfiguration Configuration { get; internal set; }

    /// <summary>
    /// Gets the upgrade hints.
    /// </summary>
    public SetSlim<UpgradeHint> Hints { get; private set; }

    /// <summary>
    /// Gets the schema upgrade hints.
    /// </summary>
    public HintSet SchemaHints { get; internal set; }

    /// <summary>
    /// Gets the storage model difference 
    /// at the current upgrade stage.
    /// </summary>
    public NodeDifference SchemaDifference { get; internal set; }

    /// <summary>
    /// Gets the schema upgrade actions
    /// at the current upgrade stage.
    /// </summary>
    public ActionSequence SchemaUpgradeActions { get; internal set; }

    /// <summary>
    /// Gets the domain model that was extracted from storage.
    /// </summary>
    public StoredDomainModel ExtractedDomainModel { get; internal set; }

    /// <summary>
    /// Gets the extracted type map (Full name of the type and TypeId).
    /// </summary>
    public Dictionary<string, int> ExtractedTypeMap { get; internal set; }

    /// <summary>
    /// Gets or sets the collection of services related to upgrade.
    /// </summary>
    public IServiceContainer Services { get; internal set; }

    /// <summary>
    /// Gets the map of upgrade handlers.
    /// </summary>
    public ReadOnlyDictionary<Assembly, IUpgradeHandler> UpgradeHandlers { get; internal set; }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IUpgradeHandler> OrderedUpgradeHandlers { get; internal set; }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IModule> Modules { get; internal set; }

    /// <summary>
    /// Gets or sets current transaction scope.
    /// </summary>
    public TransactionScope TransactionScope { get; set; }

    internal HandlerFactory HandlerFactory { get; set; }

    internal StorageDriver TemplateDriver { get; set; }

    internal NameBuilder NameBuilder { get; set; }

    internal SchemaResolver SchemaResolver { get; set; }

    internal StorageModel ExtractedModelCache { get; set; }

    internal SqlExtractionResult ExtractedSqlModelCache { get; set; }

    // Constructors.

    internal UpgradeContext(DomainConfiguration originalConfiguration)
    {
      OriginalConfiguration = originalConfiguration;
      Stage = UpgradeStage.Initializing;
      Hints = new SetSlim<UpgradeHint>();
    }
  }
}
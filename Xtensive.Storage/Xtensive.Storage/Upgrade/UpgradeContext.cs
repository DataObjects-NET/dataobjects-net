// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model.Stored;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Upgrade context.
  /// </summary>
  public sealed class UpgradeContext : Context<UpgradeScope>
  {
    #region IContext<...> static members (Current, Demand())

    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>.
    /// </summary>
    public static UpgradeContext Current {
      get { return UpgradeScope.CurrentContext; }
    }

    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="UpgradeContext.Current"/> <see cref="UpgradeContext"/> is <see langword="null" />.</exception>
    public static UpgradeContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)        
        throw Exceptions.ContextRequired<UpgradeContext,UpgradeScope>();
      return currentContext;
    }

    #endregion

    /// <summary>
    /// Gets the current upgrade stage.
    /// </summary>
    public UpgradeStage Stage { get; internal set; }

    /// <summary>
    /// Gets the original <see cref="DomainConfiguration"/>.
    /// </summary>
    public DomainConfiguration OriginalConfiguration { get; internal set; }

    /// <summary>
    /// Gets the <see cref="DomainConfiguration"/>
    /// at the current upgrade stage.
    /// </summary>
    public DomainConfiguration Configuration { get; internal set; }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public IList<IUpgradeHandler> OrderedUpgradeHandlers { get; internal set; }

    /// <summary>
    /// Gets the map of upgrade handlers.
    /// </summary>
    public IDictionary<Assembly, IUpgradeHandler> UpgradeHandlers { get; internal set; }

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
    /// Gets the native storage schema.
    /// </summary>
    public object NativeExtractedSchema { get; internal set; }

    /// <summary>
    /// Gets or sets the collection of extension modules.
    /// </summary>
    public ModuleProvider Modules { get; internal set; }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return UpgradeScope.CurrentContext==this; }
    }

    /// <inheritdoc/>
    protected override UpgradeScope CreateActiveScope()
    {
      return new UpgradeScope(this);
    }

    #endregion


    // Constructors.

    internal UpgradeContext()
    {
      Hints = new SetSlim<UpgradeHint>();
    }
  }
}
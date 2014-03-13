// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
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
    [Obsolete("Use Configuration property instead.")]
    public DomainConfiguration OriginalConfiguration { get { return Configuration; } }

    /// <summary>
    /// Gets the <see cref="DomainConfiguration"/>
    /// at the current upgrade stage.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets <see cref="NodeConfiguration"/> (if available).
    /// </summary>
    public NodeConfiguration NodeConfiguration { get; private set; }

    /// <summary>
    /// Gets parent domain.
    /// </summary>
    public Domain ParentDomain { get; private set; }

    /// <summary>
    /// Gets the upgrade hints.
    /// </summary>
    public SetSlim<UpgradeHint> Hints { get; private set; }
    
    /// <summary>
    /// Gets the recycled definitions.
    /// </summary>
    public ICollection<RecycledDefinition> RecycledDefinitions { get; private set; }

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
    /// Gets the map of upgrade handlers.
    /// </summary>
    public ReadOnlyDictionary<Assembly, IUpgradeHandler> UpgradeHandlers { get { return Services.UpgradeHandlers; } }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IUpgradeHandler> OrderedUpgradeHandlers { get { return Services.OrderedUpgradeHandlers; } }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IModule> Modules { get { return Services.Modules; } }

    /// <summary>
    /// Gets <see cref="Session"/> that is used for upgrade.
    /// Session is available only in <see cref="UpgradeHandler.OnStage"/>
    /// and <see cref="UpgradeHandler.OnUpgrade"/> methods.
    /// You should not dispose upgrade session. Session lifetime is controlled by DataObjects.Net.
    /// </summary>
    public Session Session { get; internal set; }

    /// <summary>
    /// Gets <see cref="DbConnection"/> that is used for upgrade.
    /// You should not modify connection state by calling <see cref="DbConnection.Open"/>,
    /// <see cref="DbConnection.Close"/> or similar methods. Connection state is controlled by DataObjects.Net.
    /// </summary>
    public DbConnection Connection { get { return Services.Connection.UnderlyingConnection; } }

    /// <summary>
    /// Gets <see cref="DbTransaction"/> that is used for upgrade.
    /// You should not modify transaction state by calling <see cref="DbTransaction.Commit"/>,
    /// <see cref="DbTransaction.Rollback"/> or similar methods. Transaction state is controlled by DataObjects.Net.
    /// </summary>
    public DbTransaction Transaction { get { return Services.Connection.ActiveTransaction; } }

    #region Private / internal members

    internal object Cookie { get; private set; }

    internal UpgradeServiceAccessor Services { get; private set; }

    internal StorageModel ExtractedModelCache { get; set; }

    internal SchemaExtractionResult ExtractedSqlModelCache { get; set; }

    internal StorageModel TargetStorageModel { get; set; }

    internal ModelMapping ModelMapping { get; set; }

    internal MetadataSet Metadata { get; set; }

    internal ITypeIdProvider TypeIdProvider { get; set; }

    internal static UpgradeContext GetCurrent(object cookie)
    {
      var current = Current;
      return current!=null && current.Cookie==cookie ? current : null;
    }

    private void Initialize()
    {
      Stage = Configuration.UpgradeMode.IsMultistage() ? UpgradeStage.Upgrading : UpgradeStage.Final;
      Hints = new SetSlim<UpgradeHint>();
      RecycledDefinitions = new List<RecycledDefinition>();
      Services = new UpgradeServiceAccessor();
    }

    #endregion

    // Constructors.

    internal UpgradeContext(Domain parentDomain, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(parentDomain, "parentDomain");
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");

      Configuration = parentDomain.Configuration;
      NodeConfiguration = nodeConfiguration;
      Cookie = parentDomain.UpgradeContextCookie;
      ParentDomain = parentDomain;

      Initialize();
    }

    internal UpgradeContext(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      Configuration = configuration;
      Cookie = new object();
      NodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      NodeConfiguration.Lock();

      Initialize();
    }


  }
}
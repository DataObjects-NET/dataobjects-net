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
using Xtensive.Sorting;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Model.Stored;

using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
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
    public IServiceContainer Services { get; private set; }

    /// <summary>
    /// Gets the map of upgrade handlers.
    /// </summary>
    public ReadOnlyDictionary<Assembly, IUpgradeHandler> UpgradeHandlers { get; private set; }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IUpgradeHandler> OrderedUpgradeHandlers { get; private set; }

    /// <summary>
    /// Gets the ordered collection of upgrade handlers.
    /// </summary>
    public ReadOnlyList<IModule> Modules { get; private set; }

    /// <summary>
    /// Gets or sets current transaction scope.
    /// </summary>
    public TransactionScope TransactionScope { get; set; }

    internal object      NativeExtractedSchemaCache { get; set; }
    internal StorageModel ExtractedSchemaCache { get; set; }

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

    #region BuildXxx methods

    private void BuildServices()
    {
      var handlerRegistrations = (
        from type in OriginalConfiguration.Types.UpgradeHandlers
        select new ServiceRegistration(typeof (IUpgradeHandler), type, false)
        ).ToList();
      var moduleRegistrations = (
        from type in OriginalConfiguration.Types.Modules
        select new ServiceRegistration(typeof (IModule), type, false)
        ).ToList();
      var keyGeneratorRegistrations = DomainBuilder.CreateKeyGeneratorRegistrations(OriginalConfiguration);

      var allRegistrations = 
        handlerRegistrations
        .Concat(moduleRegistrations)
        .Concat(keyGeneratorRegistrations);

      var baseServices = new ServiceContainer(new List<ServiceRegistration>{
        new ServiceRegistration(typeof (UpgradeContext), this),
        new ServiceRegistration(typeof (DomainConfiguration), OriginalConfiguration),
      });

      var serviceContainerType = OriginalConfiguration.ServiceContainerType ?? typeof (ServiceContainer);
      Services = 
        ServiceContainer.Create(typeof (ServiceContainer), allRegistrations, 
          ServiceContainer.Create(serviceContainerType, baseServices));
    }

    /// <exception cref="DomainBuilderException">More then one enabled handler is provided for some assembly.</exception>
    private void BuildUpgradeHandlers()
    {
      // Getting user handlers
      var userHandlers =
        from handler in Services.GetAll<IUpgradeHandler>()
        let assembly = handler.Assembly ?? handler.GetType().Assembly
        where handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      var handlers = new Dictionary<Assembly, IUpgradeHandler>();
      foreach (var group in userHandlers) {
        var candidates = group.ToList();
        if (candidates.Count > 1)
          throw new DomainBuilderException(
            Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY.FormatWith(
              typeof (IUpgradeHandler).GetShortName(), group.Key));
        handlers.Add(group.Key, candidates[0]);
      }

      // Adding default handlers
      var assembliesWithUserHandlers = handlers.Select(pair => pair.Key);
      var assembliesWithoutUserHandler = 
        OriginalConfiguration.Types.PersistentTypes
          .Select(type => type.Assembly)
          .Distinct()
          .Except(assembliesWithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler(assembly);
        handlers.Add(assembly, handler);
      }

      // Building a list of handlers sorted by dependencies of their assemblies
      var dependencies = handlers.Keys.ToDictionary(
        assembly => assembly,
        assembly => assembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.ToString()).ToHashSet());
      var sortedHandlers =
        from pair in 
          TopologicalSorter.Sort(handlers, 
            (a0, a1) => dependencies[a1.Key].Contains(a0.Key.GetName().ToString()))
        select pair.Value;

      // Storing the result
      UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers);
      OrderedUpgradeHandlers = 
        new ReadOnlyList<IUpgradeHandler>(sortedHandlers.ToList());
    }

    private void BuildModules()
    {
      Modules = new ReadOnlyList<IModule>(Services.GetAll<IModule>().ToList());
    }

    #endregion


    // Constructors.

    internal UpgradeContext(DomainConfiguration originalConfiguration)
    {
      OriginalConfiguration = originalConfiguration;
      Stage = UpgradeStage.Initializing;
      Hints = new SetSlim<UpgradeHint>();

      using (Activate()) {
        // Ensures UpgradeContext.Current is this context
        BuildServices();
        BuildUpgradeHandlers();
        BuildModules();
      }
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to extension modules.
  /// </summary>
  [Serializable]
  public sealed class ModuleProvider
  {
    private readonly Dictionary<Type, IModule> modules = new Dictionary<Type, IModule>();
    private readonly List<IUpgradeHandler> upgradeHandlers = new List<IUpgradeHandler>();

    /// <summary>
    /// Gets instances of all extension modules which implement <see cref="IUpgradeHandler"/>.
    /// </summary>
    public ReadOnlyList<IUpgradeHandler> UpgradeHandlers { get; private set; }

    /// <summary>
    /// Gets instances of all extension modules.
    /// </summary>
    public ReadOnlyDictionary<Type, IModule> Modules { get; private set; }

    /// <summary>
    /// Gets the ordered collection of all extension modules.
    /// </summary>
    public ReadOnlyList<IModule> OrderedModules { get; private set; }

    #region Private \ internal methods

    internal void SetUpgradeHandlers(IEnumerable<IUpgradeHandler> candidates)
    {
      foreach (var candidate in candidates) {
        var candidateType = candidate.GetType();
        if (modules.ContainsKey(candidateType)) {
          upgradeHandlers.Add(candidate);
          modules[candidateType] = (IModule)candidate;
        }
      }
    }
    
    internal void BuildModules()
    {
      var nonInstantiatedTypes = modules.Where(pair => pair.Value==null).Select(pair => pair.Key).ToList();
      foreach (var type in nonInstantiatedTypes)
        modules[type] = (IModule)type.Activate(null);
      Modules = new ReadOnlyDictionary<Type, IModule>(modules, false);
      SetOrderedModules();
    }

    private void SetOrderedModules()
    {
      var references = (from module in modules.Keys
      select new {module, Reference = module.GetType().Assembly.GetReferencedAssemblies()
        .Select(a => a.ToString())})
        .ToDictionary(a => a.module, a => a.Reference);
      var orderedModules = TopologicalSorter.Sort(modules,
        (module0, module1) => references[module1.Key].Any(asmName => asmName==module0.Key.Assembly.GetName().ToString()))
        .Select(module => module.Value).ToList();
      OrderedModules = new ReadOnlyList<IModule>(orderedModules, false);
    }

    #endregion
    

    // Constructors

    internal ModuleProvider(DomainConfiguration configuration)
    {
      configuration.Types.Count();
      var types = configuration.Types.Assemblies.SelectMany(assembly => assembly.GetTypes());
      foreach (var type in types)
        if (typeof(IModule).IsAssignableFrom(type)) {
          if (typeof(IDomainBuilder).IsAssignableFrom(type))
            throw new NotSupportedException(String.Format(Resources.Strings.ExTypeXImplementsInterfacesYAndZ,
              type.FullName, typeof(IModule).FullName, typeof(IDomainBuilder).FullName));
          if (!type.IsAbstract && type.IsClass)
            modules.Add(type, null);
        }
      UpgradeHandlers = new ReadOnlyList<IUpgradeHandler>(upgradeHandlers, false);
      //OrderedModules = new ReadOnlyList<IModule>(new List<IModule>(), false);
    }
  }
}
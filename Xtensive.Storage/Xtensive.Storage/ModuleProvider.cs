// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.02

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to extension modules.
  /// </summary>
  [Serializable]
  public sealed class ModuleProvider : IEnumerable<IModule>
  {
    private readonly Dictionary<Type, IModule> modules = new Dictionary<Type, IModule>();
    private readonly List<IUpgradeHandler> upgradeHandlers = new List<IUpgradeHandler>();
    private ReadOnlyList<IModule> orderedModules = new ReadOnlyList<IModule>(new List<IModule>(), false);

    /// <summary>
    /// Gets instances of all extension modules which implement <see cref="IUpgradeHandler"/>.
    /// </summary>
    internal ReadOnlyList<IUpgradeHandler> UpgradeHandlers { get; private set; }

    /// <summary>
    /// Gets instance of a module's type.
    /// </summary>
    /// <param name="type">The type of a module.</param>
    /// <returns>The instance of module's type or <see langword="null" /> 
    /// if <paramref name="type"/> is not registered.</returns>
    public IModule Get(Type type)
    {
      IModule result;
      if (modules.TryGetValue(type, out result))
        return result;
      return null;
    }
    
    /// <inheritdoc/>
    public IEnumerator<IModule> GetEnumerator()
    {
      return orderedModules.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

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
      SetOrderedModules();
    }

    private void SetOrderedModules()
    {
      var references = (from module in modules.Keys
      select new {module, Reference = module.GetType().Assembly.GetReferencedAssemblies()
        .Select(a => a.ToString())})
        .ToDictionary(a => a.module, a => a.Reference);
      var sortedModules = TopologicalSorter.Sort(modules,
        (module0, module1) => references[module1.Key].Any(asmName => asmName==module0.Key.Assembly.GetName().ToString()))
        .Select(module => module.Value).ToList();
      orderedModules = new ReadOnlyList<IModule>(sortedModules, false);
    }

    private static void ValidateModule(Type type)
    {
      ConstructorInfo constructor =
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

      if (constructor==null)
        throw new DomainBuilderException(string
          .Format(Strings.ExTypeXDoesNotHaveAParameterlessConstructor, type.GetFullName()));
    }

    #endregion
    

    // Constructors

    internal ModuleProvider(DomainConfiguration configuration)
    {
      configuration.Types.Count();
      var types = configuration.Types.Assemblies.SelectMany(assembly => assembly.GetTypes());
      foreach (var type in types)
        if (typeof(IModule).IsAssignableFrom(type)) {
          if (!type.IsAbstract && type.IsClass) {
            ValidateModule(type);
            modules.Add(type, null);
          }
        }
      UpgradeHandlers = new ReadOnlyList<IUpgradeHandler>(upgradeHandlers, false);
    }
  }
}
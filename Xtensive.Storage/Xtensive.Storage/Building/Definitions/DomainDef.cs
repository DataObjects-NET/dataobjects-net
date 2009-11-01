// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Notifications;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Definitions
{
  /// <summary>
  /// Represents a definition of <see cref="Domain"/>.
  /// </summary>
  [Serializable]
  public sealed class DomainDef : Node
  {
    private readonly HierarchyDefCollection hierarchies;
    private readonly NodeCollection<ServiceDef> services;
    private readonly TypeDefCollection types;

    /// <summary>
    /// Gets the <see cref="ServiceDef"/> instances contained in this instance.
    /// </summary>
    public NodeCollection<ServiceDef> Services
    {
      get { return services; }
    }

    /// <summary>
    /// Gets the <see cref="TypeDef"/> instances contained in this instance.
    /// </summary>
    public TypeDefCollection Types
    {
      get { return types; }
    }

    /// <summary>
    /// Gets the collection of <see cref="HierarchyDef"/> instances contained in this instance.
    /// </summary>
    public HierarchyDefCollection Hierarchies
    {
      get { return hierarchies; }
    }

    /// <summary>
    /// Defines new <see cref="TypeDef"/> and adds it to <see cref="DomainDef"/> instance.
    /// </summary>
    /// <param name="type">The underlying type.</param>
    /// <returns>Newly created <see cref="TypeDef"/> instance.</returns>
    public TypeDef DefineType(Type type)
    {
      ValidationResult vr = Validator.ValidateType(type, ValidationRule.Type);
      if (!vr.Success)
        throw new ArgumentOutOfRangeException("type", vr.Message);

      using (var scope = new LogCaptureScope(BuildingScope.Context.Logger)) {
        TypeDef typeDef = TypeBuilder.DefineType(type);
        if (scope.IsCaptured(LogEventTypes.Error))
          throw new DomainBuilderException(
            "Some errors have been occured during type definition process. See error log for details.");
        types.Add(typeDef);

        IndexBuilder.DefineIndexes(typeDef);

        return typeDef;
      }
    }

    /// <summary>
    /// Defines new <see cref="HierarchyDef"/> and adds it to the <see cref="DomainDef"/> instance.
    /// </summary>
    /// <param name="root">The <see cref="TypeDef"/> instance that will be the root of the hierarchy.</param>
    /// <param name="inheritanceSchema">The type of inheritance mapping.</param>
    /// <returns>Newly created <see cref="HierarchyDef"/> instance.</returns>
    public HierarchyDef DefineHierarchy(TypeDef root, InheritanceSchema inheritanceSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(root, "root");

      if (!root.IsEntity)
        throw new ArgumentException("Only entities could be hierarchy roots.");

      if (!types.Contains(root))
        throw new ArgumentException("TypeDef is not registered in the storage definition.");

      using (var scope = new LogCaptureScope(BuildingScope.Context.Logger)) {
        HierarchyDef hierarchy = HierarchyBuilder.DefineHierarchy(root, inheritanceSchema);
        if (scope.IsCaptured(LogEventTypes.Error))
          throw new DomainBuilderException(
            "Some errors have been occurred during hierarchy definition process. See error log for details.");
        hierarchies.Add(hierarchy);
        return hierarchy;
      }
    }

    /// <summary>
    /// Defines new <see cref="HierarchyDef"/> with <see cref="InheritanceSchema.Default"/> inheritance mapping type
    ///  and adds it to the <see cref="DomainDef"/> instance.
    /// </summary>
    /// <param name="root">The <see cref="TypeDef"/> instance that will be the root of the hierarchy.</param>
    /// <returns>Newly created <see cref="HierarchyDef"/> instance.</returns>
    public HierarchyDef DefineHierarchy(TypeDef root)
    {
      return DefineHierarchy(root, InheritanceSchema.ClassTableInheritance);
    }

    /// <summary>
    /// Defines new <see cref="ServiceDef"/> object and adds it to the <see cref="DomainDef"/> instance.
    /// </summary>
    /// <param name="type">The underlying type.</param>
    /// <returns>Newly created <see cref="ServiceDef"/> instance.</returns>
    public ServiceDef DefineService(Type type)
    {
      ValidationResult vr = Validator.ValidateType(type, ValidationRule.Service);
      if (!vr.Success)
        throw new ArgumentOutOfRangeException("type", vr.Message);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Finds the root of inheritance hierarchy for the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search root for.</param>
    /// <returns><see name="TypeDef"/> instance that is root of specified <paramref name="item"/> or 
    /// <see langword="null"/> if the root is not found in this collection.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public TypeDef FindRoot(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      TypeDef candidate = item;

      while (candidate != null) {
        HierarchyDef hd;
        if (hierarchies.TryGetValue(candidate, out hd))
          return candidate;
        candidate = types.FindAncestor(candidate);
      }
      return null;
    }

    /// <summary>
    /// Finds the hierarchy.
    /// </summary>
    /// <param name="item">The type to search hierarchy for.</param>
    /// <returns><see cref="HierarchyDef"/> instance or <see langword="null"/> if hierarchy is not found.</returns>
    public HierarchyDef FindHierarchy(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      TypeDef root = FindRoot(item);
      if (root != null) {
        HierarchyDef result;
        if (hierarchies.TryGetValue(root, out result))
          return result;
      }
      return null;
    }

    private void OnTypesCleared(object sender, ChangeNotifierEventArgs e)
    {
      hierarchies.Clear();
    }

    private void OnTypeRemoved(object sender, CollectionChangeNotifierEventArgs<TypeDef> e)
    {
      HierarchyDef hd;
      if (hierarchies.TryGetValue(e.Item, out hd))
        hierarchies.Remove(hd);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainDef"/> class.
    /// </summary>
    internal DomainDef()
    {
      services = new NodeCollection<ServiceDef>();
      types = new TypeDefCollection();
      hierarchies = new HierarchyDefCollection();

      types.Removed += OnTypeRemoved;
      types.Cleared += OnTypesCleared;
    }
  }
}
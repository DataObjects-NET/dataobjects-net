// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Notifications;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Model;

using System.Linq;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines the whole <see cref="Domain"/> model.
  /// The root definition.
  /// </summary>
  [Serializable]
  public sealed class DomainModelDef : Node
  {
    private readonly BuildingContext context;
    private readonly HierarchyDefCollection hierarchies;
    private readonly TypeDefCollection types;
    private readonly FullTextIndexDefCollection fullTextIndexes;

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
    /// Gets the collection of <see cref="FullTextIndexDef"/> instances contained in this instance.
    /// </summary>
    public FullTextIndexDefCollection FullTextIndexes
    {
      get { return fullTextIndexes; }
    }

    /// <summary>
    /// Defines new <see cref="TypeDef"/> and adds it to <see cref="DomainModelDef"/> instance.
    /// </summary>
    /// <param name="type">The underlying type.</param>
    /// <returns>Newly created <see cref="TypeDef"/> instance.</returns>
    public TypeDef DefineType(Type type)
    {
      Validator.EnsureTypeIsPersistent(type);

      if (types.Contains(type))
        throw new DomainBuilderException(string.Format(Strings.ExTypeXIsAlreadyDefined, type.GetFullName()));

      return ModelDefBuilder.ProcessType(context, type);
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
      var hierarchy = FindHierarchy(item);
      return hierarchy!=null ? hierarchy.Root : null;
    }

    /// <summary>
    /// Finds the hierarchy.
    /// </summary>
    /// <param name="item">The type to search hierarchy for.</param>
    /// <returns><see cref="HierarchyDef"/> instance or <see langword="null"/> if hierarchy is not found.</returns>
    public HierarchyDef FindHierarchy(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Hierarchies
        .FirstOrDefault(hierarchy => hierarchy.Root.UnderlyingType.IsAssignableFrom(item.UnderlyingType));
    }

    private void OnTypesCleared(object sender, ChangeNotifierEventArgs e)
    {
      hierarchies.Clear();
    }

    private void OnTypeRemoved(object sender, CollectionChangeNotifierEventArgs<TypeDef> e)
    {
      HierarchyDef hd = hierarchies.TryGetValue(e.Item);
      if (hd != null)
        hierarchies.Remove(hd);
    }

    // Constructors

    internal DomainModelDef(BuildingContext context)
    {
      this.context = context;

      types = new TypeDefCollection(this, "Types");
      hierarchies = new HierarchyDefCollection();
      fullTextIndexes = new FullTextIndexDefCollection();

      types.Removed += OnTypeRemoved;
      types.Cleared += OnTypesCleared;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// A collection of <see cref="TypeDef"/> items.
  /// </summary>
  public sealed class TypeDefCollection : NodeCollection<TypeDef>
  {
    private readonly Type baseType = typeof (object);
    private readonly Dictionary<Type, TypeDef> typeIndex;

    /// <summary>
    /// Finds the ancestor of the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The type to search ancestor for.</param>
    /// <returns><see name="TypeDef"/> instance that is ancestor of specified <paramref name="item"/> or 
    /// <see langword="null"/> if the ancestor is not found in this collection.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
    public TypeDef FindAncestor(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (item.IsInterface)
        return null;

      return FindAncestor(item.UnderlyingType);
    }

    /// <summary>
    /// Finds the ancestor of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to search ancestor for.</param>
    /// <returns><see name="TypeDef"/> instance that is ancestor of specified <paramref name="type"/> or 
    /// <see langword="null"/> if the ancestor is not found in this collection.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="type"/> is <see langword="null"/>.</exception>
    private TypeDef FindAncestor(Type type)
    {
      if (type == baseType || type.BaseType == null)
        return null;
      return Contains(type.BaseType) ? this[type.BaseType] : FindAncestor(type.BaseType);
    }

    /// <summary>
    /// Find the <see cref="IEnumerable{T}"/> of interfaces that specified <paramref name="type"/> implements.
    /// </summary>
    /// <param name="type">The type to search interfaces for.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see name="TypeDef"/> instance that are implemented by the specified <paramref name="type"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="type"/> is <see langword="null"/>.</exception>
    public IEnumerable<TypeDef> FindInterfaces(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      var interfaces = type.GetInterfaces();
      return interfaces.Select(t => TryGetValue(t)).Where(result => result != null);
    }

    /// <summary>
    /// Determines whether collection contains a specific item.
    /// </summary>
    /// <param name="item">Value to search for.</param>
    /// <returns>
    ///   <see langword="True"/> if the object is found; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Contains(TypeDef item)
    {
      if (item==null)
        return false;
      TypeDef result = TryGetValue(item.UnderlyingType);
      if (result == null)
        return false;
      return result==item;
    }

    /// <summary>
    /// Determines whether this instance contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Type key)
    {
      return typeIndex.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public TypeDef TryGetValue(Type key)
    {
      TypeDef result;
      typeIndex.TryGetValue(key, out result);
      return result;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public TypeDef this[Type key]
    {
      get
      {
        TypeDef result = TryGetValue(key);
        if (result == null)
          throw new ArgumentException(String.Format(Resources.Strings.ExItemByKeyXWasNotFound, key), "key");
        return result;

      }
    }

    protected override void OnInserted(TypeDef value, int index)
    {
      base.OnInserted(value, index);
      typeIndex[value.UnderlyingType] = value;
    }

    protected override void OnCleared()
    {
      base.OnCleared();
      typeIndex.Clear();
    }

    protected override void OnRemoved(TypeDef value, int index)
    {
      base.OnRemoved(value, index);
      typeIndex.Remove(value.UnderlyingType);
    }


    // Constructors

    internal TypeDefCollection(Node owner, string name)
      : base(owner, name)
    {
      typeIndex = new Dictionary<Type, TypeDef>();
    }
  }
}
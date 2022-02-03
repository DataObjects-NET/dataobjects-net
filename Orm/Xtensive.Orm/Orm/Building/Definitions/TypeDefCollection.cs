// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Definitions
{
  public sealed class TypeDefCollectionChangedEventArgs: EventArgs
  {
    public TypeDef Item { get; }

    public TypeDefCollectionChangedEventArgs(TypeDef item)
    {
      Item = item;
    }
  }

  public sealed class TypeDefCollectionClearedEventArgs: EventArgs {}

  /// <summary>
  /// A collection of <see cref="TypeDef"/> items.
  /// </summary>
  public sealed class TypeDefCollection : NodeCollection<TypeDef>
  {
    private readonly Dictionary<Type, TypeDef> typeIndex = new Dictionary<Type, TypeDef>();

    public event EventHandler<TypeDefCollectionChangedEventArgs> Added;

    public event EventHandler<TypeDefCollectionChangedEventArgs> Removed;

    public event EventHandler<TypeDefCollectionClearedEventArgs> Cleared;

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
    private TypeDef FindAncestor(Type type) =>
      type == WellKnownTypes.Object || type.BaseType == null
        ? null
        : TryGetValue(type.BaseType) ?? FindAncestor(type.BaseType);

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
      return interfaces.Select(TryGetValue).Where(result => result != null);
    }

    /// <summary>
    /// Determines whether collection contains a specific item.
    /// </summary>
    /// <param name="item">Value to search for.</param>
    /// <returns>
    ///   <see langword="True"/> if the object is found; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Contains(TypeDef item) =>
      item != null && TryGetValue(item.UnderlyingType) == item;

    /// <summary>
    /// Determines whether this instance contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Type key) => typeIndex.ContainsKey(key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public TypeDef TryGetValue(Type key)
    {
      typeIndex.TryGetValue(key, out var result);
      return result;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public TypeDef this[Type key] =>
      TryGetValue(key) ?? throw new ArgumentException(String.Format(Strings.ExItemByKeyXWasNotFound, key), "key");

    /// <inheritdoc/>
    public override void Add(TypeDef item) {
      base.Add(item);
      typeIndex[item.UnderlyingType] = item;
      Added?.Invoke(this, new TypeDefCollectionChangedEventArgs(item));
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<TypeDef> items)
    {
      this.EnsureNotLocked();
      foreach (var item in items) {
        Add(item);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(TypeDef item)
    {
      if (base.Remove(item)) {
        typeIndex.Remove(item.UnderlyingType);
        Removed?.Invoke(this, new TypeDefCollectionChangedEventArgs(item));
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override void Clear() {
      base.Clear();
      typeIndex.Clear();
      Cleared?.Invoke(this, new TypeDefCollectionClearedEventArgs());
    }

    // Constructors

    internal TypeDefCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}

// Copyright (C) 2003-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Collections;

namespace Xtensive.Orm.Building.Definitions
{
  public sealed class HierarchyDefCollectionChangedEventArgs: EventArgs
  {
    public HierarchyDef Item { get; }

    public HierarchyDefCollectionChangedEventArgs(HierarchyDef item)
    {
      Item = item;
    }
  }

  /// <summary>
  /// A collection of <see cref="HierarchyDef"/> items.
  /// </summary>
  public class HierarchyDefCollection : CollectionBaseSlim<HierarchyDef>
  {
    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Added;
    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Removed;

    /// <inheritdoc/>
    public override bool Contains(HierarchyDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return TryGetValue(item.Root) != null;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(TypeDef key)
    {
      return TryGetValue(key.UnderlyingType);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(Type key)
    {
      foreach (HierarchyDef item in this)
        if (item.Root.UnderlyingType==key)
          return item;
      return null;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public HierarchyDef this[Type key] =>
      TryGetValue(key) ?? throw new ArgumentException(String.Format(Strings.ExItemByKeyXWasNotFound, key), "key");

    /// <inheritdoc/>
    public override void Add(HierarchyDef item)
    {
      base.Add(item);
      Added?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<HierarchyDef> items)
    {
      foreach (var item in items) {
        Add(item);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(HierarchyDef item)
    {
      if (base.Remove(item)) {
        Removed?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (var item in this) {
        Removed?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
      }
      base.Clear();
    }

    private readonly Dictionary<Type, HierarchyDef> hierarchyDefByTypeCache = new();

    /// <summary>
    /// Finds the hierarchy.
    /// </summary>
    /// <param name="item">The type to search hierarchy for.</param>
    /// <returns><see cref="HierarchyDef"/> instance or <see langword="null"/> if hierarchy is not found.</returns>
    public HierarchyDef Find(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var itemUnderlyingType = item.UnderlyingType;

      if (!hierarchyDefByTypeCache.TryGetValue(itemUnderlyingType, out var hierarchyDef)) {
        hierarchyDef = this.FirstOrDefault(hierarchy => hierarchy.Root.UnderlyingType.IsAssignableFrom(itemUnderlyingType));
        hierarchyDefByTypeCache.Add(itemUnderlyingType, hierarchyDef);
      }
      return hierarchyDef;
    }

    private void ClearCache(object _, HierarchyDefCollectionChangedEventArgs __)
    {
      hierarchyDefByTypeCache.Clear();
    }

    public HierarchyDefCollection()
    {
      Added += ClearCache;
      Removed += ClearCache;
    }

    private void HierarchyDefCollection_Removed(object sender, HierarchyDefCollectionChangedEventArgs e) => throw new NotImplementedException();
  }
}

// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using Xtensive.Core;
using Xtensive.Collections;

namespace Xtensive.Orm.Building.Definitions
{
  public readonly struct HierarchyDefCollectionChangedEventArgs
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
    private readonly Dictionary<Type, HierarchyDef> hierarchyDefByTypeCache = new();
    private bool invalidateCache;

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
    public HierarchyDef TryGetValue(TypeDef key) => TryGetValue(key.UnderlyingType);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(Type key)
    {
      foreach (var item in this) {
        if (item.Root.UnderlyingType == key) {
          return item;
        }
      }
      return null;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public HierarchyDef this[Type key] =>
      TryGetValue(key) ?? throw new ArgumentException(string.Format(Strings.ExItemByKeyXWasNotFound, key), nameof(key));

    /// <inheritdoc/>
    public override void Add(HierarchyDef item)
    {
      base.Add(item);
      Added?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
      invalidateCache = true;
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
        invalidateCache = true;
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
      invalidateCache = true;
      base.Clear();
    }

    /// <summary>
    /// Finds the hierarchy.
    /// </summary>
    /// <param name="item">The type to search hierarchy for.</param>
    /// <returns><see cref="HierarchyDef"/> instance or <see langword="null"/> if hierarchy is not found.</returns>
    public HierarchyDef Find(TypeDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var itemUnderlyingType = item.UnderlyingType;

      HierarchyDef hierarchyDef;
      if (invalidateCache) {
        hierarchyDefByTypeCache.Clear();
        invalidateCache = false;

        FindAndCache(itemUnderlyingType, out hierarchyDef);
      }
      else if (!hierarchyDefByTypeCache.TryGetValue(itemUnderlyingType, out hierarchyDef)) {
        FindAndCache(itemUnderlyingType, out hierarchyDef);
      }
      return hierarchyDef;

      void FindAndCache(Type underlyingType, out HierarchyDef hierarchyDef1)
      {
        hierarchyDef1 = this.FirstOrDefault(hierarchy => hierarchy.Root.UnderlyingType.IsAssignableFrom(underlyingType));
        hierarchyDefByTypeCache.Add(itemUnderlyingType, hierarchyDef1);
      }
    }
  }
}

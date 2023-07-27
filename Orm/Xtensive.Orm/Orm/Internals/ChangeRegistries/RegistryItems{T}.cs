// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections;
using System.Collections.Generic;


namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Items of registry (e.g. <see cref="EntityChangeRegistry"/> or <see cref="EntitySetChangeRegistry"/>)
  /// </summary>
  public readonly struct RegistryItems<T> : IReadOnlyCollection<T>
  {
    private readonly HashSet<T> hashSet;

    /// <inheritdoc/>
    public int Count => hashSet.Count;

    /// <summary>
    /// Gets bare enumerator of internal collection so faster
    /// enumeration is available
    /// </summary>
    /// <returns><see cref="HashSet{T}.Enumerator"/>.</returns>
    public HashSet<T>.Enumerator GetEnumerator()
      => hashSet.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
      => hashSet.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
      => hashSet.GetEnumerator();

    public RegistryItems(HashSet<T> hashset)
    {
      hashSet = hashset;
    }
  }
}
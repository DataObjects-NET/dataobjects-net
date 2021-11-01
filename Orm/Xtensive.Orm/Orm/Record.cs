// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Orm.Internals;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// A single item in <see cref="EntityDataReader.Read"/> result
  /// containing both raw <see cref="Source"/> and parsed primary keys.
  /// </summary>
  public readonly struct Record
  {
    private readonly Pair<Key, Tuple> keyTuplePair;
    private readonly Pair<Key, Tuple>[] keyTuplePairs;

    /// <summary>
    /// Gets raw tuple this record is build from.
    /// </summary>
    public readonly Tuple Source { get; }

    /// <summary>
    /// Gets the first primary key in the <see cref="Record"/>.
    /// </summary>
    public Key GetKey() => keyTuplePair.First;

    /// <summary>
    /// Gets the <see cref="Key"/> by specified index.
    /// </summary>
    public Key GetKey(int index) => GetPair(index)?.First;

    /// <summary>
    /// Gets the first tuple in the <see cref="Record"/>.
    /// </summary>
    public Tuple GetTuple() => keyTuplePair.Second;

    /// <summary>
    /// Gets the <see cref="Tuple"/> by specified index.
    /// </summary>
    public Tuple GetTuple(int index) => GetPair(index)?.Second;

    private Pair<Key, Tuple>? GetPair(int index) =>
      index == 0 ? keyTuplePair
      : keyTuplePairs == null || index < 0 || index >= keyTuplePairs.Length ? null
      : keyTuplePairs[index];

    /// <summary>
    /// Gets the key count.
    /// </summary>
    public int Count => keyTuplePairs?.Length ?? 1;


    // Constructors

    internal Record(Tuple tuple, IEnumerable<Pair<Key, Tuple>> keyTuplePairs)
    {
      Source = tuple;
      this.keyTuplePairs = keyTuplePairs.ToArray();
      keyTuplePair = this.keyTuplePairs[0];
    }

    internal Record(Tuple tuple, Pair<Key, Tuple> keyTuplePair)
    {
      Source = tuple;
      keyTuplePairs = null;
      this.keyTuplePair = keyTuplePair;
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  /// A single item in <see cref="RecordSetReader.Read"/> result 
  /// containing both raw <see cref="Source"/> and parsed primary keys.
  /// </summary>
  public sealed class Record
  {
    private readonly Pair<Key, Tuple> keyTuplePair;
    private readonly Pair<Key, Tuple>[] keyTuplePairs;

    /// <summary>
    /// Gets the first primary key in the <see cref="Record"/>.
    /// </summary>
    public Key GetKey()
    {
      return keyTuplePair.First;
    }

    /// <summary>
    /// Gets the <see cref="Key"/> by specified index.
    /// </summary>
    public Key GetKey(int index)
    {
      if (index == 0)
        return keyTuplePair.First;
      if (keyTuplePairs == null || index < 0 || index >= keyTuplePairs.Length)
        return null;
      return keyTuplePairs[index].First;
    }

    /// <summary>
    /// Gets the first tuple in the <see cref="Record"/>.
    /// </summary>
    public Tuple GetTuple()
    {
      return keyTuplePair.Second;
    }

    /// <summary>
    /// Gets the <see cref="Tuple"/> by specified index.
    /// </summary>
    public Tuple GetTuple(int index)
    {
      if (index == 0)
        return keyTuplePair.Second;
      if (keyTuplePairs == null || index < 0 || index >= keyTuplePairs.Length)
        return null;
      return keyTuplePairs[index].Second;
    }

    /// <summary>
    /// Gets the key count.
    /// </summary>
    public int Count
    {
      get { return keyTuplePairs == null ? 1 : keyTuplePairs.Length; }
    }

    /// <summary>
    /// Gets raw tuple this record is build from.
    /// </summary>
    public Tuple Source { get; private set; }


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
      this.keyTuplePair = keyTuplePair;
    }

  }
}
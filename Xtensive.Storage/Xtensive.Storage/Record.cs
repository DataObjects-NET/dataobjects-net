// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  /// <summary>
  /// A single item in <see cref="RecordSetExtensions.Parse"/> result 
  /// containing both raw <see cref="Tuple"/> and parsed primary keys.
  /// </summary>
  public sealed class Record
  {
    private readonly Key key;
    private readonly Key[] keys;

    /// <summary>
    /// Gets the first primary key in the <see cref="Record"/>.
    /// </summary>
    public Key GetKey()
    {
      return key;
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.Key"/> by specified index.
    /// </summary>
    public Key GetKey(int index)
    {
      if (index == 0)
        return key;
      if (keys == null || index < 0 || index >= keys.Length)
        return null;
      return keys[index];
    }

    /// <summary>
    /// Gets the key count.
    /// </summary>
    public int KeyCount
    {
      get { return keys == null ? 1 : keys.Length; }
    }

    /// <summary>
    /// Gets raw tuple this record is build from.
    /// </summary>
    public Tuple Tuple { get; private set; }


    // Constructors

    internal Record(Tuple tuple, IEnumerable<Key> keys)
    {
      Tuple = tuple;
      this.keys = keys.ToArray();
      key = this.keys[0];
    }

    internal Record(Tuple tuple, Key key)
    {
      Tuple = tuple;
      this.key = key;
    }

  }
}
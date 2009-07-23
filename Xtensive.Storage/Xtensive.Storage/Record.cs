// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  /// <summary>
  /// A single item in <see cref="RecordSetExtensions.Parse"/> result 
  /// containing both raw <see cref="Tuple"/> and parsed <see cref="PrimaryKeys"/>.
  /// </summary>
  public sealed class Record
  {
    private readonly ReadOnlyList<Key> primaryKeys;

    /// <summary>
    /// Gets the first primary key in the <see cref="Record"/>.
    /// </summary>
    public Key DefaultKey {
      get {
        if (primaryKeys.Count > 0)
          return primaryKeys[0];
        return null;
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.Key"/> by specified column group.
    /// </summary>
    public Key this[int columnGroup] {
      get {
        if (columnGroup < 0 || columnGroup >= primaryKeys.Count)
          return null;
        return primaryKeys[columnGroup];
      }
    }

    /// <summary>
    /// Gets raw tuple this record is build from.
    /// </summary>
    public Tuple Tuple { get; private set; }

    /// <summary>
    /// Gets the primary keys collection.
    /// </summary>
    public ReadOnlyList<Key> PrimaryKeys {
      get { return primaryKeys; }
    }


    // Constructors

    internal Record(Tuple tuple, IList<Key> primaryKeys)
    {
      Tuple = tuple;
      this.primaryKeys = new ReadOnlyList<Key>(primaryKeys);
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains raw <see cref="Data"/> as a <see cref="Tuple"/> and parsed <see cref="PrimaryKeys"/>.
  /// </summary>
  public sealed class Record
  {
    private readonly ReadOnlyList<Key> primaryKeys;

    /// <summary>
    /// Gets the first primary key in the <see cref="Record"/>.
    /// </summary>
    public Key DefaultKey
    {
      get
      {
        if (primaryKeys.Count > 0)
          return primaryKeys[0];
        return null;
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.Key"/> by specified column group.
    /// </summary>
    public Key this[int columnGroup]
    {
      get
      {
        if (columnGroup < 0 || columnGroup >= primaryKeys.Count)
          return null;
        return primaryKeys[columnGroup];
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.Key"/> by specified column group and foreign key <see cref="FieldInfo"/>.
    /// </summary>
    public Key this[int columnGroup, FieldInfo foreignKeyField]
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Gets raw data.
    /// </summary>
    public Tuple Data { get; private set; }

    /// <summary>
    /// Gets the primary keys collection.
    /// </summary>
    public ReadOnlyList<Key> PrimaryKeys
    {
      get { return primaryKeys; }
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    internal Record(Tuple data, IList<Key> primaryKeys)
    {
      Data = data;
      this.primaryKeys = new ReadOnlyList<Key>(primaryKeys);
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public sealed class Record
  {
    private readonly ReadOnlyList<Key> primaryKeys;

    public Key DefaultKey
    {
      get
      {
        if (primaryKeys.Count > 0)
          return primaryKeys[0];
        return null;
      }
    }

    public Key this[int columnGroup]
    {
      get
      {
        if (columnGroup < 0 || columnGroup >= primaryKeys.Count)
          return null;
        return primaryKeys[columnGroup];
      }
    }

    public Key this[int columnGroup, FieldInfo foreignKeyField]
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public Tuple Data { get; private set; }


    // Constructors

    public Record(Tuple data, IList<Key> primaryKeys)
    {
      Data = data;
      this.primaryKeys = new ReadOnlyList<Key>(primaryKeys);
    }
  }
}
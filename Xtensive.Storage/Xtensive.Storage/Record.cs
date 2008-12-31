// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.30

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public sealed class Record
  {
    public Key this[int columnGroup]
    {
      get
      {
        throw new NotImplementedException();
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
  }
}
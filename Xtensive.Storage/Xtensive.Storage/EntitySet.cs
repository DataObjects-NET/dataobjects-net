// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public abstract class EntitySet : SessionBound,
    IFieldHandler
  {
    /// <inheritdoc/>
    public abstract Persistent Owner { get; internal set; }

    /// <inheritdoc/>
    public abstract FieldInfo Field { get; internal set; }

    internal static IFieldHandler Activate(Type type, Persistent obj, FieldInfo field)
    {
      throw new NotImplementedException();
    }
  }
}
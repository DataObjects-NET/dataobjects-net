// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.08

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal abstract class CachingFieldAccessor<T> : FieldAccessor<T>
  {
    protected static Func<Persistent, FieldInfo, IFieldValueAdapter> ctor;

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureGenericParameterIsValid(field);
      return (T) obj.GetFieldValueAdapter(field, ctor);
    }
  }
}
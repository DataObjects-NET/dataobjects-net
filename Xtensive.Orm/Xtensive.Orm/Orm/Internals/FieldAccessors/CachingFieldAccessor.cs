// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.08

using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal abstract class CachingFieldAccessor<T> : FieldAccessor<T>
  {
    public static Func<Persistent, FieldInfo, IFieldValueAdapter> Constructor;

    public override T GetValue(Persistent obj)
    {
      var field = Field;
      return (T) obj.GetFieldValueAdapter(field, Constructor);
    }
  }
}
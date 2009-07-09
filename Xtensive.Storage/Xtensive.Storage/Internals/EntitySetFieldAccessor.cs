// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class EntitySetFieldAccessor<T> : CachingFieldAccessor<T> 
  {
    public static readonly FieldAccessor<T> Instance = new EntitySetFieldAccessor<T>();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always thrown by this method.</exception>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      throw new InvalidOperationException(Strings.ExEntitySetCanTBeAssigned);
    }


    static EntitySetFieldAccessor()
    {
       ctor = (obj, field) => Activator.CreateEntitySet((Entity) obj, field);
    }
  }
}
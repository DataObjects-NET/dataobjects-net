// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class EntitySetFieldAccessor<T> : CachingFieldAccessor<T> 
  {
    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      return ReferenceEquals(oldValue, newValue);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always thrown by this method.</exception>
    public override void SetValue(Persistent obj, T value)
    {
      throw new InvalidOperationException(Strings.ExEntitySetCanTBeAssigned);
    }

    // Type initializer

    static EntitySetFieldAccessor()
    {
       Constructor = (obj, field) => Activator.CreateEntitySet((Entity) obj, field);
    }
  }
}
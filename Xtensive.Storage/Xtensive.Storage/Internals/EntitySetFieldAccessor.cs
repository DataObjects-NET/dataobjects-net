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
  internal class EntitySetFieldAccessor<T> : FieldAccessorBase<T> 
  {
    private static readonly FieldAccessorBase<T> instance = new EntitySetFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field, bool notify)
    {
      ValidateType(field);
      IFieldValueAdapter result;
      if (obj.FieldHandlers.TryGetValue(field, out result))
        return (T)result;
      result = Activator.CreateEntitySet((Entity) obj, field, notify);
      obj.FieldHandlers.Add(field, result);
      return (T) result;

    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always thrown by this method.</exception>
    public override void SetValue(Persistent obj, FieldInfo field, T value, bool notify)
    {
      throw new InvalidOperationException(Strings.ExEntitySetCanTBeAssigned);
    }
  }
}
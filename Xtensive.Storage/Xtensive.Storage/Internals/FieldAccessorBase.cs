// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.02

using System;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  internal abstract class FieldAccessorBase<T>
  {
    public abstract void SetValue(Persistent obj, FieldInfo field, T value);
    public abstract T GetValue(Persistent obj, FieldInfo field);

    public static void ValidateType(FieldInfo fieldInfo)
    {
      Type resultType = typeof(T);
      Type valueType = fieldInfo.ValueType;
      if (!resultType.IsAssignableFrom(valueType))
        throw new InvalidOperationException(String.Format(Strings.ExResultTypeIncorrect, valueType.Name, resultType.Name));
    }
  }
}
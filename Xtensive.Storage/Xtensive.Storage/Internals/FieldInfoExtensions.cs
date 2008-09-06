// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.04

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class FieldInfoExtensions
  {
    public static FieldAccessorBase<T> GetAccessor<T>(this FieldInfo field)
    {
      if (field.IsEntity)
        return EntityFieldAccessor<T>.Instance;
      if (field.IsStructure)
        return StructureFieldAccessor<T>.Instance;
      if (field.IsEnum)
        return EnumFieldAccessor<T>.Instance;
      if (field.IsEntitySet)
        return EntitySetFieldAccessor<T>.Instance;
      return DefaultFieldAccessor<T>.Instance;
    }
  }
}
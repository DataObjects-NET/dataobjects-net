// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.22

using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals.Prefetch
{
  internal static class PrefetchHelper
  {
    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || (!field.IsLazyLoad && !field.IsEntitySet);
    }

    public static PrefetchFieldDescriptor[] CreateDescriptorsForFieldsLoadedByDefault(TypeInfo type)
    {
      return type.Fields.Where(field => field.Parent==null && IsFieldToBeLoadedByDefault(field))
        .Select(field => new PrefetchFieldDescriptor(field, false, false)).ToArray();
    }

    public static bool? TryGetExactKeyType(Key key, PrefetchProcessor processor, out TypeInfo type)
    {
      type = null;
      if (!key.TypeRef.Type.IsLeaf) {
        var cachedKey = key;
        EntityState state;
        if (!processor.TryGetTupleOfNonRemovedEntity(ref cachedKey, out state))
          return null;
        if (cachedKey.HasExactType) {
          type = cachedKey.TypeRef.Type;
          return true;
        }
        return false;
      }
      type = key.TypeRef.Type;
      return true;
    }
  }
}
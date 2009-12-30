// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals.Prefetch
{
  internal static class PrefetchHelper
  {
    private static readonly object descriptorArraysCachingRegion = new object();

    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || (!field.IsLazyLoad && !field.IsEntitySet);
    }

    public static FieldDescriptorCollection CreateDescriptorsForFieldsLoadedByDefault(TypeInfo type)
    {
      return new FieldDescriptorCollection(type.Fields
        .Where(field => field.Parent==null && IsFieldToBeLoadedByDefault(field))
        .Select(field => new PrefetchFieldDescriptor(field, false, false)));
    }

    public static FieldDescriptorCollection GetCachedDescriptorsForFieldsLoadedByDefault(Domain domain,
      TypeInfo type)
    {
      object key = new Pair<object, TypeInfo>(descriptorArraysCachingRegion, type);
      Func<object, object> generator = pair => CreateDescriptorsForFieldsLoadedByDefault(((Pair<object, TypeInfo>) pair).Second);
      return (FieldDescriptorCollection) domain.Cache.GetValue(key, generator);
    }

    public static bool? TryGetExactKeyType(Key key, PrefetchManager manager, out TypeInfo type)
    {
      type = null;
      if (!key.TypeRef.Type.IsLeaf) {
        var cachedKey = key;
        EntityState state;
        if (!manager.TryGetTupleOfNonRemovedEntity(ref cachedKey, out state))
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

    public static SortedDictionary<int, ColumnInfo> GetColumns(IEnumerable<ColumnInfo> candidateColumns,
      TypeInfo type)
    {
      var columns = new SortedDictionary<int, ColumnInfo>();
      AddColumns(candidateColumns, columns, type);
      return columns;
    }

    public static bool AddColumns(IEnumerable<ColumnInfo> candidateColumns,
      SortedDictionary<int, ColumnInfo> columns, TypeInfo type)
    {
      var result = false;
      var primaryIndex = type.Indexes.PrimaryIndex;
      foreach (var column in candidateColumns) {
        result = true;
        if (type.IsInterface == column.Field.DeclaringType.IsInterface)
          columns[type.Fields[column.Field.Name].MappingInfo.Offset] = column;
        else if (column.Field.DeclaringType.IsInterface)
          columns[type.FieldMap[column.Field].MappingInfo.Offset] = column;
        else
          throw new InvalidOperationException();
      }
      return result;
    }

    public static List<int> GetColumnsToBeLoaded(SortedDictionary<int, ColumnInfo> userColumnIndexes,
      TypeInfo type)
    {
      var result = new List<int>(userColumnIndexes.Count);
      result.AddRange(type.Indexes.PrimaryIndex.ColumnIndexMap.System);
      result.AddRange(userColumnIndexes.Where(pair => !pair.Value.IsPrimaryKey
        && !pair.Value.IsSystem).Select(pair => pair.Key));
      return result;
    }
  }
}
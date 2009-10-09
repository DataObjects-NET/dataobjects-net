// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.09

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal static class KeyFactory
  {
    private const string GenericKeyNameFormat = "{0}.{1}`{2}";

    /// <exception cref="ArgumentException">Wrong key structure for the specified <paramref name="type"/>.</exception>
    internal static Key Create(Domain domain, TypeInfo type, Tuple value, int[] keyIndexes, bool exactType, bool canCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      var hierarchy = type.Hierarchy;
      var keyInfo = hierarchy.KeyInfo;
      if (keyIndexes==null) {
        if (value.Descriptor!=keyInfo.TupleDescriptor)
          throw new ArgumentException(Strings.ExWrongKeyStructure);
        if (exactType) {
          int typeIdColumnIndex = keyInfo.TypeIdColumnIndex;
          if (typeIdColumnIndex >= 0 && !value.GetFieldState(typeIdColumnIndex).IsAvailable())
            // Ensures TypeId is filled in into Keys of newly created Entities
            value.SetValue(typeIdColumnIndex, type.TypeId);
        }
      }
      if (hierarchy.Root.IsLeaf) {
        exactType = true;
        canCache = false; // No reason to cache
      }

      Key key;
      var isGenericKey = keyInfo.Length <= WellKnown.MaxGenericKeyLength;
      if (isGenericKey)
        key = CreateGenericKey(domain, type.Hierarchy, exactType ? type : null, value, keyIndexes);
      else {
        if (keyIndexes!=null)
          throw Exceptions.InternalError(Strings.ExKeyIndexesAreSpecifiedForNonGenericKey, LogTemplate<Log>.Instance);
        key = new Key(type.Hierarchy, exactType ? type : null, value);
      }
      if (!canCache || domain==null)
        return key;
      var keyCache = domain.KeyCache;
      lock (keyCache) {
        Key foundKey;
        if (keyCache.TryGetItem(key, true, out foundKey))
          key = foundKey;
        else {
          if (exactType)
            keyCache.Add(key);
        }
      }
      return key;
    }

    private static Key CreateGenericKey(Domain domain, HierarchyInfo hierarchy, TypeInfo type,
      Tuple tuple, int[] keyIndexes)
    {
      var keyTypeInfo = domain.genericKeyTypes.GetValue(
        hierarchy.Root.TypeId, BuildGenericKeyTypeInfo, hierarchy);
      if (keyIndexes==null)
        return keyTypeInfo.DefaultConstructor(hierarchy, type, tuple);
      return keyTypeInfo.KeyIndexBasedConstructor(hierarchy, type, tuple, keyIndexes);
    }

    private static GenericKeyTypeInfo BuildGenericKeyTypeInfo(int typeId, HierarchyInfo hierarchy)
    {
      var descriptor = hierarchy.KeyInfo.TupleDescriptor;
      int length = descriptor.Count;
      var type = typeof (Key).Assembly.GetType(
        String.Format(GenericKeyNameFormat, typeof (Key<>).Namespace, typeof(Key).Name, length));
      type = type.MakeGenericType(descriptor.ToArray());
      return new GenericKeyTypeInfo(type,
        DelegateHelper.CreateDelegate<Func<HierarchyInfo, TypeInfo, Tuple, Key>>(null,
          type, "Create", ArrayUtils<Type>.EmptyArray),
        DelegateHelper.CreateDelegate<Func<HierarchyInfo, TypeInfo, Tuple, int[], Key>>(null, type,
          "Create", ArrayUtils<Type>.EmptyArray));
    }
  }
}
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
    internal static Key Create(TypeInfo type, Tuple value, int[] keyIndexes, bool exactType, bool canCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");

      var domain = Domain.Demand();
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

    internal static Key CreateNext(TypeInfo type)
    {
      var domain = Domain.Demand();
      Tuple keyValue = GenerateKeyValue(domain, type.Hierarchy);
      return Create(type, keyValue, null, true, false);
    }

    private static Tuple GenerateKeyValue(Domain domain, HierarchyInfo hierarchy)
    {
      var keyGenerator = domain.KeyGenerators[hierarchy.GeneratorInfo];
      if (keyGenerator==null)
        throw new InvalidOperationException(
          String.Format(Strings.ExUnableToCreateKeyForXHierarchy, hierarchy));
      return keyGenerator.Next();
    }

    internal static Key Create(TypeInfo type, bool exactType, params object[] values)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      var keyInfo = type.Hierarchy.KeyInfo;
      ArgumentValidator.EnsureArgumentIsInRange(values.Length, 1, keyInfo.TupleDescriptor.Count, "values");

      var tuple = Tuple.Create(keyInfo.TupleDescriptor);
      int typeIdIndex = keyInfo.TypeIdColumnIndex;
      if (typeIdIndex>=0)
        tuple.SetValue(typeIdIndex, type.TypeId);

      int tupleIndex = 0;
      if (tupleIndex==typeIdIndex)
        tupleIndex++;
      for (int valueIndex = 0; valueIndex < values.Length; valueIndex++) {
        var value = values[valueIndex];
        ArgumentValidator.EnsureArgumentNotNull(value, String.Format("values[{0}]", valueIndex));
        var entity = value as Entity;
        if (entity!=null)
          value = entity.Key;
        var key = value as Key;
        if (key!=null) {
          if (key.Hierarchy==type.Hierarchy)
            typeIdIndex = -1; // Key must be fully copied in this case
          for (int keyIndex = 0; keyIndex < key.Value.Count; keyIndex++) {
            tuple.SetValue(tupleIndex++, key.Value.GetValueOrDefault(keyIndex));
            if (tupleIndex==typeIdIndex)
              tupleIndex++;
          }
          continue;
        }
        else {
          tuple.SetValue(tupleIndex++, value);
          if (tupleIndex==typeIdIndex)
            tupleIndex++;
        }
      }
      if (tupleIndex != tuple.Count)
        throw new ArgumentException(String.Format(
          Strings.ExSpecifiedValuesArentEnoughToCreateKeyForTypeX, type.Name));

      return Create(type, tuple, null, exactType, false);
    }
  }
}
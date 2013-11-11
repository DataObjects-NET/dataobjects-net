// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.09

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal static class KeyFactory
  {
    private const string GenericKeyNameFormat = "{0}.{1}`{2}";

    public static Key Generate(Session session, TypeInfo typeInfo)
    {
      if (!typeInfo.IsEntity)
        throw new InvalidOperationException(String.Format(Strings.ExCouldNotConstructNewKeyInstanceTypeXIsNotAnEntity, typeInfo));
      var domain = session.Domain;
      var keyGenerator = domain.KeyGenerators.Get(typeInfo.Key, session.IsDisconnected);
      if (keyGenerator==null)
        throw new InvalidOperationException(String.Format(Strings.ExUnableToCreateKeyForXHierarchy, typeInfo.Hierarchy));
      var keyValue = keyGenerator.GenerateKey(typeInfo.Key, session);
      var key = Materialize(domain, typeInfo, keyValue, TypeReferenceAccuracy.ExactType, false, null);

      return key;
    }

    public static Key Materialize(Domain domain, TypeInfo type, Tuple value, TypeReferenceAccuracy accuracy, bool canCache, int[] keyIndexes)
    {
      var hierarchy = type.Hierarchy;
      var keyInfo = type.Key;
      if (keyIndexes==null) {
        if (value.Descriptor!=keyInfo.TupleDescriptor)
          throw new ArgumentException(Strings.ExWrongKeyStructure);
        if (accuracy == TypeReferenceAccuracy.ExactType) {
          int typeIdColumnIndex = keyInfo.TypeIdColumnIndex;
          if (typeIdColumnIndex >= 0 && !value.GetFieldState(typeIdColumnIndex).IsAvailable())
            // Ensures TypeId is filled in into Keys of newly created Entities
            value.SetValue(typeIdColumnIndex, type.TypeId);
        }
      }
      if (hierarchy != null && hierarchy.Root.IsLeaf) {
        accuracy = TypeReferenceAccuracy.ExactType;
        canCache = false; // No reason to cache
      }

      Key key;
      var isGenericKey = keyInfo.TupleDescriptor.Count <= WellKnown.MaxGenericKeyLength;
      if (isGenericKey)
        key = CreateGenericKey(domain, type, accuracy, value, keyIndexes);
      else {
        if (keyIndexes!=null)
          throw Exceptions.InternalError(Strings.ExKeyIndexesAreSpecifiedForNonGenericKey, OrmLog.Instance);
        key = new LongKey(type, accuracy, value);
      }
      if (!canCache)
        return key;
      var keyCache = domain.KeyCache;
      lock (keyCache) {
        Key foundKey;
        if (keyCache.TryGetItem(key, true, out foundKey))
          key = foundKey;
        else {
          if (accuracy == TypeReferenceAccuracy.ExactType)
            keyCache.Add(key);
        }
      }
      return key;
    }

    public static Key Materialize(Domain domain, TypeInfo type, TypeReferenceAccuracy accuracy, params object[] values)
    {
      var keyInfo = type.Key;
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
        if (entity!=null) {
          entity.EnsureNotRemoved();
          value = entity.Key;
        }
        var key = value as Key;
        if (key!=null) {
          if (key.TypeReference.Type.Hierarchy==type.Hierarchy)
            typeIdIndex = -1; // Key must be fully copied in this case
          for (int keyIndex = 0; keyIndex < key.Value.Count; keyIndex++) {
            tuple.SetValue(tupleIndex++, key.Value.GetValueOrDefault(keyIndex));
            if (tupleIndex==typeIdIndex)
              tupleIndex++;
          }
          continue;
        }
        tuple.SetValue(tupleIndex++, value);
        if (tupleIndex==typeIdIndex)
          tupleIndex++;
      }
      if (tupleIndex != tuple.Count)
        throw new ArgumentException(String.Format(
          Strings.ExSpecifiedValuesArentEnoughToCreateKeyForTypeX, type.Name));

      return Materialize(domain, type, tuple, accuracy, false, null);
    }


    public static bool IsValidKeyTuple(Tuple tuple)
    {
      var limit = tuple.Descriptor.Count;
      for (int i = 0; i < limit; i++)
        if (tuple.GetFieldState(i).IsNull())
          return false;
      return true;
    }

    public static bool IsValidKeyTuple(Tuple tuple, int[] keyIndexes)
    {
      if (keyIndexes==null)
        return IsValidKeyTuple(tuple);
      var limit = keyIndexes.Length;
      for (int i = 0; i < limit; i++)
        if (tuple.GetFieldState(keyIndexes[i]).IsNull())
          return false;
      return true;
    }

    private static Key CreateGenericKey(Domain domain, TypeInfo type, TypeReferenceAccuracy accuracy, Tuple tuple, int[] keyIndexes)
    {
      var keyTypeInfo = domain.GenericKeyTypes.GetValue(type.TypeId, BuildGenericKeyTypeInfo, type);
      if (keyIndexes==null)
        return keyTypeInfo.DefaultConstructor(type, tuple, accuracy);
      return keyTypeInfo.KeyIndexBasedConstructor(type, tuple, accuracy, keyIndexes);
    }

    private static GenericKeyTypeInfo BuildGenericKeyTypeInfo(int typeId, TypeInfo typeInfo)
    {
      var descriptor = typeInfo.Key.TupleDescriptor;
      int length = descriptor.Count;
      var keyType = typeof (Key).Assembly.GetType(
        String.Format(GenericKeyNameFormat, typeof (Key<>).Namespace, typeof(Key).Name, length));
      keyType = keyType.MakeGenericType(descriptor.ToArray());
      return new GenericKeyTypeInfo(keyType,
        DelegateHelper.CreateDelegate<Func<TypeInfo, Tuple, TypeReferenceAccuracy, Key>>(null, keyType, "Create", ArrayUtils<Type>.EmptyArray),
        DelegateHelper.CreateDelegate<Func<TypeInfo, Tuple, TypeReferenceAccuracy, int[], Key>>(null, keyType,
          "Create", ArrayUtils<Type>.EmptyArray));
    }
  }
}
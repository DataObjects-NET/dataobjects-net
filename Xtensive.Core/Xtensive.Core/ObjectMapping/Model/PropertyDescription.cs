// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.Threading;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  [DebuggerDisplay("{SystemProperty} in {ReflectedType.SystemType}")]
  public abstract class PropertyDescription : LockableBase
  {
    #region Nested classes

    private class MemberInfoCacheEntry
    {
      public readonly PropertyInfo Count;

      public readonly MethodInfo Add;


      // Constrcutors

      public MemberInfoCacheEntry(PropertyInfo count, MethodInfo add)
      {
        Count = count;
        Add = add;
      }
    }

    #endregion

    private static readonly HashSet<Type> primitiveTypes;
    private static readonly ThreadSafeDictionary<Type, Pair<Type, MemberInfoCacheEntry>> collectionTypes =
      ThreadSafeDictionary<Type, Pair<Type, MemberInfoCacheEntry>>.Create(new object());
    private static readonly MemberInfoCacheEntry arrayCacheEntry =
      new MemberInfoCacheEntry(typeof (Array).GetProperty("Length"), null);

    private bool isCollection;

    public readonly bool IsPrimitive;

    public TypeDescription ReflectedType { get; private set; }

    public readonly PropertyInfo SystemProperty;
    
    public bool IsCollection {
      get { return isCollection; }
      internal set{
        this.EnsureNotLocked();
        isCollection = value;
      }
    }

    public Type ItemType { get; private set; }

    public PropertyInfo CountProperty { get; private set; }

    public MethodInfo AddMethod { get; private set; }

    internal static bool IsPropertyPrimitive(PropertyInfo propertyInfo)
    {
      return primitiveTypes.Contains(propertyInfo.PropertyType);
    }


    // Constructors

    protected PropertyDescription(PropertyInfo systemProperty, TypeDescription reflectedType)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemProperty, "systemProperty");
      ArgumentValidator.EnsureArgumentNotNull(reflectedType, "reflectedType");

      SystemProperty = systemProperty;
      ReflectedType = reflectedType;
      IsPrimitive = primitiveTypes.Contains(systemProperty.PropertyType);
      AssignCollectionRelatedProperties(systemProperty);
    }

    private void AssignCollectionRelatedProperties(PropertyInfo systemProperty)
    {
      Pair<Type, MemberInfoCacheEntry>? cacheItem = null;
      Pair<Type, MemberInfoCacheEntry> foundItem;
      if (collectionTypes.TryGetValue(systemProperty.PropertyType, out foundItem))
        cacheItem = foundItem;
      else if (systemProperty.PropertyType.IsArray)
        cacheItem = GenerateArrayCacheItem(systemProperty);
      else if (systemProperty.PropertyType.IsGenericType
        && typeof (IEnumerable).IsAssignableFrom(systemProperty.PropertyType))
        cacheItem = GenerateCollectionCacheItem(systemProperty);
      if (cacheItem!=null) {
        IsCollection = true;
        ItemType = cacheItem.Value.First;
        CountProperty = cacheItem.Value.Second.Count;
        AddMethod = cacheItem.Value.Second.Add;
      }
    }

    private static Pair<Type, MemberInfoCacheEntry>? GenerateCollectionCacheItem(PropertyInfo systemProperty)
    {
      Pair<Type, MemberInfoCacheEntry>? result = null;
      var interfaces = systemProperty.PropertyType.GetInterfaces();
      for (var i = 0; i < interfaces.Length; i++) {
        var interfaceType = interfaces[i];
        if (interfaceType.IsGenericType) {
          var interfaceGenericDefinition = interfaceType.GetGenericTypeDefinition();
          if (interfaceGenericDefinition==typeof (ICollection<>)) {
            result = collectionTypes.GetValue(systemProperty.PropertyType,
              GenerateCollectionTypesCacheItem, interfaceType);
            break;
          }
        }
      }
      return result;
    }

    private static Pair<Type, MemberInfoCacheEntry> GenerateArrayCacheItem(PropertyInfo systemProperty)
    {
      return collectionTypes.GetValue(systemProperty.PropertyType,
        (type, propertyCacheEntry) =>
          new Pair<Type, MemberInfoCacheEntry>(type.GetElementType(), propertyCacheEntry),
        arrayCacheEntry);
    }

    private static Pair<Type, MemberInfoCacheEntry> GenerateCollectionTypesCacheItem(Type type,
      Type interfaceType)
    {
      return new Pair<Type, MemberInfoCacheEntry>(interfaceType.GetGenericArguments()[0],
        new MemberInfoCacheEntry(interfaceType.GetProperty("Count"), interfaceType.GetMethod("Add")));
    }

    static PropertyDescription()
    {
      primitiveTypes = new HashSet<Type> {
        typeof (Int16), typeof (Int32), typeof (Int64), typeof (Byte), typeof (UInt16), typeof (UInt32),
        typeof (UInt64), typeof(Guid), typeof (Byte), typeof (Char), typeof (String), typeof (Decimal),
        typeof (Single), typeof (Double), typeof (DateTime), typeof (TimeSpan), typeof (DateTimeOffset)
      };
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.ObjectMapping.Model
{
  /// <summary>
  /// Description of a property of a mapped class.
  /// </summary>
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

    private static readonly ThreadSafeDictionary<Type, Pair<Type, MemberInfoCacheEntry>> collectionTypes =
      ThreadSafeDictionary<Type, Pair<Type, MemberInfoCacheEntry>>.Create(new object());
    private static readonly MemberInfoCacheEntry arrayCacheEntry =
      new MemberInfoCacheEntry(typeof (Array).GetProperty("Length"), null);

    private bool isCollection;

    /// <summary>
    /// Indicates whether this instance is primitive property.
    /// </summary>
    public readonly bool IsPrimitive;

    /// <summary>
    /// Gets the type that was used to obtain this description.
    /// </summary>
    public TypeDescription ReflectedType { get; private set; }

    /// <summary>
    /// Gets the underlying system property.
    /// </summary>
    public readonly PropertyInfo SystemProperty;

    /// <summary>
    /// Gets a value indicating whether this instance is collection property.
    /// </summary>
    public bool IsCollection {
      get { return isCollection; }
      internal set{
        this.EnsureNotLocked();
        isCollection = value;
      }
    }

    /// <summary>
    /// Gets the type of a collection's item.
    /// </summary>
    public Type ItemType { get; private set; }

    /// <summary>
    /// Gets the descriptor of the collection's "Count" property.
    /// </summary>
    public PropertyInfo CountProperty { get; private set; }

    /// <summary>
    /// Gets the descriptor of the collection's "Add" method.
    /// </summary>
    public MethodInfo AddMethod { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return SystemProperty.ToString();
    }

    #region Private / internal methods

    private void AssignCollectionRelatedProperties(PropertyInfo systemProperty)
    {
      Pair<Type, MemberInfoCacheEntry>? cacheItem = null;
      Pair<Type, MemberInfoCacheEntry> foundItem;
      if (collectionTypes.TryGetValue(systemProperty.PropertyType, out foundItem))
        cacheItem = foundItem;
      else if (systemProperty.PropertyType.IsArray)
        cacheItem = GenerateArrayCacheItem(systemProperty);
      else if (MappingHelper.IsCollectionCandidate(systemProperty.PropertyType))
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
      Type interfaceType;
      if (MappingHelper.TryGetCollectionInterface(systemProperty.PropertyType, out interfaceType))
        result = collectionTypes.GetValue(systemProperty.PropertyType, GenerateCollectionTypesCacheItem,
          interfaceType);
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

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemProperty">The system property.</param>
    /// <param name="reflectedType">The the type that was used to obtain this description.</param>
    protected PropertyDescription(PropertyInfo systemProperty, TypeDescription reflectedType)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemProperty, "systemProperty");
      ArgumentValidator.EnsureArgumentNotNull(reflectedType, "reflectedType");

      SystemProperty = systemProperty;
      ReflectedType = reflectedType;
      IsPrimitive = MappingHelper.IsTypePrimitive(systemProperty.PropertyType);
      AssignCollectionRelatedProperties(systemProperty);
    }
  }
}
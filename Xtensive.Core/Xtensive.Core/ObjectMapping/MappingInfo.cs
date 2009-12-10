// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  [Serializable]
  internal sealed class MappingInfo : LockableBase
  {
    #region Nested classes

    private class PropertyInfoContainer
    {
      public readonly ReadOnlyList<PropertyInfo> PrimitiveProperties;

      public readonly ReadOnlyList<PropertyInfo> ComplexProperties;


      // Constructors

      public PropertyInfoContainer(IList<PropertyInfo> primitiveProperties,
        IList<PropertyInfo> complexProperties)
      {
        PrimitiveProperties = new ReadOnlyList<PropertyInfo>(primitiveProperties, false);
        ComplexProperties = new ReadOnlyList<PropertyInfo>(complexProperties, false);
      }
    }

    #endregion

    private static readonly HashSet<Type> primitiveTypes;

    private readonly Dictionary<Type, PropertyInfoContainer> targetProperties =
      new Dictionary<Type, PropertyInfoContainer>();
    private readonly Dictionary<Type, Pair<Type, ReadOnlyDictionary<string, PropertyInfo>>> typeMap =
      new Dictionary<Type, Pair<Type, ReadOnlyDictionary<string, PropertyInfo>>>();
    private readonly Dictionary<PropertyInfo, Func<object, object>> propertyReversedMap =
      new Dictionary<PropertyInfo, Func<object, object>>();
    private readonly HashSet<Type> targetTypes = new HashSet<Type>();
    private readonly Dictionary<Type, Func<object, object>> keyExtractors =
      new Dictionary<Type, Func<object, object>>();

    public void Register(Type source, Func<object, object> sourceKeyExtractor, Type target,
      Func<object, object> targetKeyExtractor)
    {
      this.EnsureNotLocked();
      if (typeMap.ContainsKey(source))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));
      /*if (targetTypes.Contains(target))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));*/
      var properties = source.GetProperties();
      var propertyDictionary = new Dictionary<string, PropertyInfo>();
      for (int i = 0; i < properties.Length; i++) {
        var property = properties[i];
        propertyDictionary[property.Name] = property;
      }
      typeMap.Add(source, new Pair<Type, ReadOnlyDictionary<string, PropertyInfo>>(target,
        new ReadOnlyDictionary<string, PropertyInfo>(propertyDictionary)));
      targetTypes.Add(target);
      keyExtractors.Add(source, sourceKeyExtractor);
      keyExtractors.Add(target, targetKeyExtractor);
    }

    public void Register(Func<object, object> converter, PropertyInfo target)
    {
      this.EnsureNotLocked();
      if (propertyReversedMap.ContainsKey(target))
        throw new InvalidOperationException(
          String.Format(Strings.ExMappingForPropertyXHasAlreadyBeenRegistered, target));
      propertyReversedMap.Add(target, converter);
    }
    
    public void Build()
    {
      this.EnsureNotLocked();
      foreach (var mapPair in typeMap) {
        if (!targetProperties.ContainsKey(mapPair.Value.First)) {
          var primitiveProperties = new List<PropertyInfo>();
          var complexProperties = new List<PropertyInfo>();
          var properties = mapPair.Value.First.GetProperties();
          for (var i = 0; i < properties.Length; i++) {
            var property = properties[i];
            var type = property.PropertyType;
            if (IsPrimitive(type))
              primitiveProperties.Add(property);
            else {
              EnsureTypeIsRegisteredAsTarget(type);
              complexProperties.Add(property);
            }
          }
          targetProperties.Add(mapPair.Value.First,
            new PropertyInfoContainer(primitiveProperties, complexProperties));
        }
      }
      Lock(true);
    }

    public Pair<Type, ReadOnlyDictionary<string, PropertyInfo>> GetMap(Type sourceType)
    {
      return typeMap[sourceType];
    }

    public ReadOnlyList<PropertyInfo> GetTargetPrimitiveProperties(Type type)
    {
      return targetProperties[type].PrimitiveProperties;
    }

    public ReadOnlyList<PropertyInfo> GetTargetComplexProperties(Type type)
    {
      return targetProperties[type].ComplexProperties;
    }

    public object ExtractKey(object obj)
    {
      return keyExtractors[obj.GetType()].Invoke(obj);
    }

    public bool TryGetCustomConverter(PropertyInfo targetProperty, out Func<object, object> converter)
    {
      return propertyReversedMap.TryGetValue(targetProperty, out converter);
    }

    private static bool IsPrimitive(Type type)
    {
      return primitiveTypes.Contains(type);
    }

    private void EnsureTypeIsRegisteredAsSource(Type type)
    {
      if (!typeMap.ContainsKey(type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasNotBeenRegistered, type.FullName));
    }

    private void EnsureTypeIsRegisteredAsTarget(Type targetType)
    {
      if (!targetTypes.Contains(targetType))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasNotBeenRegistered,
          targetType.FullName));
    }

    /*private PropertyInfoContainer GetPropertyInfoContainer(Type targetType)
    {
      PropertyInfoContainer result;
      if (!targetProperties.TryGetValue(targetType, out result))
        throw new InvalidOperationException(
          String.Format("The type {0} hasn't been registered.", targetType.FullName));
      return result;
    }*/


    // Constructors

    static MappingInfo()
    {
      primitiveTypes = new HashSet<Type> {
        typeof (Int16), typeof (Int32), typeof (Int64), typeof (Byte), typeof (UInt16), typeof (UInt32),
        typeof (UInt64), typeof(Guid), typeof (Byte), typeof (Char), typeof (String), typeof (Decimal),
        typeof (Single), typeof (Double), typeof (DateTime), typeof (TimeSpan), typeof (DateTimeOffset)
      };
    }
  }
}
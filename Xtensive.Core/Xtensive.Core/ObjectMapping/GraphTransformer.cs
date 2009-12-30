// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class GraphTransformer
  {

    #region Nested classes

    private class ResultDescriptor
    {
      private bool isTransformationCompleted;

      public readonly object Result;

      public int Level { get; set; }

      public bool IsTransformationCompleted { get { return isTransformationCompleted; } }

      public void SetIsTransformationCompeleted()
      {
        isTransformationCompleted = true;
      }


      // Constructors

      public ResultDescriptor(object result)
      {
        Result = result;
      }
    }

    #endregion

    private static readonly ThreadSafeDictionary<Type, Func<object>> constructors =
      ThreadSafeDictionary<Type, Func<object>>.Create(new object());
    private readonly MappingDescription mappingDescription;
    private readonly Dictionary<object, ResultDescriptor> transformedObjects =
      new Dictionary<object, ResultDescriptor>();
    private readonly Queue<KeyValuePair<object, object>> sourceObjects =
      new Queue<KeyValuePair<object, object>>();
    private readonly List<object> rootObjectKeys = new List<object>();
    private bool isRootCollection;
    private readonly MapperSettings mapperSettings;
    private readonly Func<object, ResultDescriptor, TargetPropertyDescription, object>
      transformerForComplexPropertyHavingConverter;
    private readonly Func<object, ResultDescriptor, TargetPropertyDescription, object>
      transformerForCollection;
    private readonly Func<object, ResultDescriptor, TargetPropertyDescription, object>
      transformerForRegularComplexProperty;

    public object Transform(object source)
    {
      try {
        if (source==null)
          return null;
        InitializeTransformation(source);
        TransformObjects();
        return CreateResult();
      }
      finally {
        transformedObjects.Clear();
        sourceObjects.Clear();
        rootObjectKeys.Clear();
      }
    }

    #region Private \ internal methods

    private void InitializeTransformation(object source)
    {
      var type = source.GetType();
      Type interfaceType;
      if (MappingHelper.IsCollectionCandidate(type)
        && MappingHelper.TryGetCollectionInterface(type, out interfaceType)) {
        isRootCollection = true;
        foreach (var obj in (IEnumerable) source)
          RegisterRootObject(obj);
      }
      else {
        isRootCollection = false;
        RegisterRootObject(source);
      }
    }

    private void RegisterRootObject(object obj)
    {
      object key;
      TransformObject(obj, -1, out key);
      rootObjectKeys.Add(key);
    }

    private void TransformObjects()
    {
      while (sourceObjects.Count > 0) {
        var currentSource = sourceObjects.Dequeue();
        ResultDescriptor resultDescriptor;
        var targetTypeDesc = mappingDescription.GetMappedTargetType(currentSource.Value.GetType());
        if (transformedObjects.TryGetValue(currentSource.Key, out resultDescriptor)) {
          if (resultDescriptor.IsTransformationCompleted)
            continue;
        }
        TransformProperties(currentSource.Value, targetTypeDesc, resultDescriptor);
        resultDescriptor.SetIsTransformationCompeleted();
      }
    }

    private object CreateResult()
    {
      if (isRootCollection) {
        var result = new List<object>(rootObjectKeys.Count);
        result.AddRange(rootObjectKeys.Select(k => k!=null ? transformedObjects[k].Result : null));
        return result;
      }
      return transformedObjects[rootObjectKeys.Single()].Result;
    }

    private static object CreateTargetObject(Type type)
    {
      var constructor = constructors.GetValue(type, DelegateHelper.CreateConstructorDelegate<Func<object>>);
      return constructor.Invoke();
    }

    private void TransformProperties(object source, TargetTypeDescription targetDescription,
      ResultDescriptor target)
    {
      TransformPrimitiveProperties(source, targetDescription, target.Result);
      TransformComplexProperties(source, targetDescription, target);
    }

    private static void TransformPrimitiveProperties(object source, TargetTypeDescription targetDescription,
      object target)
    {
      foreach (var targetPropertyDesc in targetDescription.PrimitiveProperties.Values) {
        var propertyValue = targetPropertyDesc.Converter.Invoke(source, targetPropertyDesc.SourceProperty);
        targetPropertyDesc.SystemProperty.SetValue(target, propertyValue, null);
      }
    }

    private void TransformComplexProperties(object source, TargetTypeDescription targetDescription,
      ResultDescriptor target)
    {
      foreach (var targetProperty in targetDescription.ComplexProperties.Values) {
        if (targetProperty.Converter!=null)
          SetComplexTargetProperty(source, target, targetProperty,
            transformerForComplexPropertyHavingConverter);
        else if (targetProperty.IsCollection)
          SetComplexTargetProperty(source, target, targetProperty, transformerForCollection);
        else
          SetComplexTargetProperty(source, target, targetProperty, transformerForRegularComplexProperty);
      }
    }

    private void SetComplexTargetProperty(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty,
      Func<object, ResultDescriptor, TargetPropertyDescription, object> transformer)
    {
      var transformedValue = transformer.Invoke(source, target, targetProperty);
      if (transformedValue == null || !HandleExceedingOfGraphDepthLimit(target, targetProperty))
            targetProperty.SystemProperty.SetValue(target.Result, transformedValue, null);
    }

    private bool HandleExceedingOfGraphDepthLimit(ResultDescriptor owner,
      TargetPropertyDescription targetProperty)
    {
      if (owner.Level >= mapperSettings.GraphDepthLimit) {
        switch (mapperSettings.GraphTruncationType) {
        case GraphTruncationType.Throw:
          throw new InvalidOperationException(Strings.ExLimitOfGraphDepthIsExceeded);
        case GraphTruncationType.SetNull:
          targetProperty.SystemProperty.SetValue(owner.Result, null, null);
          return true;
        default:
          throw new ArgumentOutOfRangeException("mapperSettings.GraphTruncationType");
        }
      }
      return false;
    }

    private static object TransformComplexPropertyHavingConverter(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty)
    {
      return targetProperty.Converter.Invoke(source, targetProperty.SourceProperty);
    }

    private object TransformRegularComplexProperty(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty)
    {
      var sourceValue = targetProperty.SourceProperty.SystemProperty.GetValue(source, null);
      return TransformObject(sourceValue, target.Level);
    }

    private object TransformCollection(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty)
    {
      var sourceProperty = targetProperty.SourceProperty;
      var sourceValue = sourceProperty.SystemProperty.GetValue(source, null);
      if (sourceValue == null)
        return null;
      var itemCount = (int) sourceProperty.CountProperty.GetValue(sourceValue, null);
      object targetValue;
      var isItemTypePrimitive = MappingHelper.IsTypePrimitive(targetProperty.ItemType);
      if (targetProperty.SystemProperty.PropertyType.IsArray) {
        var array = Array.CreateInstance(targetProperty.ItemType, itemCount);
        var index = 0;
        foreach (var obj in (IEnumerable) sourceValue) {
          var value = isItemTypePrimitive ? obj : TransformObject(obj, target.Level);
          array.SetValue(value, index++);
        }
        targetValue = array;
      }
      else {
        var collection = Activator.CreateInstance(targetProperty.SystemProperty.PropertyType);
        var addMethod = targetProperty.AddMethod;
        foreach (var obj in (IEnumerable) sourceValue) {
          var value = isItemTypePrimitive ? obj : TransformObject(obj, target.Level);
          addMethod.Invoke(collection, new[] {value});
        }
        targetValue = collection;
      }
      return targetValue;
    }

    private object TransformObject(object source, int levelOfOwner)
    {
      object key;
      return TransformObject(source, levelOfOwner, out key);
    }

    private object TransformObject(object source, int levelOfOwner, out object key)
    {
      key = null;
      if (source==null)
        return null;
      var targetType = mappingDescription.GetMappedTargetType(source.GetType());
      key = mappingDescription.ExtractSourceKey(source);
      ResultDescriptor resultDescriptor;
      if (!transformedObjects.TryGetValue(key, out resultDescriptor)) {
        var targetValue = CreateTargetObject(targetType.SystemType);
        resultDescriptor = new ResultDescriptor(targetValue);
        transformedObjects.Add(key, resultDescriptor);
        sourceObjects.Enqueue(new KeyValuePair<object, object>(key, source));
      }
      resultDescriptor.Level = levelOfOwner + 1;
      return resultDescriptor.Result;
    }

    #endregion


    // Constructors

    public GraphTransformer(MappingDescription mappingDescription,
      MapperSettings mapperSettings)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      ArgumentValidator.EnsureArgumentNotNull(mapperSettings, "mapperSettings");

      this.mappingDescription = mappingDescription;
      this.mapperSettings = mapperSettings;
      transformerForComplexPropertyHavingConverter = TransformComplexPropertyHavingConverter;
      transformerForCollection = TransformCollection;
      transformerForRegularComplexProperty = TransformRegularComplexProperty;
    }
  }
}
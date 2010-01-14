// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Object graph transformer.
  /// </summary>
  internal class GraphTransformer
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
    private readonly Func<object, int, TargetPropertyDescription, object>
      transformerForComplexPropertyHavingConverter;
    private readonly Func<object, int, TargetPropertyDescription, object>
      transformerForCollection;
    private readonly Func<object, int, TargetPropertyDescription, object>
      transformerForRegularComplexProperty;

    /// <summary>
    /// Transforms an object graph.
    /// </summary>
    /// <param name="source">The source graph.</param>
    /// <returns>The transformed graph.</returns>
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
      if (!type.IsValueType) {
        var constructor = constructors.GetValue(type, DelegateHelper.CreateConstructorDelegate<Func<object>>);
        return constructor.Invoke();
      }
      return Activator.CreateInstance(type, true);
    }

    private void TransformProperties(object source, TargetTypeDescription targetDescription,
      ResultDescriptor target)
    {
      TransformPrimitiveProperties(source, targetDescription, target.Result);
      TransformComplexProperties(source, targetDescription, target);
    }

    private static void TransformPrimitiveProperties(object source, TargetTypeDescription targetType,
      object target)
    {
      foreach (var targetProperty in targetType.PrimitiveProperties.Values) {
        var propertyValue = targetProperty.Converter.Invoke(source, targetProperty.SourceProperty);
        targetProperty.SystemProperty.SetValue(target, propertyValue, null);
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
        else if (targetProperty.IsUserStructure)
          TransformUserStructure(source, target, targetProperty);
        else
          SetComplexTargetProperty(source, target, targetProperty, transformerForRegularComplexProperty);
      }
    }

    private void SetComplexTargetProperty(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty,
      Func<object, int, TargetPropertyDescription, object> transformer)
    {
      var transformedValue = transformer.Invoke(source, target.Level, targetProperty);
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
        case GraphTruncationType.SetDefaultValue:
          targetProperty.SystemProperty.SetValue(owner.Result, null, null);
          return true;
        default:
          throw new ArgumentOutOfRangeException("mapperSettings.GraphTruncationType");
        }
      }
      return false;
    }

    private static object TransformComplexPropertyHavingConverter(object source, int targetLevel,
      TargetPropertyDescription targetProperty)
    {
      return targetProperty.Converter.Invoke(source, targetProperty.SourceProperty);
    }

    private object TransformRegularComplexProperty(object source, int targetLevel,
      TargetPropertyDescription targetProperty)
    {
      var sourceValue = targetProperty.SourceProperty.SystemProperty.GetValue(source, null);
      return TransformObject(sourceValue, targetLevel);
    }

    private object TransformCollection(object source, int targetLevel,
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
          var value = isItemTypePrimitive ? obj : TransformObject(obj, targetLevel);
          array.SetValue(value, index++);
        }
        targetValue = array;
      }
      else {
        var collection = Activator.CreateInstance(targetProperty.SystemProperty.PropertyType);
        var addMethod = targetProperty.AddMethod;
        foreach (var obj in (IEnumerable) sourceValue) {
          var value = isItemTypePrimitive ? obj : TransformObject(obj, targetLevel);
          addMethod.Invoke(collection, new[] {value});
        }
        targetValue = collection;
      }
      return targetValue;
    }

    private void TransformUserStructure(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty)
    {
      var targetValueType = mappingDescription.TargetTypes[targetProperty.SystemProperty.PropertyType];
      object targetValue;
      if (targetProperty.Converter != null) {
        targetValue = targetProperty.Converter.Invoke(source, targetProperty.SourceProperty);
        if (targetValue==null || !HandleExceedingOfGraphDepthLimit(target, targetProperty))
          targetProperty.SystemProperty.SetValue(target.Result, targetValue, null);
      }
      else {
        var sourceValue = targetProperty.SourceProperty.SystemProperty.GetValue(source, null);
        if (sourceValue == null)
          targetProperty.SystemProperty.SetValue(target.Result, null, null);
        else if (!HandleExceedingOfGraphDepthLimit(target, targetProperty)) {
          targetValue = CreateTargetObject(targetValueType.SystemType);
          TransformProperties(sourceValue, targetValueType,
            new ResultDescriptor(targetValue) {Level = target.Level + 1}); 
          targetProperty.SystemProperty.SetValue(target.Result, targetValue, null);
        }
      }
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    /// <param name="mapperSettings">The mapper settings.</param>
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
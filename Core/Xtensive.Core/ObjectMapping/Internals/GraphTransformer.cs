// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping.Model;
using Xtensive.Reflection;
using Xtensive.Resources;


namespace Xtensive.ObjectMapping
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

    private struct RootObjectDescriptor
    {
      public readonly object Object;

      public readonly ObjectKind Kind;


      // Constructors

      public RootObjectDescriptor(object obj, ObjectKind kind)
      {
        Object = obj;
        Kind = kind;
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
    private readonly List<RootObjectDescriptor> rootObjectKeys = new List<RootObjectDescriptor>();
    private readonly MapperSettings mapperSettings;
    private readonly Func<object, int, TargetPropertyDescription, object>
      transformerForComplexPropertyHavingConverter;
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
      var rootSystemType = source.GetType();
      var rootModelType = GetSourceType(rootSystemType, false);
      if (rootModelType == null)
        RegisterEnumerable(source, rootSystemType);
      else
        RegisterRootObject(source, rootModelType);
    }

    private Type GetTypeSafely(object obj)
    {
      return obj!=null ? obj.GetType() : null;
    }

    private SourceTypeDescription GetSourceType(Type systemType, bool throwIfNotFound)
    {
      if (systemType == null)
        return null;
      if (throwIfNotFound)
        return mappingDescription.GetSourceType(systemType);
      else {
        SourceTypeDescription result;
        return mappingDescription.TryGetSourceType(systemType, out result) ? result : null;
      }
    }

    private void RegisterEnumerable(object source, Type sourceSystemType)
    {
      if (MappingHelper.IsEnumerable(sourceSystemType))
        foreach (var item in (IEnumerable) source) {
          var itemSystemType = GetTypeSafely(item);
          if (itemSystemType!=null && MappingHelper.IsCollection(itemSystemType))
            throw new ArgumentException(Strings.ExNestedCollectionIsNotSupported, "item");
          var itemModelType = GetSourceType(itemSystemType, true);
          RegisterRootObject(item, itemModelType);
        }
      else
        MappingDescription.ThrowTypeHasNotBeenRegistered(sourceSystemType);
    }

    private void RegisterRootObject(object obj, SourceTypeDescription modelType)
    {
      if (obj==null || modelType.ObjectKind==ObjectKind.Primitive) {
        rootObjectKeys.Add(new RootObjectDescriptor(obj, ObjectKind.Primitive));
        return;
      }
      if (modelType.TargetType.ObjectKind==ObjectKind.UserStructure) {
        rootObjectKeys.Add(new RootObjectDescriptor(obj, ObjectKind.UserStructure));
        return;
      }
      object key;
      TransformComplexObject(obj, -1, out key);
      rootObjectKeys.Add(new RootObjectDescriptor(key, ObjectKind.Entity));
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
      if (rootObjectKeys.Count!=1) {
        var result = new List<object>(rootObjectKeys.Count);
        foreach (var rootDescriptor in rootObjectKeys)
          result.Add(CreateResultItem(rootDescriptor));
        return result;
      }
      return CreateResultItem(rootObjectKeys[0]);
    }

    private object CreateResultItem(RootObjectDescriptor rootDescriptor)
    {
      switch (rootDescriptor.Kind) {
      case ObjectKind.Entity:
        return transformedObjects[rootDescriptor.Object].Result;
      case ObjectKind.Primitive:
        return rootDescriptor.Object;
      case ObjectKind.UserStructure:
        var targetType = mappingDescription.GetSourceType(rootDescriptor.Object.GetType()).TargetType;
        var transformedStructure = CreateTargetObject(targetType.SystemType);
        TransformProperties(rootDescriptor.Object, targetType, new ResultDescriptor(transformedStructure));
        return transformedStructure;
      default:
        throw new ArgumentOutOfRangeException("rootDescriptor.Type");
      }
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
        else if (targetProperty.IsCollection) {
          var value = TransformCollection(source, target.Level, targetProperty);
          targetProperty.SystemProperty.SetValue(target.Result, value, null);
        }
        else if (targetProperty.ValueType.ObjectKind==ObjectKind.UserStructure) {
          var value = TransformUserStructure(source, target, targetProperty);
          targetProperty.SystemProperty.SetValue(target.Result, value, null);
        }
        else
          SetComplexTargetProperty(source, target, targetProperty, transformerForRegularComplexProperty);
      }
    }

    private void SetComplexTargetProperty(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty,
      Func<object, int, TargetPropertyDescription, object> transformer)
    {
      var transformedValue = transformer.Invoke(source, target.Level, targetProperty);
      HandleExceedingOfGraphDepthLimit(target.Level, ref transformedValue);
      targetProperty.SystemProperty.SetValue(target.Result, transformedValue, null);
    }

    private bool HandleExceedingOfGraphDepthLimit(int ownerLevel, ref object value)
    {
      if (value==null)
        return true;
      if (ownerLevel >= mapperSettings.GraphDepthLimit) {
        switch (mapperSettings.GraphTruncationType) {
        case GraphTruncationType.Throw:
          throw new InvalidOperationException(Strings.ExLimitOfGraphDepthIsExceeded);
        case GraphTruncationType.SetDefaultValue:
          value = null;
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
      object key;
      return TransformComplexObject(sourceValue, targetLevel, out key);
    }

    private object TransformCollection(object source, int ownerLevel,
      TargetPropertyDescription targetProperty)
    {
      var sourceProperty = targetProperty.SourceProperty;
      var sourceValue = sourceProperty.SystemProperty.GetValue(source, null);
      if (sourceValue == null)
        return null;
      if (HandleExceedingOfGraphDepthLimit(ownerLevel, ref sourceValue))
        return sourceValue;
      var itemCount = (int) sourceProperty.CountProperty.GetValue(sourceValue, null);
      object targetValue;
      object key;
      if (targetProperty.SystemProperty.PropertyType.IsArray) {
        var array = Array.CreateInstance(targetProperty.ValueType.SystemType, itemCount);
        var index = 0;
        foreach (var obj in (IEnumerable) sourceValue) {
          var value = TransformCollectionItem(obj, ownerLevel);
          array.SetValue(value, index++);
        }
        targetValue = array;
      }
      else {
        var collection = CreateTargetObject(targetProperty.SystemProperty.PropertyType);
        var addMethod = targetProperty.AddMethod;
        foreach (var obj in (IEnumerable) sourceValue) {
          var value = TransformCollectionItem(obj, ownerLevel);
          addMethod.Invoke(collection, new[] {value});
        }
        targetValue = collection;
      }
      return targetValue;
    }

    private object TransformUserStructure(object source, ResultDescriptor target,
      TargetPropertyDescription targetProperty)
    {
      var targetValueType = mappingDescription.GetTargetType(targetProperty.SystemProperty.PropertyType);
      object targetValue;
      object sourceValue = null;
      if (targetProperty.Converter!=null)
        targetValue = targetProperty.Converter.Invoke(source, targetProperty.SourceProperty);
      else {
        sourceValue = targetProperty.SourceProperty.SystemProperty.GetValue(source, null);
        targetValue = sourceValue==null ? null : CreateTargetObject(targetValueType.SystemType);
      }
      if (!HandleExceedingOfGraphDepthLimit(target.Level, ref targetValue))
        TransformProperties(sourceValue, targetValueType,
          new ResultDescriptor(targetValue) {Level = target.Level + 1});
      return targetValue;
    }

    private object TransformCollectionItem(object source, int ownerLevel)
    {
      if (source==null)
        return null;
      var sourceType = mappingDescription.GetSourceType(source.GetType());
      if (sourceType.ObjectKind==ObjectKind.Primitive)
        return source;
      object target;
      if (sourceType.TargetType.ObjectKind==ObjectKind.UserStructure) {
        target = CreateTargetObject(sourceType.TargetType.SystemType);
        TransformProperties(source, sourceType.TargetType,
          new ResultDescriptor(target) {Level = ownerLevel + 1});
        return target;
      }
      object key;
      target = TransformComplexObject(source, sourceType.TargetType.SystemType, ownerLevel, out key);
      return target;
    }

    private object TransformComplexObject(object source, int levelOfOwner, out object key)
    {
      key = null;
      if (source==null)
        return null;
      var targetType = mappingDescription.GetMappedTargetType(source.GetType());
      return TransformComplexObject(source, targetType.SystemType, levelOfOwner, out key);
    }

    private object TransformComplexObject(object source, Type targetType, int levelOfOwner, out object key)
    {
      key = mappingDescription.ExtractSourceKey(source);
      ResultDescriptor resultDescriptor;
      if (!transformedObjects.TryGetValue(key, out resultDescriptor)) {
        var targetValue = CreateTargetObject(targetType);
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
      transformerForRegularComplexProperty = TransformRegularComplexProperty;
    }
  }
}
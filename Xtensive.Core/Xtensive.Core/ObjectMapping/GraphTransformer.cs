// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class GraphTransformer
  {

    #region Nested classes

    private class ResultDescriptor
    {
      private bool isTransformationCompleted;

      public readonly object Result;

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
      var key = mappingDescription.ExtractSourceKey(obj);
      sourceObjects.Enqueue(new KeyValuePair<object, object>(key, obj));
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
        else {
          var target = CreateTargetObject(targetTypeDesc.SystemType);
          resultDescriptor = new ResultDescriptor(target);
          transformedObjects.Add(currentSource.Key, resultDescriptor);
        }
        ConvertProperties(currentSource.Value, targetTypeDesc, resultDescriptor.Result);
        resultDescriptor.SetIsTransformationCompeleted();
      }
    }

    private object CreateResult()
    {
      if (isRootCollection) {
        var result = new List<object>(rootObjectKeys.Count);
        result.AddRange(rootObjectKeys.Select(k => transformedObjects[k].Result));
        return result;
      }
      return transformedObjects[rootObjectKeys.Single()].Result;
    }

    private static object CreateTargetObject(Type type)
    {
      var constructor = constructors.GetValue(type, DelegateHelper.CreateConstructorDelegate<Func<object>>);
      return constructor.Invoke();
    }

    private void ConvertProperties(object source, TargetTypeDescription targetDescription,
      object target)
    {
      ConvertPrimitiveProperties(source, targetDescription, target);
      ConvertComplexProperties(source, targetDescription, target);
    }

    private static void ConvertPrimitiveProperties(object source, TargetTypeDescription targetDescription,
      object target)
    {
      foreach (var targetPropertyDesc in targetDescription.PrimitiveProperties.Values)
        targetPropertyDesc.Converter.Invoke(source, targetPropertyDesc.SourceProperty,
          target, targetPropertyDesc);
    }

    private void ConvertComplexProperties(object source, TargetTypeDescription targetDescription, object target)
    {
      foreach (var targetProperty in targetDescription.ComplexProperties.Values) {
        if (targetProperty.Converter!=null)
          targetProperty.Converter.Invoke(source, targetProperty.SourceProperty,
            target, targetProperty);
        else if (targetProperty.IsCollection)
          ConvertCollection(source, targetProperty.SourceProperty, target, targetProperty);
        else
          ConvertRegularComplexProperty(source, targetProperty.SourceProperty.SystemProperty,
            target, targetProperty.SystemProperty);
      }
    }

    private void ConvertRegularComplexProperty(object source, PropertyInfo sourceProperty, object target,
      PropertyInfo targetProperty)
    {
      var sourceValue = sourceProperty.GetValue(source, null);
      targetProperty.SetValue(target, TransformObject(sourceValue), null);
    }

    private void ConvertCollection(object source, SourcePropertyDescription sourceProperty,
      object target, TargetPropertyDescription targetProperty)
    {
      var sourceValue = sourceProperty.SystemProperty.GetValue(source, null);
      if (sourceValue == null) {
        targetProperty.SystemProperty.SetValue(target, null, null);
        return;
      }
      var itemCount = (int) sourceProperty.CountProperty.GetValue(sourceValue, null);
      object targetValue;
      if (targetProperty.SystemProperty.PropertyType.IsArray) {
        var array = Array.CreateInstance(targetProperty.ItemType, itemCount);
        var index = 0;
        foreach (var obj in (IEnumerable) sourceValue)
          array.SetValue(TransformObject(obj), index++);
        targetValue = array;
      }
      else {
        var collection = Activator.CreateInstance(targetProperty.SystemProperty.PropertyType);
        var addMethod = targetProperty.AddMethod;
        foreach (var obj in (IEnumerable) sourceValue)
          addMethod.Invoke(collection, new[] {TransformObject(obj)});
        targetValue = collection;
      }
      targetProperty.SystemProperty.SetValue(target, targetValue, null);
    }

    private object TransformObject(object source)
    {
      if (source==null)
        return null;
      var targetType = mappingDescription.GetMappedTargetType(source.GetType());
      var key = mappingDescription.ExtractSourceKey(source);
      ResultDescriptor resultDescriptor;
      if (!transformedObjects.TryGetValue(key, out resultDescriptor)) {
        var targetValue = CreateTargetObject(targetType.SystemType);
        resultDescriptor = new ResultDescriptor(targetValue);
        transformedObjects.Add(key, resultDescriptor);
        sourceObjects.Enqueue(new KeyValuePair<object, object>(key, source));
      }
      return resultDescriptor.Result;
    }

    #endregion


    // Constructors

    public GraphTransformer(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}
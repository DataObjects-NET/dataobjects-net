// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
using System.Reflection;
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
    private readonly MappingInfo mappingInfo;
    private readonly Dictionary<object, ResultDescriptor> transformedObjects =
      new Dictionary<object, ResultDescriptor>();
    private readonly Dictionary<object, Dictionary<PropertyInfo, object>> delayedAssignments =
      new Dictionary<object, Dictionary<PropertyInfo, object>>();
    private readonly Queue<KeyValuePair<object, object>> sourceObjects =
      new Queue<KeyValuePair<object, object>>();

    public object Transform(object source)
    {
      try {
        if (source==null)
          return null;
        var rootKey = mappingInfo.ExtractKey(source);
        sourceObjects.Enqueue(new KeyValuePair<object, object>(rootKey, source));
        while (sourceObjects.Count > 0) {
          var currentSource = sourceObjects.Dequeue();
          ResultDescriptor resultDescriptor;
          var map = mappingInfo.GetMap(currentSource.Value.GetType());
          if (transformedObjects.TryGetValue(currentSource.Key, out resultDescriptor)) {
            if (resultDescriptor.IsTransformationCompleted)
              continue;
          }
          else {
            var target = CreateTargetObject(map.First);
            resultDescriptor = new ResultDescriptor(target);
            transformedObjects.Add(currentSource.Key, resultDescriptor);
          }
          ConvertProperties(currentSource.Value, map.Second, resultDescriptor.Result);
          resultDescriptor.SetIsTransformationCompeleted();
        }
        return transformedObjects[rootKey].Result;
      }
      finally {
        transformedObjects.Clear();
        delayedAssignments.Clear();
        sourceObjects.Clear();
      }
    }

    private static object CreateTargetObject(Type type)
    {
      var constructor = constructors.GetValue(type, DelegateHelper.CreateConstructorDelegate<Func<object>>);
      return constructor.Invoke();
    }

    private void ConvertProperties(object source, IDictionary<string, PropertyInfo> sourceProperties,
      object target)
    {
      ConvertPrimitiveProperties(source, sourceProperties, target);
      ConvertComplexProperties(source, sourceProperties, target);
    }

    private void ConvertPrimitiveProperties(object source,
      IDictionary<string, PropertyInfo> sourceProperties, object target)
    {
      foreach (var targetProperty in mappingInfo.GetTargetPrimitiveProperties(target.GetType())) {
        Func<object, object> converter;
        object convertedValue;
        if (mappingInfo.TryGetCustomConverter(targetProperty, out converter))
          convertedValue = converter.Invoke(source);
        else {
          var property = sourceProperties[targetProperty.Name];
          convertedValue = property.GetValue(source, null);
        }
        targetProperty.SetValue(target, convertedValue, null);
      }
    }

    private void ConvertComplexProperties(object source,
      IDictionary<string, PropertyInfo> sourceProperties, object target)
    {
      foreach (var targetProperty in mappingInfo.GetTargetComplexProperties(target.GetType())) {
        Func<object, object> converter;
        if (mappingInfo.TryGetCustomConverter(targetProperty, out converter))
          targetProperty.SetValue(target, converter.Invoke(source), null);
        else
          ExecuteDefaultConversionOfComplexProperty(source, sourceProperties[targetProperty.Name],
            target, targetProperty);
      }
    }

    private void ExecuteDefaultConversionOfComplexProperty(object source,
      PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
    {
      var sourceValue = sourceProperty.GetValue(source, null);
      if (sourceValue == null)
        targetProperty.SetValue(target, null, null);
      else {
        var key = mappingInfo.ExtractKey(sourceValue);
        ResultDescriptor resultDescriptor;
        if (!transformedObjects.TryGetValue(key, out resultDescriptor)) {
          var targetValue = CreateTargetObject(targetProperty.PropertyType);
          resultDescriptor = new ResultDescriptor(targetValue);
          transformedObjects.Add(key, resultDescriptor);
          sourceObjects.Enqueue(new KeyValuePair<object, object>(key, sourceValue));
        }
        targetProperty.SetValue(target, resultDescriptor.Result, null);
      }
    }


    // Constructors

    public GraphTransformer(MappingInfo mappingInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingInfo, "mappingInfo");

      this.mappingInfo = mappingInfo;
    }
  }
}
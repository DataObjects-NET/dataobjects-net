// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
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

    public object Transform(object source)
    {
      try {
        if (source==null)
          return null;
        var rootKey = mappingDescription.ExtractSourceKey(source);
        sourceObjects.Enqueue(new KeyValuePair<object, object>(rootKey, source));
        while (sourceObjects.Count > 0) {
          var currentSource = sourceObjects.Dequeue();
          ResultDescriptor resultDescriptor;
          var targetTypeDesc = mappingDescription.GetMappedType(currentSource.Value.GetType());
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
        return transformedObjects[rootKey].Result;
      }
      finally {
        transformedObjects.Clear();
        sourceObjects.Clear();
      }
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
      foreach (var targetPropertyDesc in targetDescription.ComplexProperties.Values) {
        if (targetPropertyDesc.Converter != null)
          targetPropertyDesc.Converter.Invoke(source, targetPropertyDesc.SourceProperty,
            target, targetPropertyDesc);
        else
          ExecuteDefaultConversionOfComplexProperty(source, targetPropertyDesc.SourceProperty.SystemProperty,
            target, targetPropertyDesc.SystemProperty);
      }
    }

    private void ExecuteDefaultConversionOfComplexProperty(object source,
      PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
    {
      var sourceValue = sourceProperty.GetValue(source, null);
      if (sourceValue == null)
        targetProperty.SetValue(target, null, null);
      else {
        var key = mappingDescription.ExtractSourceKey(sourceValue);
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

    public GraphTransformer(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}
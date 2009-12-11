// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class MappingBuilder
  {
    private static readonly Action<object, SourcePropertyDescription, object, TargetPropertyDescription>
      defaultPrimitiveConverter = (source, sourceProperty, target, targetProperty) =>
        targetProperty.SystemProperty
          .SetValue(target, sourceProperty.SystemProperty.GetValue(source, null), null);
    private readonly Dictionary<PropertyInfo, PropertyInfo> propertyBindings =
      new Dictionary<PropertyInfo, PropertyInfo>();

    private readonly MappingDescription mappingDescription = new MappingDescription();

    public void Register(Type source, Func<object, object> sourceKeyExtractor, Type target,
      Func<object, object> targetKeyExtractor)
    {
      mappingDescription.Register(source, sourceKeyExtractor, target, targetKeyExtractor);
    }

    public void Register(PropertyInfo source, Func<object, object> converter, PropertyInfo target)
    {
      mappingDescription.Register(source, converter, target);
      if (source != null)
        propertyBindings.Add(target, source);
    }

    public MappingDescription Build()
    {
      mappingDescription.EnsureNotLocked();
      BuildSourceDescriptions();
      BuildTargetDescriptions();
      mappingDescription.Lock(true);
      return mappingDescription;
    }

    private void BuildTargetDescriptions()
    {
      foreach (var targetType in mappingDescription.TargetTypes.Values) {
        ApplyCustomBindings(targetType);
        BuildTargetPropertyDescriptions(targetType);
      }
    }

    private void BuildTargetPropertyDescriptions(TargetTypeDescription targetType)
    {
      var properties = targetType.SystemType.GetProperties();
      for (var i = 0; i < properties.Length; i++) {
        var property = properties[i];
        if (!targetType.Properties.ContainsKey(property)) {
          var propertyDesc = new TargetPropertyDescription(property, targetType) {IsCollection = false};
          var sourceProperty = targetType.SourceType.Properties
            .Where(p => p.Key.Name==property.Name).Select(p => p.Value).Single();
          propertyDesc.SourceProperty = (SourcePropertyDescription) sourceProperty;
          if (!propertyDesc.IsPrimitive) {
            if (!mappingDescription.TargetTypes.ContainsKey(propertyDesc.SystemProperty.PropertyType))
              ThrowTypeHasNotBeenRegistered(propertyDesc);
          }
          else
            propertyDesc.Converter = defaultPrimitiveConverter;
          targetType.AddProperty(propertyDesc);
        }
      }
    }

    private void ApplyCustomBindings(TargetTypeDescription targetType)
    {
      foreach (var pair in targetType.Properties) {
        PropertyInfo sourceProperty;
        if (propertyBindings.TryGetValue(pair.Value.SystemProperty, out sourceProperty)) {
          var sourceTypeDesc = mappingDescription.SourceTypes[sourceProperty.DeclaringType];
          ((TargetPropertyDescription) pair.Value).SourceProperty =
            (SourcePropertyDescription) sourceTypeDesc.Properties[sourceProperty];
        }
      }
    }

    private void BuildSourceDescriptions()
    {
      foreach (var sourceType in mappingDescription.SourceTypes) {
        var properties = sourceType.Key.GetProperties();
        for (var i = 0; i < properties.Length; i++) {
          var property = properties[i];
          if (!sourceType.Value.Properties.ContainsKey(property)) {
            var propertyDesc = new SourcePropertyDescription(property, sourceType.Value) {IsCollection = false};
            if (!propertyDesc.IsPrimitive
              && !mappingDescription.SourceTypes.ContainsKey(propertyDesc.SystemProperty.PropertyType))
              ThrowTypeHasNotBeenRegistered(propertyDesc);
            sourceType.Value.AddProperty(propertyDesc);
          }
        }
      }
    }

    private static void ThrowTypeHasNotBeenRegistered(PropertyDescription propertyDesc)
    {
      throw new InvalidOperationException(String.Format(Strings.ExTypeXHasNotBeenRegistered,
        propertyDesc.SystemProperty.PropertyType.FullName));
    }
  }
}
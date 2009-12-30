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
  internal sealed class ModelBuilder
  {
    public static readonly Func<object, SourcePropertyDescription, object>
      DefaultPrimitiveConverter = (source, sourceProperty) =>
        sourceProperty.SystemProperty.GetValue(source, null);
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
      else
        MarkPropertyAsImmutable(target);
    }

    public void RegisterHeir(Type targetBase, Type source, Type target)
    {
      mappingDescription.EnsureTargetTypeIsRegistered(targetBase);
      var targetBaseDescription = mappingDescription.TargetTypes[targetBase];
      if (!targetBaseDescription.SourceType.SystemType.IsAssignableFrom(source))
        throw new ArgumentException(String.Format(Strings.ExTypeXIsNotSubclassOfTypeY, source,
          targetBaseDescription.SourceType.SystemType));
      mappingDescription.Register(source, targetBaseDescription.SourceType.KeyExtractor, target,
        targetBaseDescription.KeyExtractor);
      var targetDescription = mappingDescription.TargetTypes[target];
    }

    public void MarkPropertyAsIgnored(PropertyInfo propertyInfo)
    {
      mappingDescription.MarkPropertyAsIgnored(propertyInfo);
    }

    public void MarkPropertyAsImmutable(PropertyInfo propertyInfo)
    {
      mappingDescription.MarkPropertyAsImmutable(propertyInfo);
    }

    public MappingDescription Build()
    {
      mappingDescription.EnsureNotLocked();
      BindHierarchies();
      BuildSourceDescriptions();
      BuildTargetDescriptions();
      mappingDescription.Lock(true);
      var modelValidator = new ModelValidator();
      modelValidator.Validate(mappingDescription);
      return mappingDescription;
    }

    #region Private / internal methods

    private void BindHierarchies()
    {
      var roots = GetHierarchies();
      BindHierarchy(roots);
    }

    private List<TargetTypeDescription> GetHierarchies()
    {
      var result = new List<TargetTypeDescription>();
      foreach (var type in mappingDescription.TargetTypes.Values) {
        var ancestor = type.SystemType.BaseType;
        while (true) {
          if (ancestor==typeof (object)) {
            result.Add(type);
            break;
          }
          TargetTypeDescription ancestorDescription;
          if (mappingDescription.TargetTypes.TryGetValue(ancestor, out ancestorDescription)) {
            ancestorDescription.AddDirectDescendant(type);
            break;
          }
          ancestor = ancestor.BaseType;
        }
      }
      return result;
    }

    private void BindHierarchy(IEnumerable<TargetTypeDescription> ancestors)
    {
      foreach (var ancestor in ancestors) {
        var directDescendants = ancestor.DirectDescendants;
        if (directDescendants == null)
          continue;
        foreach (var descendant in directDescendants)
          InheritProperties(ancestor, descendant);
        BindHierarchy(directDescendants);
      }
    }

    private void InheritProperties(TargetTypeDescription ancestor, TargetTypeDescription descendant)
    {
      foreach (var property in ancestor.Properties.Values.Cast<TargetPropertyDescription>()) {
        var descendantSystemProperty = descendant.SystemType.GetProperty(property.SystemProperty.Name);
        if (descendant.Properties.ContainsKey(descendantSystemProperty))
          continue;
        if (property.Converter!=null) {
          PropertyInfo sourceSystemProperty = null;
          if (property.SourceProperty!=null)
            sourceSystemProperty = descendant.SourceType.SystemType
              .GetProperty(property.SourceProperty.SystemProperty.Name);
          mappingDescription.Register(sourceSystemProperty, property.Converter, descendantSystemProperty);
        }
        if (property.IsIgnored)
          mappingDescription.MarkPropertyAsIgnored(descendantSystemProperty);
        if (property.IsImmutable)
          mappingDescription.MarkPropertyAsImmutable(descendantSystemProperty);
      }
    }

    private void BuildTargetDescriptions()
    {
      foreach (var targetType in mappingDescription.TargetTypes.Values) {
        ApplyCustomBindings(targetType);
        BuildTargetPropertyDescriptions(targetType);
      }
    }

    private static void BuildTargetPropertyDescriptions(TargetTypeDescription targetType)
    {
      var properties = targetType.SystemType.GetProperties();
      for (var i = 0; i < properties.Length; i++) {
        var property = properties[i];
        PropertyDescription existingProperty;
        if (targetType.Properties.TryGetValue(property, out existingProperty))
          SetPropertyConverter(targetType, (TargetPropertyDescription) existingProperty);
        else {
          var newProperty = new TargetPropertyDescription(property, targetType);
          SetPropertyConverter(targetType, newProperty);
          targetType.AddProperty(newProperty);
        }
      }
    }

    private static void SetPropertyConverter(TargetTypeDescription targetType,
      TargetPropertyDescription propertyDescription)
    {
      if (propertyDescription.Converter == null && !propertyDescription.IsIgnored) {
        var sourceProperty = targetType.SourceType.Properties
          .Where(p => p.Key.Name==propertyDescription.SystemProperty.Name).Select(p => p.Value).Single();
        propertyDescription.SourceProperty = (SourcePropertyDescription) sourceProperty;
        if (propertyDescription.IsPrimitive)
          propertyDescription.Converter = DefaultPrimitiveConverter;
      }
    }

    private void ApplyCustomBindings(TargetTypeDescription targetType)
    {
      foreach (var pair in targetType.Properties) {
        PropertyInfo sourceProperty;
        if (propertyBindings.TryGetValue(pair.Value.SystemProperty, out sourceProperty)) {
          var sourceTypeDesc = mappingDescription.SourceTypes[sourceProperty.ReflectedType];
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
          if (sourceType.Value.Properties.ContainsKey(property))
            continue;
          var propertyDesc = new SourcePropertyDescription(property, sourceType.Value);
          sourceType.Value.AddProperty(propertyDesc);
        }
      }
    }

    #endregion
  }
}
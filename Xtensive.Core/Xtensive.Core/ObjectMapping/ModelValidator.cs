// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.21

using System;
using System.Linq;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class ModelValidator
  {
    private MappingDescription description;

    public void Validate(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      description = mappingDescription;
      foreach (var typePair in description.TargetTypes) {
        var type = typePair.Value;
        var properties = type.Properties.Select(pair => pair.Value).Cast<TargetPropertyDescription>();
        foreach (var property in properties) {
          if (property.SourceProperty==null)
            continue;
          var sourceProperty = property.SourceProperty;
          if (property.IsPrimitive)
            ValidatePrimitiveProperty(property, sourceProperty);
          else if (property.IsCollection)
            ValidateCollectionProperty(property, sourceProperty);
          else
            ValidateReferenceProperty(property, sourceProperty);
        }
      }
    }

    private void ValidateReferenceProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (sourceProperty.IsPrimitive || sourceProperty.IsCollection)
        throw new InvalidOperationException(
          String.Format(Strings.ExReferencePropertyXIsBoundToPropertyYThatIsNotReference, property,
          sourceProperty));
      description.EnsureTargetTypeIsRegistered(property.SystemProperty.PropertyType);
    }

    private void ValidateCollectionProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (!sourceProperty.IsCollection)
        throw new InvalidOperationException(
          String.Format(Strings.ExCollectionPropertyXIsBoundToPropertyYThatIsNotCollection, property,
          sourceProperty));
      if (!MappingHelper.IsTypePrimitive(property.ItemType))
        description.EnsureTargetTypeIsRegistered(property.ItemType);
    }

    private void ValidatePrimitiveProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (!sourceProperty.IsPrimitive)
        throw new InvalidOperationException(
          String.Format(Strings.ExPrimitivePropertyXIsBoundToPropertyYThatIsNotPrimitive, property,
          sourceProperty));
      var areCompatible = property.SystemProperty.PropertyType
        .IsAssignableFrom(sourceProperty.SystemProperty.PropertyType);
      if (!areCompatible)
        throw new InvalidOperationException(
          String.Format(Strings.ExPropertiesXAndYHaveIncompatibleTypes, property, sourceProperty));
    }
  }
}
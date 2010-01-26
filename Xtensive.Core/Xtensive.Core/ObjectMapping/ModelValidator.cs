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
          if (property.IsCollection)
            ValidateCollectionProperty(property, sourceProperty);
          else if (property.ValueType.ObjectKind==ObjectKind.Primitive)
            ValidatePrimitiveProperty(property, sourceProperty);
          else
            ValidateReferenceProperty(property, sourceProperty);
        }
      }
    }

    private void ValidateReferenceProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (sourceProperty.IsCollection
        || MappingHelper.IsTypePrimitive(sourceProperty.SystemProperty.PropertyType))
        throw new InvalidOperationException(
          String.Format(Strings.ExReferencePropertyXIsBoundToPropertyYThatIsNotReference, property,
          sourceProperty));
    }

    private void ValidateCollectionProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (!sourceProperty.IsCollection)
        throw new InvalidOperationException(
          String.Format(Strings.ExCollectionPropertyXIsBoundToPropertyYThatIsNotCollection, property,
          sourceProperty));
      if (property.ValueType.ObjectKind==ObjectKind.UserStructure && !property.IsChangeTrackingDisabled)
        throw new InvalidOperationException(Strings.ExDetectionOfChangesInUserStructureCollectionIsNotSupported);
    }

    private void ValidatePrimitiveProperty(TargetPropertyDescription property,
      SourcePropertyDescription sourceProperty)
    {
      if (!MappingHelper.IsTypePrimitive(sourceProperty.SystemProperty.PropertyType))
        throw new InvalidOperationException(
          String.Format(Strings.ExPrimitivePropertyXIsBoundToPropertyYThatIsNotPrimitive, property,
          sourceProperty));
    }
  }
}
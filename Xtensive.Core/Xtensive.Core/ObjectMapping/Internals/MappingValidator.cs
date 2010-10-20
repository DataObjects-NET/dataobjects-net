// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.21

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.ObjectMapping.Model;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping
{
  internal sealed class MappingValidator
  {
    private MappingDescription description;

    public void Validate(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      description = mappingDescription;
      foreach (var type in description.TargetTypes) {
        EnsureTypeIsClassOrValueType(type);
        EnsureTypeIsConcrete(type);
        if (type.SourceType!=null)
          EnsureTypeIsClassOrValueType(type.SourceType);
        var properties = type.Properties.Select(pair => pair.Value).Cast<TargetPropertyDescription>();
        foreach (var property in properties.Where(p => !p.IsIgnored)) {
          CheckSetterAndGetter(property);
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

    #region Private \ internal methods

    private static void EnsureTypeIsClassOrValueType(TypeDescription type)
    {
      if (!type.SystemType.IsClass && !type.SystemType.IsValueType)
        throw new InvalidOperationException(String.Format(Strings.ExXIsNeitherClassNorValueType, type));
    }

    private static void EnsureTypeIsConcrete(TypeDescription type)
    {
      if (type.SystemType.IsAbstract)
        throw new InvalidOperationException(String.Format(Strings.ExTypeXMustBeNonAbstractType, type));
    }

    private static void CheckSetterAndGetter(TargetPropertyDescription property)
    {
      if (property.SystemProperty.GetSetMethod()==null)
        throw new InvalidOperationException(
          String.Format(Strings.ExPropertyDoesNotHaveSetter, property.SystemProperty, property.ReflectedType));
      if (!property.IsChangeTrackingDisabled && property.SystemProperty.GetGetMethod()==null)
        throw new InvalidOperationException(
          String.Format(Strings.ExPropertyDoesNotHaveGetter, property.SystemProperty, property.ReflectedType));
      if (property.SourceProperty!=null && property.SourceProperty.SystemProperty.GetGetMethod()==null)
        throw new InvalidOperationException(
          String.Format(Strings.ExPropertyDoesNotHaveGetter, property.SourceProperty,
          property.SourceProperty.ReflectedType));
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
      if (IsNullable(property.SystemProperty) && !IsNullable(sourceProperty.SystemProperty))
        throw new InvalidOperationException(String.Format(Strings.ExNullablePropertyXIsBoundToPropertyYThatIsNotNullable,
          property, sourceProperty));
      if (!IsNullable(property.SystemProperty) && IsNullable(sourceProperty.SystemProperty))
        throw new InvalidOperationException(String.Format(Strings.ExNullablePropertyXIsBoundToPropertyYThatIsNotNullable,
          sourceProperty, property));
      if (property.Converter==MappingBuilder.DefaultPrimitiveConverter
        && property.SystemProperty.PropertyType!=sourceProperty.SystemProperty.PropertyType) {
        throw new InvalidOperationException(String.Format(Strings.ExPropertiesXAndYHaveDifferentPrimitiveTypes,
          sourceProperty, property));}
    }

    private bool IsNullable(PropertyInfo property)
    {
      return property.PropertyType.IsGenericType
        && property.PropertyType.GetGenericTypeDefinition()==typeof (Nullable<>);
    }

    #endregion
  }
}
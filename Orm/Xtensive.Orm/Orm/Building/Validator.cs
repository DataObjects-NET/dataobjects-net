// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Reflection;
using Xtensive.Sql;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;

namespace Xtensive.Orm.Building
{
  internal class Validator
  {
    private readonly HashSet<Type> ValidFieldTypes = new HashSet<Type>();
    private readonly Regex ColumnNamingRule;
    private readonly Regex TypeNamingRule;
    private readonly Regex FieldNamingRule;

    /// <summary>
    /// Determines whether the specified name is valid.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="rule">The validation rule.</param>
    /// <returns>
    /// <see langword="true"/> if the specified name is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public void ValidateName(string name, ValidationRule rule)
    {
      if (String.IsNullOrEmpty(name))
        throw new DomainBuilderException(String.Format(Strings.ExXNameCantBeEmpty, rule));

      Regex namingRule;
      switch (rule) {
        case ValidationRule.Index:
        case ValidationRule.Type:
        case ValidationRule.Schema:
        case ValidationRule.Database:
          namingRule = TypeNamingRule;
          break;
        case ValidationRule.Field:
          namingRule = FieldNamingRule;
          break;
        case ValidationRule.Column:
          namingRule = ColumnNamingRule;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      if (!namingRule.IsMatch(name))
        throw new DomainBuilderException(string.Format(Strings.ExXIsNotValidNameForX, name, rule));
    }

    public void ValidateHierarchyRoot(DomainModelDef modelDef, TypeDef typeDef)
    {
      // Ensures that typeDef doesn't belong to another hierarchy
      TypeDef root = modelDef.FindRoot(typeDef);
      if (root!=null && typeDef!=root)
        throw new DomainBuilderException(
          String.Format(Strings.ExTypeDefXIsAlreadyBelongsToHierarchyWithTheRootY,
            typeDef.UnderlyingType.GetFullName(), root.UnderlyingType.GetFullName()));

      // Ensures that typeDef is not an ancestor of any other hierarchy root
      foreach (HierarchyDef hierarchy in modelDef.Hierarchies)
        if (hierarchy.Root.UnderlyingType.IsSubclassOf(typeDef.UnderlyingType))
          throw new DomainBuilderException(string.Format(
            Strings.ExXDescendantIsAlreadyARootOfAnotherHierarchy, hierarchy.Root.UnderlyingType));
    }

    public void ValidateHierarchy(HierarchyDef hierarchyDef)
    {
      var keyFields = hierarchyDef.KeyFields;

      if (keyFields.Count==0)
        throw new DomainBuilderException(string.Format(
          Strings.ExHierarchyXDoesntContainAnyKeyFields, hierarchyDef.Root.Name));

      var root = hierarchyDef.Root;

      if (hierarchyDef.KeyGeneratorKind==KeyGeneratorKind.Default) {
        // Default key generator can't produce values with 2 or more fields
        if (keyFields.Count > 2)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        // if one of key fields is TypeId field and number of fields == 2 then it is OK
        if (keyFields.Count==2 && keyFields.Find(f => f.Name==WellKnown.TypeIdFieldName)!=null)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        var field = keyFields.FirstOrDefault(f => f.Name!=WellKnown.TypeIdFieldName);
      }

      foreach (var keyField in keyFields) {

        if (keyField==null)
          throw new DomainBuilderException(String.Format(Strings.ExKeyStructureForXContainsNULLValue, root.Name));

        FieldDef fieldDef = root.Fields[keyField.Name];
        if (fieldDef==null)
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldXXIsNotFound, root.Name, keyField.Name));

        ValidateFieldType(root, fieldDef.ValueType, true);

        if (fieldDef.IsLazyLoad)
          throw new DomainBuilderException(string.Format(
            Strings.ExFieldXCannotBeLazyLoadAsItIsIncludedInPrimaryKey, fieldDef.Name));
      }
    }

    internal void ValidateFieldType(TypeDef declaringType, Type fieldType, bool isKeyField)
    {
      if (fieldType.IsGenericType) {
        Type genericType = fieldType.GetGenericTypeDefinition();
        if (genericType==typeof (Nullable<>)) {
          ValidateFieldType(declaringType, Nullable.GetUnderlyingType(fieldType), isKeyField);
          return;
        }
      }

      if (fieldType.IsArray && isKeyField)
        throw new DomainBuilderException(String.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));

      if (fieldType.IsPrimitive || fieldType.IsEnum || ValidFieldTypes.Contains(fieldType))
        return;

      if (fieldType.IsSubclassOf(typeof (Entity)))
        return;

      if (fieldType.IsInterface && typeof (IEntity).IsAssignableFrom(fieldType) && fieldType!=typeof (IEntity))
        return;

      if (fieldType.IsSubclassOf(typeof (Structure))) {
        if (isKeyField)
          throw new DomainBuilderException(String.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));
        return;
      }

      if (fieldType.IsOfGenericType(typeof (EntitySet<>))) {
        if (declaringType.IsStructure)
          throw new DomainBuilderException(
            String.Format(Strings.ExStructuresDoNotSupportFieldsOfTypeX, fieldType.Name));
        if (isKeyField)
          throw new DomainBuilderException(String.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));
        return;
      }

      throw new DomainBuilderException(String.Format(Strings.ExUnsupportedType, fieldType.GetShortName()));
    }

    internal void ValidateVersionField(FieldDef field, bool isKeyField)
    {
      if (isKeyField)
        throw new DomainBuilderException(string.Format(
          Strings.ExPrimaryKeyFieldXCanTBeMarkedAsVersion, field.Name));
      if (field.IsLazyLoad)
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeLazyLoadField, field.Name));
      if (field.IsEntitySet)
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeOfYType, field.Name, field.ValueType.GetShortName()));
      if (field.ValueType.IsArray)
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeOfYType, field.Name, field.ValueType.GetShortName()));
      if (field.IsSystem)
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeSystemField, field.Name));
      if (field.IsTypeId)
        throw new DomainBuilderException(string.Format(
          Strings.VersionFieldXCanTBeTypeIdField, field.Name));
      if (field.IsTypeDiscriminator)
        throw new DomainBuilderException(string.Format(
          Strings.VersionFieldXCanTBeTypeIdField, field.Name));
      if ((field.Attributes & (FieldAttributes.AutoVersion | FieldAttributes.ManualVersion )) > 0 && field.IsStructure)
        throw new DomainBuilderException(string.Format(
          Strings.ExUnableToApplyVersionOnFieldXOfTypeY, field.Name, field.ValueType.GetShortName()));
    }

    internal void ValidateType(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
    }

    public void EnsureTypeIsPersistent(Type type)
    {
      if (type.IsClass && type.IsSubclassOf(typeof (Persistent)))
        return;

      if (type.IsInterface && typeof (IEntity).IsAssignableFrom(type))
        return;

      throw new DomainBuilderException(String.Format(Strings.ExUnsupportedType, type));
    }

    public void ValidateStructureField(TypeDef typeDef, FieldDef fieldDef)
    {
      if (fieldDef.ValueType==typeDef.UnderlyingType)
        throw new DomainBuilderException(String.Format(Strings.ExStructureXCantContainFieldOfTheSameType, typeDef.Name));
    }

    public void ValidateEntitySetField(TypeDef typeDef, FieldDef fieldDef)
    {
      // Restriction for EntitySet properties only
      if (fieldDef.OnTargetRemove == OnRemoveAction.Cascade || fieldDef.OnTargetRemove == OnRemoveAction.None)
        throw new DomainBuilderException(string.Format(
          Strings.ExValueIsNotAcceptableForOnTargetRemoveProperty, typeDef.Name, fieldDef.Name, fieldDef.OnTargetRemove));
    }

    /// <exception cref="DomainBuilderException">Field cannot be nullable.</exception>
    internal void EnsureIsNullable(Type valueType)
    {
      if (!(typeof(IEntity).IsAssignableFrom(valueType) || valueType==typeof (string) || valueType==typeof (byte[])))
        throw new DomainBuilderException(string.Format(
          Strings.ExFieldOfTypeXCannotBeNullableForValueTypesConsiderUsingNullableT, valueType));
    }

    // Type initializer

    public Validator()
    {
      ColumnNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      TypeNamingRule = new Regex(@"^[\w][\w\-\.\(\),]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      FieldNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

      foreach (var type in SqlType.RegisteredTypes)
        ValidFieldTypes.Add(type.Value);
      ValidFieldTypes.Add(typeof (Key));
    }

    public void ValidateHierarchyEquality(TypeDef @interface, HierarchyDef first, HierarchyDef second)
    {
      // TypeId mode must match
      if (first.IncludeTypeId != second.IncludeTypeId)
        throw new DomainBuilderException(string.Format(
          Strings.ExImplementorsOfXInterfaceBelongToHierarchiesOneOfWhichIncludesTypeIdButAnotherDoesntYZ,
          @interface.Name, first.Root.Name, second.Root.Name));

      // Number of key fields must match
      if (first.KeyFields.Count != second.KeyFields.Count)
        throw new DomainBuilderException(string.Format(
          Strings.ExImplementorsOfXInterfaceBelongToHierarchiesWithDifferentKeyStructureYZ,
          @interface.Name, first.Root.Name, second.Root.Name));

      // Type of each key field must match
      for (int i = 0; i < first.KeyFields.Count; i++) {
        var masterField = first.Root.Fields[first.KeyFields[i].Name];
        var candidateField = second.Root.Fields[second.KeyFields[i].Name];
        if (masterField.ValueType != candidateField.ValueType)
          throw new DomainBuilderException(string.Format(
            Strings.ExImplementorsOfXInterfaceBelongToHierarchiesWithDifferentKeyStructureYZ,
            @interface.Name, first.Root.Name, second.Root.Name));
      }
    }
  }
}
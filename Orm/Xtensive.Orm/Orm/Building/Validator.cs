// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;

namespace Xtensive.Orm.Building
{
  internal class Validator
  {
    private readonly HashSet<Type> validFieldTypes;
    private readonly Regex columnNamingRule;
    private readonly Regex typeNamingRule;
    private readonly Regex fieldNamingRule;

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
      if (string.IsNullOrEmpty(name)) {
        throw new DomainBuilderException(string.Format(Strings.ExXNameCantBeEmpty, rule));
      }

      Regex namingRule;
      switch (rule) {
        case ValidationRule.Index:
        case ValidationRule.Type:
        case ValidationRule.Schema:
        case ValidationRule.Database:
          namingRule = typeNamingRule;
          break;
        case ValidationRule.Field:
          namingRule = fieldNamingRule;
          break;
        case ValidationRule.Column:
          namingRule = columnNamingRule;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      if (!namingRule.IsMatch(name)) {
        throw new DomainBuilderException(string.Format(Strings.ExXIsNotValidNameForX, name, rule));
      }
    }

    public void ValidateHierarchyRoot(DomainModelDef modelDef, TypeDef typeDef)
    {
      // Ensures that typeDef doesn't belong to another hierarchy
      var root = modelDef.FindRoot(typeDef);
      if (root != null && typeDef != root) {
        throw new DomainBuilderException(
          string.Format(Strings.ExTypeDefXIsAlreadyBelongsToHierarchyWithTheRootY,
            typeDef.UnderlyingType.GetFullName(), root.UnderlyingType.GetFullName()));
      }

      // Ensures that typeDef is not an ancestor of any other hierarchy root
      foreach (var hierarchy in modelDef.Hierarchies) {
        if (hierarchy.Root.UnderlyingType.IsSubclassOf(typeDef.UnderlyingType)) {
          throw new DomainBuilderException(string.Format(
            Strings.ExXDescendantIsAlreadyARootOfAnotherHierarchy, hierarchy.Root.UnderlyingType));
        }
      }
    }

    public void ValidateHierarchy(HierarchyDef hierarchyDef)
    {
      var keyFields = hierarchyDef.KeyFields;

      if (keyFields.Count == 0) {
        throw new DomainBuilderException(string.Format(
          Strings.ExHierarchyXDoesntContainAnyKeyFields, hierarchyDef.Root.Name));
      }

      var root = hierarchyDef.Root;

      if (hierarchyDef.KeyGeneratorKind == KeyGeneratorKind.Default) {
        // Default key generator can't produce values with 2 or more fields
        if (keyFields.Count > 2) {
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        }

        // if one of key fields is TypeId field and number of fields == 2 then it is OK
        if (keyFields.Count == 2 && keyFields.Find(f => f.Name == WellKnown.TypeIdFieldName) != null) {
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        }
      }

      foreach (var keyField in keyFields) {

        if (keyField == null) {
          throw new DomainBuilderException(string.Format(Strings.ExKeyStructureForXContainsNULLValue, root.Name));
        }

        var fieldDef = root.Fields[keyField.Name];
        if (fieldDef == null) {
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldXXIsNotFound, root.Name, keyField.Name));
        }

        ValidateFieldType(root, fieldDef.ValueType, true);

        if (fieldDef.IsLazyLoad) {
          throw new DomainBuilderException(string.Format(
            Strings.ExFieldXCannotBeLazyLoadAsItIsIncludedInPrimaryKey, fieldDef.Name));
        }
      }
    }

    internal void ValidateFieldType(TypeDef declaringType, Type fieldType, bool isKeyField)
    {
      var nullableFieldType = fieldType.StripNullable();
      if (!ReferenceEquals(fieldType, nullableFieldType)) {
        // Field type is nullable
        ValidateFieldType(declaringType, nullableFieldType, isKeyField);
        return;
      }

      if (fieldType.IsArray && isKeyField) {
        throw new DomainBuilderException(string.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));
      }

      if (fieldType.IsPrimitive || fieldType.IsEnum || validFieldTypes.Contains(fieldType)) {
        return;
      }

      if (fieldType.IsSubclassOf(WellKnownOrmTypes.Entity)) {
        return;
      }

      if (fieldType.IsInterface && WellKnownOrmInterfaces.Entity.IsAssignableFrom(fieldType)
        && fieldType != WellKnownOrmInterfaces.Entity) {
        return;
      }

      if (fieldType.IsSubclassOf(WellKnownOrmTypes.Structure)) {
        if (isKeyField) {
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));
        }

        return;
      }

      if (fieldType.IsOfGenericType(WellKnownOrmTypes.EntitySetOfT)) {
        if (declaringType.IsStructure) {
          throw new DomainBuilderException(
            string.Format(Strings.ExStructuresDoNotSupportFieldsOfTypeX, fieldType.Name));
        }

        if (isKeyField) {
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldCantBeOfXType, fieldType.GetShortName()));
        }

        return;
      }

      throw new DomainBuilderException(string.Format(Strings.ExUnsupportedType, fieldType.GetShortName()));
    }

    internal void ValidateVersionField(FieldDef field, bool isKeyField)
    {
      if (isKeyField) {
        throw new DomainBuilderException(string.Format(
          Strings.ExPrimaryKeyFieldXCanTBeMarkedAsVersion, field.Name));
      }

      if (field.IsLazyLoad) {
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeLazyLoadField, field.Name));
      }

      if (field.IsEntitySet) {
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeOfYType, field.Name, field.ValueType.GetShortName()));
      }

      if (field.ValueType.IsArray) {
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeOfYType, field.Name, field.ValueType.GetShortName()));
      }

      if (field.IsSystem) {
        throw new DomainBuilderException(string.Format(
          Strings.ExVersionFieldXCanTBeSystemField, field.Name));
      }

      if (field.IsTypeId) {
        throw new DomainBuilderException(string.Format(
          Strings.VersionFieldXCanTBeTypeIdField, field.Name));
      }

      if (field.IsTypeDiscriminator) {
        throw new DomainBuilderException(string.Format(
          Strings.VersionFieldXCanTBeTypeIdField, field.Name));
      }

      if ((field.Attributes & (FieldAttributes.AutoVersion | FieldAttributes.ManualVersion)) > 0 && field.IsStructure) {
        throw new DomainBuilderException(string.Format(
          Strings.ExUnableToApplyVersionOnFieldXOfTypeY, field.Name, field.ValueType.GetShortName()));
      }
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

    internal void ValidateType(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
    }

    public void EnsureTypeIsPersistent(Type type)
    {
      if (type.IsClass && type.IsSubclassOf(WellKnownOrmTypes.Persistent)) {
        return;
      }

      if (type.IsInterface && WellKnownOrmInterfaces.Entity.IsAssignableFrom(type)) {
        return;
      }

      throw new DomainBuilderException(string.Format(Strings.ExUnsupportedType, type));
    }

    public void ValidateStructureField(TypeDef typeDef, FieldDef fieldDef)
    {
      if (fieldDef.ValueType == typeDef.UnderlyingType) {
        throw new DomainBuilderException(
          string.Format(Strings.ExStructureXCantContainFieldOfTheSameType, typeDef.Name));
      }
    }

    public void ValidateEntitySetField(TypeDef typeDef, FieldDef fieldDef)
    {
      // Restriction for EntitySet properties only
      if (fieldDef.OnTargetRemove == OnRemoveAction.Cascade || fieldDef.OnTargetRemove == OnRemoveAction.None) {
        throw new DomainBuilderException(string.Format(
          Strings.ExValueIsNotAcceptableForOnTargetRemoveProperty, typeDef.Name, fieldDef.Name, fieldDef.OnTargetRemove));
      }
    }

    /// <exception cref="DomainBuilderException">Field cannot be nullable.</exception>
    internal void EnsureIsNullable(Type valueType)
    {
      if (!(WellKnownOrmInterfaces.Entity.IsAssignableFrom(valueType) || valueType == WellKnownTypes.String
        || valueType == WellKnownTypes.ByteArray)) {
        throw new DomainBuilderException(string.Format(
          Strings.ExFieldOfTypeXCannotBeNullableForValueTypesConsiderUsingNullableT, valueType));
      }
    }

    // Type initializer

    public Validator(IEnumerable<Type> validFieldTypes)
    {
      columnNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      typeNamingRule = new Regex(@"^[\w][\w\-\.\(\),]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      fieldNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

      this.validFieldTypes = new HashSet<Type>(validFieldTypes) {WellKnownOrmTypes.Key};
    }
  }
}

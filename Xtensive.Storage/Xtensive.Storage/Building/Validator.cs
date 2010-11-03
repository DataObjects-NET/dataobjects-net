// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Resources;
using FieldAttributes = Xtensive.Storage.Model.FieldAttributes;

namespace Xtensive.Storage.Building
{
  internal static class Validator
  {
    private static readonly HashSet<Type> ValidFieldTypes = new HashSet<Type>();
    private static readonly Regex ColumnNamingRule;
    private static readonly Regex TypeNamingRule;
    private static readonly Regex FieldNamingRule;

    /// <summary>
    /// Determines whether the specified name is valid.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="rule">The validation rule.</param>
    /// <returns>
    /// <see langword="true"/> if the specified name is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public static void ValidateName(string name, ValidationRule rule)
    {
      if (String.IsNullOrEmpty(name))
        throw new DomainBuilderException(String.Format(Strings.ExXNameCantBeEmpty, rule));

      Regex namingRule;
      switch (rule) {
        case ValidationRule.Index:
        case ValidationRule.Type:
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
        throw new DomainBuilderException(String.Format(Strings.ExXIsNotValidNameForX, name, rule));
    }

    public static void ValidateHierarchyRoot(TypeDef typeDef)
    {
      BuildingContext context = BuildingContext.Demand();

      // Ensures that typeDef doesn't belong to another hierarchy
      TypeDef root = context.ModelDef.FindRoot(typeDef);
      if (root!=null && typeDef!=root)
        throw new DomainBuilderException(
          String.Format(Strings.ExTypeDefXIsAlreadyBelongsToHierarchyWithTheRootY,
            typeDef.UnderlyingType.GetFullName(), root.UnderlyingType.GetFullName()));

      // Ensures that typeDef is not an ancestor of any other hierarchy root
      foreach (HierarchyDef hierarchy in context.ModelDef.Hierarchies)
        if (hierarchy.Root.UnderlyingType.IsSubclassOf(typeDef.UnderlyingType))
          throw new DomainBuilderException(
            String.Format(Strings.ExXDescendantIsAlreadyARootOfAnotherHierarchy, hierarchy.Root.UnderlyingType));
    }

    public static void ValidateHierarchy(HierarchyDef hierarchyDef)
    {
      if (hierarchyDef.KeyFields.Count==0)
        throw new DomainBuilderException(String.Format(Strings.ExHierarchyXDoesntContainAnyKeyFields, hierarchyDef.Root.Name));

      var root = hierarchyDef.Root;

      if (hierarchyDef.KeyGeneratorType == typeof(KeyGenerator)) {
        // Default key generator can't produce values with 2 or more fields
        if (hierarchyDef.KeyFields.Count > 2)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        // if one of key fields is TypeId field and number of fields == 2 then it is OK
        if (hierarchyDef.KeyFields.Count==2 && hierarchyDef.KeyFields.Find(f => f.Name == WellKnown.TypeIdFieldName) == null)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
      }

      foreach (var keyField in hierarchyDef.KeyFields) {

        if (keyField == null)
          throw new DomainBuilderException(String.Format(Strings.ExKeyStructureForXContainsNULLValue, root.Name));

        FieldDef fieldDef = root.Fields[keyField.Name];
        if (fieldDef==null)
          throw new DomainBuilderException(String.Format(Strings.ExKeyFieldXXIsNotFound, root.Name, keyField.Name));

        ValidateFieldType(root, fieldDef.ValueType, true);

        // If property has a setter, it must be private
        if (fieldDef.UnderlyingProperty==null)
          continue;
        var setter = fieldDef.UnderlyingProperty.GetSetMethod(true);
        if (setter != null && (setter.Attributes & MethodAttributes.Private) == 0) {
          var debuggerNonUserCodeAttribute = setter
            .GetAttribute<DebuggerNonUserCodeAttribute>(AttributeSearchOptions.Default);
          var compilerGeneratedAttribute = setter
            .GetAttribute<CompilerGeneratedAttribute>(AttributeSearchOptions.Default);
          if (debuggerNonUserCodeAttribute == null || compilerGeneratedAttribute != null)
            throw new DomainBuilderException(
              String.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, keyField.Name, hierarchyDef.Root.Name));
        }

        if (fieldDef.IsLazyLoad)
          throw new DomainBuilderException(String.Format(Strings.ExFieldXCannotBeLazyLoadAsItIsIncludedInPrimaryKey, fieldDef.Name));
      }
    }

    internal static void ValidateFieldType(TypeDef declaringType, Type fieldType, bool isKeyField)
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

    internal static void ValidateVersionField(FieldDef field, bool isKeyField)
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

    internal static void ValidateType(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
//      if (typeDef.Fields.Any(field => field.IsVersion) && hierarchyDef.Root!=typeDef)
//        throw new DomainBuilderException(string.Format(
//          Strings.ExTypeXCantContainsVersionFieldsAsItsNotAHierarchyRoot, typeDef.Name));
    }

    public static void EnsureUnderlyingTypeIsAspected(TypeDef type)
    {
      var constructor = type.UnderlyingType.GetConstructor(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new[] {typeof (EntityState)});
      if (constructor!=null)
        return;
      var typeName = type.UnderlyingType.GetFullName();
      var assemblyName = type.UnderlyingType.Assembly.GetName().FullName;
      throw new DomainBuilderException(String.Format(
        Strings.ExPersistentAttributeIsNotSetOnTypeXOrAssemblyYIsNotProcessedByPostSharp,
        typeName, assemblyName));
    }

    public static void ValidateKeyGeneratorType(Type type)
    {
      if (!typeof(KeyGenerator).IsAssignableFrom(type))
        throw new InvalidOperationException(String.Format(Strings.ExXMustBeInheritedFromX, type.GetShortName(), typeof(KeyGenerator).GetShortName()));
    }

    public static void EnsureTypeIsPersistent(Type type)
    {
      if (type.IsClass && type.IsSubclassOf(typeof (Persistent)))
        return;

      if (type.IsInterface && typeof (IEntity).IsAssignableFrom(type))
        return;

      throw new DomainBuilderException(String.Format(Strings.ExUnsupportedType, type));
    }

    public static void ValidateStructureField(TypeDef typeDef, FieldDef fieldDef)
    {
      if (fieldDef.ValueType==typeDef.UnderlyingType)
        throw new DomainBuilderException(String.Format(Strings.ExStructureXCantContainFieldOfTheSameType, typeDef.Name));
    }

    public static void ValidateEntitySetField(TypeDef typeDef, FieldDef fieldDef)
    {
      // Restriction for EntitySet properties only
      if (fieldDef.OnTargetRemove == OnRemoveAction.Cascade || fieldDef.OnTargetRemove == OnRemoveAction.None)
        throw new DomainBuilderException(String.Format(Strings.ExValueIsNotAcceptableForOnTargetRemoveProperty, typeDef.Name, fieldDef.Name, fieldDef.OnTargetRemove));
    }

    /// <exception cref="DomainBuilderException">Field cannot be nullable.</exception>
    internal static void EnsureIsNullable(Type valueType)
    {
      if (!(typeof(IEntity).IsAssignableFrom(valueType) || valueType==typeof (string) || valueType==typeof (byte[])))
        throw new DomainBuilderException(String.Format(Strings.ExFieldOfTypeXCannotBeNullableForValueTypesConsiderUsingNullableT, valueType));
    }

    // Type initializer

    static Validator()
    {
      ColumnNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      TypeNamingRule = new Regex(@"^[\w][\w\-\.\(\),]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      FieldNamingRule = new Regex(@"^[\w][\w\-\.]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

      ValidFieldTypes.Add(typeof (string));
      ValidFieldTypes.Add(typeof (byte[]));
      ValidFieldTypes.Add(typeof (Guid));
      ValidFieldTypes.Add(typeof (DateTime));
      ValidFieldTypes.Add(typeof (TimeSpan));
      ValidFieldTypes.Add(typeof (decimal));
      ValidFieldTypes.Add(typeof (Key));
    }

    public static void ValidateHierarchyEquality(TypeDef @interface, HierarchyDef first, HierarchyDef second)
    {
      // TypeId mode must match
      if (first.IncludeTypeId != second.IncludeTypeId)
        throw new DomainBuilderException(String.Format("Implementors of {0} interface belong to hierarchies one of which includes TypeId, but another doesn't: {1} & {2}.", @interface.Name, first.Root.Name, second.Root.Name));

      // Number of key fields must match
      if (first.KeyFields.Count != second.KeyFields.Count)
        throw new DomainBuilderException(String.Format("Implementors of {0} interface belong to hierarchies with different key structure: {1} & {2}.", @interface.Name, first.Root.Name, second.Root.Name));

      // Type of each key field must match
      for (int i = 0; i < first.KeyFields.Count; i++) {
        var masterField = first.Root.Fields[first.KeyFields[i].Name];
        var candidateField = second.Root.Fields[second.KeyFields[i].Name];
        if (masterField.ValueType != candidateField.ValueType)
          throw new DomainBuilderException(String.Format("Implementors of {0} interface belong to hierarchies with different key structure: {1} & {2}.", @interface.Name, first.Root.Name, second.Root.Name));
      }
    }
  }
}
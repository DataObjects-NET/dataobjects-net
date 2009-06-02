// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

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
    public static void EnsureNameIsValid(string name, ValidationRule rule)
    {
      if (String.IsNullOrEmpty(name))
        throw new DomainBuilderException(string.Format("'{0}' name can't be empty.", rule));

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
        throw new DomainBuilderException(string.Format("'{0}' is not valid name for {1}.", name, rule));
    }

    public static void EnsureHierarchyRootIsValid(TypeDef typeDef)
    {
      BuildingContext context = BuildingContext.Current;

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

    public static void EnsureHierarchyIsValid(HierarchyDef hierarchyDef)
    {
      if (hierarchyDef.KeyFields.Count==0)
        throw new DomainBuilderException(string.Format("Hierarchy '{0}' doesn't contain any key fields.", hierarchyDef.Root.Name));

      var context = BuildingContext.Current;
      var root = hierarchyDef.Root;

      if (hierarchyDef.KeyGenerator == typeof(KeyGenerator)) {
        // Default key generator can't produce values with 2 or more fields
        if (hierarchyDef.KeyFields.Count > 2)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        // if one of key fields is TypeId field and number of fields == 2 then it is OK
        if (hierarchyDef.KeyFields.Count==2 && hierarchyDef.KeyFields.Find(f => f.Name == WellKnown.TypeIdField) == null)
          throw new DomainBuilderException(Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
      }

      foreach (var keyField in hierarchyDef.KeyFields) {

        if (keyField == null)
          throw new DomainBuilderException(string.Format("Key structure for '{0}' contains NULL value.", root.Name));

        FieldDef fieldDef = root.Fields[keyField.Name];
        if (fieldDef==null)
          throw new DomainBuilderException(string.Format("Key field '{0}.{1}' is not found.", root.Name, keyField.Name));

        EnsureFieldTypeIsValid(root, fieldDef.ValueType, true);

        // If property has a setter, it must be private
        if (fieldDef.UnderlyingProperty==null)
          continue;
        var setter = fieldDef.UnderlyingProperty.GetSetMethod(true);
        if (setter!=null && (setter.Attributes & MethodAttributes.Private)==0)
          throw new DomainBuilderException(
            string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, keyField.Name, hierarchyDef.Root.Name));
      }
    }

    private static void EnsureFieldTypeIsValid(TypeDef declaringType, Type fieldType, bool isKeyField)
    {
      if (fieldType.IsGenericType) {
        Type genericType = fieldType.GetGenericTypeDefinition();
        if (genericType==typeof (Nullable<>))
          EnsureFieldTypeIsValid(declaringType, Nullable.GetUnderlyingType(fieldType), isKeyField);
      }

      if (fieldType.IsPrimitive || fieldType.IsEnum || ValidFieldTypes.Contains(fieldType))
        return;

      if (fieldType.IsSubclassOf(typeof (Entity)))
        return;

      if (fieldType.IsInterface && typeof (IEntity).IsAssignableFrom(fieldType) && fieldType!=typeof (IEntity))
        return;

      if (fieldType.IsSubclassOf(typeof (Structure))) {
        if (isKeyField)
          throw new DomainBuilderException(string.Format("Key field can't be of '{0}' type.", fieldType.GetShortName()));
        return;
      }

      if (fieldType.IsOfGenericType(typeof (EntitySet<>))) {
        if (declaringType.IsStructure)
          throw new DomainBuilderException(
            string.Format("Structures do not support fields of type '{0}'.", fieldType.Name));
        if (isKeyField)
          throw new DomainBuilderException(string.Format("Key field can't be of '{0}' type.", fieldType.GetShortName()));
      }
      throw new DomainBuilderException(string.Format("Unsupported type: '{0}'", fieldType.GetShortName()));
    }

    public static void EnsureUnderlyingTypeIsAspected(TypeDef type)
    {
      var constructor = type.UnderlyingType.GetConstructor(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new[] {typeof (EntityState), typeof (bool)});
      if (constructor!=null)
        return;
      var assemblyName = type.UnderlyingType.Assembly.GetName().Name;
      throw new DomainBuilderException(string.Format(
        Strings.ExPersistentAttributeIsNotSetOnTypeX, assemblyName));
    }

    public static void EnsureKeyGeneratorTypeIsValid(Type type)
    {
      if (!typeof(KeyGenerator).IsAssignableFrom(type))
        throw new InvalidOperationException(string.Format("'{0}' must be inherited from '{1}'.", type.GetShortName(), typeof(KeyGenerator).GetShortName()));
    }

    public static void EnsureTypeIsPersistent(Type type)
    {
      if (type.IsClass && type.IsSubclassOf(typeof (Persistent)))
        return;

      if (type.IsInterface && typeof (IEntity).IsAssignableFrom(type))
        return;

      throw new DomainBuilderException(string.Format(Strings.ExUnsupportedType, type));
    }

    // Type initializer

    static Validator()
    {
      ColumnNamingRule = new Regex(@"^[A-z][A-z0-9\-\._]*$", RegexOptions.Compiled);
      TypeNamingRule = new Regex(@"^[A-z][A-z0-9\-\.\(\)_,]*$", RegexOptions.Compiled);
      FieldNamingRule = new Regex(@"^[A-z][A-z0-9\-_]*$", RegexOptions.Compiled);

      ValidFieldTypes.Add(typeof (string));
      ValidFieldTypes.Add(typeof (byte[]));
      ValidFieldTypes.Add(typeof (Guid));
      ValidFieldTypes.Add(typeof (DateTime));
      ValidFieldTypes.Add(typeof (TimeSpan));
      ValidFieldTypes.Add(typeof (decimal));
      ValidFieldTypes.Add(typeof (Key));
    }
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Building
{
  internal static class Validator
  {
    private static readonly Type entitySetType = typeof (EntitySet<>);
    private static readonly ValidationResult failureResult = new ValidationResult(false);
    private static readonly Type persistentType = typeof (Persistent);
    private static readonly Dictionary<ValidationRule, Regex> regexps = new Dictionary<ValidationRule, Regex>(5);
    private static readonly Type stringType = typeof (string);
    private static readonly ValidationResult successResult = new ValidationResult(true);

    public static ValidationResult ValidateName(string name, ValidationRule rule)
    {
      switch (rule) {
      case ValidationRule.Type:
      case ValidationRule.Service:
      case ValidationRule.Field:
      case ValidationRule.Column:
      case ValidationRule.Index:
        if (string.IsNullOrEmpty(name))
          return failureResult;
        if (regexps.ContainsKey(rule)) {
          Regex regexp = regexps[rule];
          return regexp.IsMatch(name) ? successResult : failureResult;
        }
        return successResult;
      }
      return failureResult;
    }

//    public static ValidationResult ValidateConverter(FieldDef field,  Type type)
//    {
//      // Check parametreless constructor
//      ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
//      if (constructor==null)
//        return
//          new ValidationResult(false,
//            string.Format(CultureInfo.CurrentCulture,
//              "'{0}' type must have public instance parameterless constructor in order to be used as converter.",
//              type.FullName));
//      // Check generic interface
//      Type genericInterface = typeof (IValueConverter<>);
//      Type constructedInterface = type.GetInterface(genericInterface.FullName);
//      if (constructedInterface==null)
//        return
//          new ValidationResult(false,
//            string.Format(CultureInfo.CurrentCulture,
//              "'{0}' type does not implement '{1}' interface.",
//              type.FullName,
//              genericInterface.FullName));
//      Type[] genericArguments = constructedInterface.GetGenericArguments();
//      if (genericArguments.Length != 1 || !genericArguments[0].IsAssignableFrom(field.ValueType)) {
//        return
//          new ValidationResult(false,
//            string.Format(CultureInfo.CurrentCulture,
//              "'{0}' type implements incorrect '{1}' interface.",
//              type.FullName,
//              genericInterface.FullName));
//      }
//      return successResult;
//    }
//
//    public static ValidationResult ValidateConstraint(FieldInfo field, IFieldConstraint fieldConstraint)
//    {
//      Type valueType = fieldConstraint.AfterConverting ? field.MappingType : field.ValueType;
//
//      if (!fieldConstraint.IsApplicableFor(valueType))
//        return new ValidationResult(false,
//          string.Format("Constraint '{0}' does not support value type '{1}'.", fieldConstraint.GetType().Name, valueType.Name));
//
//      return successResult;
//    }

    public static ValidationResult ValidateBuilder(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ConstructorInfo constructor =
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
      if (constructor==null)
        return
          new ValidationResult(false,
            string.Format(CultureInfo.CurrentCulture,
              "Type '{0}' must have public instance parameterless constructor in order to be used as storage definition builder.",
              type.FullName));
      if (!typeof (IDomainBuilder).IsAssignableFrom(type))
        return
          new ValidationResult(false,
            string.Format(CultureInfo.CurrentCulture,
              "Type '{0}' does not implement '{1}' interface.",
              type.FullName,
              typeof (IDomainBuilder).FullName));
      return successResult;
    }

    public static ValidationResult ValidateType(Type type, ValidationRule rule)
    {
      if (type==null)
        return new ValidationResult(false, Strings.ExTypeCantBeNull);

      switch (rule) {
      case ValidationRule.Type:
        if (type.IsClass && type.IsSubclassOf(persistentType))
          return successResult;
        if (type.IsInterface && typeof (IEntity).IsAssignableFrom(type))
          return successResult;
        break;
      case ValidationRule.Column:
        if (type.IsPrimitive || type.IsEnum || type==stringType)
          return successResult;
        if (type.IsGenericType && type.GetGenericTypeDefinition()==entitySetType)
          return successResult;
        return ValidateType(type, ValidationRule.Type);
      }
      return new ValidationResult(false, string.Format(CultureInfo.CurrentCulture, "Unsupported type '{0}'", type));
    }

    public static ValidationResult ValidateCollatable(Type type)
    {
      if (type==typeof (string) || type==typeof (char))
        return successResult;
      return failureResult;
    }

    public static ValidationResult ValidateStreamType(Type type)
    {
      if (type==typeof (string) || type==typeof (byte[]))
        return successResult;
      return failureResult;
    }

    public static ValidationResult ValidateLength(int length)
    {
      return length <= 0 ? failureResult : successResult;
    }

    public static ValidationResult ValidateFillFactor(double fillFactor)
    {
      return
        fillFactor >= 0 && fillFactor <= 1
          ? successResult
          : new ValidationResult(false,
            string.Format(CultureInfo.CurrentCulture,
              "Invalid fill factor '{0}'. Value must be between 0 and 1.",
              fillFactor));
    }

    public static ValidationResult ValidateAttribute(TypeDef typeDef, MappingAttribute attribute)
    {
      ArgumentValidator.EnsureArgumentNotNull(attribute, "attribute");
      bool isRootEntity = attribute is EntityAttribute;
      if (typeDef.IsStructure && isRootEntity)
        return
          new ValidationResult(false,
            string.Format(CultureInfo.CurrentCulture,
              "'{0}' is not applicable to '{1}' descendants.",
              attribute.GetType().Name,
              typeof (Structure).Name));
      return successResult;
    }

    public static ValidationResult ValidateKeyProvider(Type type)
    {
      ConstructorInfo constructor =
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
      if (constructor==null)
        return
          new ValidationResult(false,
            string.Format(CultureInfo.CurrentCulture,
              "'{0}' type must have public instance parameterless constructor in order to be used as key provider.",
              type.FullName));
      else {
        if (!typeof (IKeyProvider).IsAssignableFrom(type))
          return
            new ValidationResult(false,
              string.Format(CultureInfo.CurrentCulture,
                "Type '{0}' does not implement '{1}' interface.",
                type.FullName,
                typeof (IKeyProvider).FullName));
      }
      return successResult;
    }

    public static ValidationResult ValidateHierarchy(HierarchyDef hierarchy)
    {
      if (hierarchy.KeyFields.Count==0)
        return new ValidationResult(false,
          string.Format(CultureInfo.CurrentCulture, "Hierarchy '{0}' must contain at least one key field.", hierarchy.Root.Name));
      return successResult;
    }

    public static ValidationResult ValidateLazyLoad(FieldInfo fieldInfo)
    {
      if (fieldInfo.IsPrimaryKey && fieldInfo.LazyLoad) {
        return new ValidationResult(false,
          string.Format(CultureInfo.CurrentCulture, "Field '{0}' can't be load on demand as it is included in primary key.", fieldInfo.Name));
      }
      return successResult;
    }

    public static ValidationResult ValidateHierarchyRoot(TypeInfo typeInfo)
    {
      FieldInfo fieldInfo;
      if (!typeInfo.Fields.TryGetValue(BuildingScope.Context.NameProvider.TypeId, out fieldInfo))
        return new ValidationResult(false, String.Format(CultureInfo.CurrentCulture, "Type '{0}' does not contain 'TypeId' field.", typeInfo.Name));
      if (fieldInfo.ValueType != typeof(int))
        return new ValidationResult(false, "Value type mismatch for field 'TypeId'.");
      return successResult;
    }

    static Validator()
    {
      Regex nameRe = new Regex(@"^[A-z][A-z0-9\-\._]*$", RegexOptions.Compiled);
      regexps.Add(ValidationRule.Type, nameRe);
      regexps.Add(ValidationRule.Field, new Regex(@"^[A-z][A-z0-9\-_]*$", RegexOptions.Compiled));
      regexps.Add(ValidationRule.Column, nameRe);
      regexps.Add(ValidationRule.Index, nameRe);
    }
  }
}
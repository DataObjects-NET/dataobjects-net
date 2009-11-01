// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AttributeProcessor
  {
    private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

    public static void Process(TypeDef type, EntityAttribute attribute)
    {
      ProcessMappingName(type, attribute, ValidationRule.Type);
    }

    public static void Process(TypeDef type, MaterializedViewAttribute attribute)
    {
      ProcessMappingName(type, attribute, ValidationRule.Type);
      type.Attributes |= TypeAttributes.Materialized;
    }

    public static void Process(HierarchyDef hierarchy, HierarchyRootAttribute attribute)
    {
      ProcessKeyProvider(hierarchy, attribute);
      ProcessKeyFields(hierarchy, attribute);
      ProcessInheritanceSchema(hierarchy, attribute);
    }

    public static void Process(FieldDef field, FieldAttribute attribute)
    {
      ProcessMappingName(field, attribute, ValidationRule.Field);
      ProcessIsNullable(field, attribute);
      ProcessIsTranslatable(field, attribute);
      ProcessIsCollatable(field, attribute);
      ProcessLength(field, attribute);
      ProcessLazyLoad(field, attribute);
      ProcessOnDelete(field, attribute);
      ProcessPairTo(field, attribute);
    }

    public static void Process(IndexDef index, IndexAttribute attribute)
    {
      ProcessMappingName(index, attribute, ValidationRule.Index);
      ProcessKeyFields(attribute.KeyFields, index.KeyFields);
      ProcessIncludedFields(attribute.IncludedFields, index.IncludedFields);
      ProcessFillFactor(index, attribute);
      ProcessIsUnique(index, attribute);
    }
//
//    public static void Process(FieldDef field, ConstraintAttribute attribute)
//    {
//      field.Constraints.Add(attribute.CreateConstraint(field.ValueType));
//    }

    public static void ProcessPairTo(FieldDef field, FieldAttribute attribute)
    {
      if (field.IsPrimitive || field.IsStructure) {
        if (!string.IsNullOrEmpty(attribute.PairTo))
          Log.Error(string.Format("Invalid usage of 'PairTo' attribute."));
      }
      else
        field.PairTo = attribute.PairTo;
    }

    private static void ProcessKeyProvider(HierarchyDef hierarchy, HierarchyRootAttribute attribute)
    {
      hierarchy.KeyProvider = attribute.KeyProvider;
    }

    private static void ProcessKeyFields(HierarchyDef hierarchy, HierarchyRootAttribute attribute)
    {
      KeyProviderAttribute ks =
        (KeyProviderAttribute)
          Attribute.GetCustomAttribute(hierarchy.KeyProvider, typeof (KeyProviderAttribute), true);
      if (ks==null) {
        Log.Error(string.Format("Key provider '{0}' should define at least one key field.", attribute.KeyProvider));
        return;
      }
      if (attribute.KeyFields.Length!=ks.Fields.Length) {
        Log.Error(
          string.Format("Key provider '{0}' and hierarchy {1} key field amount mismatch.",
            attribute.KeyProvider,
            hierarchy.Root.Name));
        return;
      }
      for (int index = 0; index < attribute.KeyFields.Length; index++) {
        Pair<string, Direction> result = ParseFieldName(attribute.KeyFields[index]);
        KeyField field = new KeyField(result.First, ks.Fields[index]);

        if (!Validator.ValidateName(result.First, ValidationRule.Field).Success)
          Log.Error(Strings.ExIndexFieldValidationError, attribute.KeyFields[index]);
        else if (hierarchy.KeyFields.ContainsKey(field))
          Log.Error(Strings.ExIndexAlreadyContainsField, attribute.KeyFields[index]);
        else
          hierarchy.KeyFields.Add(field, result.Second);
      }
    }

    private static void ProcessInheritanceSchema(HierarchyDef hierarchy, HierarchyRootAttribute attribute)
    {
      hierarchy.Schema = attribute.InheritanceSchema;
    }

    private static void ProcessIsUnique(IndexDef index, IndexAttribute attribute)
    {
      if (attribute.isUnique.HasValue)
        index.IsUnique = attribute.IsUnique;
    }

    private static void ProcessLength(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.length!=null)
        if (!Validator.ValidateLength(attribute.Length).Success)
          Log.Error("Invalid 'Length' attribute");
        else
          field.Length = attribute.Length;
    }

    private static void ProcessOnDelete(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.referentialAction!=null) {
        if (!field.IsEntity)
          Log.Error("Invalid 'OnDelete' attribute usage. Field is not entity reference.");
        else
          field.OnDelete = attribute.OnDelete;
      }
    }

    private static void ProcessIsCollatable(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.isCollatable!=null)
        field.IsCollatable = attribute.IsCollatable;
    }

    private static void ProcessIsTranslatable(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.isTranslatable!=null)
        field.IsTranslatable = attribute.IsTranslatable;
    }

    private static void ProcessIsNullable(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.isNullable!=null)
        if (field.UnderlyingProperty.PropertyType.IsGenericType &&
          field.UnderlyingProperty.PropertyType.GetGenericTypeDefinition()==typeof (Nullable<>))
          if (attribute.IsNullable)
            Log.Warning("Explicit 'IsNullable' attribute is redundant");
          else
            Log.Error(
              "Field has '{0}' type but is marked as not nullable.",
              field.UnderlyingProperty.PropertyType.Name);
        else
          field.IsNullable = attribute.IsNullable;
    }

    private static void ProcessLazyLoad(FieldDef field, FieldAttribute attribute)
    {
      field.LazyLoad = attribute.LazyLoad;
      if (!field.IsPrimitive && field.LazyLoad) {
        Log.Warning("Explicit 'LazyLoad' attribute is redundant");
      }
    }

    private static void ProcessMappingName(MappingNode node, MappingAttribute attribute, ValidationRule rule)
    {
      if (!String.IsNullOrEmpty(attribute.MappingName))
        if (Validator.ValidateName(attribute.MappingName, rule).Success) {
          if (comparer.Compare(node.MappingName, attribute.MappingName)==0)
            Log.Warning(
              "Explicit mapping name setting is redundant. The same name will be generated automatically.");
          else
            node.MappingName = attribute.MappingName;
        }
        else
          Log.Error("Invalid mapping name '{0}'.", attribute.MappingName);
    }

    private static void ProcessKeyFields(string[] source, IDictionary<string, Direction> target)
    {
      if (source==null || source.Length==0)
        Log.Error("Index must contain at least one field.");
      else
        for (int index = 0; index < source.Length; index++) {
          Pair<string, Direction> result = ParseFieldName(source[index]);
          if (!Validator.ValidateName(result.First, ValidationRule.Column).Success)
            Log.Error(Strings.ExIndexFieldValidationError, source[index]);
          else if (target.ContainsKey(result.First))
            Log.Error(Strings.ExIndexAlreadyContainsField, source[index]);
          else
            target.Add(result.First, result.Second);
        }
    }

    private static void ProcessIncludedFields(string[] source, IList<string> target)
    {
      if (source==null || source.Length==0)
        return;
      for (int index = 0; index < source.Length; index++) {
        string fieldName = source[index];

        if (!Validator.ValidateName(fieldName, ValidationRule.Column).Success)
          Log.Error(Strings.ExIndexFieldValidationError, source[index]);
        else if (target.Contains(fieldName))
          Log.Error(Strings.ExIndexAlreadyContainsField, source[index]);
        else
          target.Add(fieldName);
      }
    }    

    private static void ProcessFillFactor(IndexDef index, IndexAttribute attribute)
    {
      if (attribute.fillFactor.HasValue) {
        ValidationResult vr = Validator.ValidateFillFactor(attribute.FillFactor);
        if (!vr.Success)
          Log.Error(vr.Message);
        else
          index.FillFactor = attribute.FillFactor;
      }
    }

    private static Pair<string, Direction> ParseFieldName(string fieldName)
    {
      if (fieldName.EndsWith(":DESC", StringComparison.InvariantCultureIgnoreCase))
        return new Pair<string, Direction>(fieldName.Substring(0, fieldName.Length - 5), Direction.Negative);
      if (fieldName.EndsWith(":ASC", StringComparison.InvariantCultureIgnoreCase))
        fieldName = fieldName.Substring(0, fieldName.Length - 4);

      return new Pair<string, Direction>(fieldName, Direction.Positive);
    }
  }
}
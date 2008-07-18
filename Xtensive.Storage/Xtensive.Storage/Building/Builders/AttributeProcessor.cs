// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
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
        if (!attribute.PairTo.IsNullOrEmpty())
          throw new DomainBuilderException(
            string.Format(Strings.ExPairToAttributeCanNotBeUsedWithXField, field.Name));
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

      if (ks==null)
        throw new DomainBuilderException(
          string.Format(Strings.ExKeyProviderXShouldDefineAtLeastOneKeyField, attribute.KeyProvider));

      if (attribute.KeyFields.Length!=ks.Fields.Length)
        throw new DomainBuilderException(
          string.Format(Strings.ExKeyProviderXAndHierarchyYKeyFieldAmountMismatch, 
          attribute.KeyProvider, hierarchy.Root.Name));

      for (int index = 0; index < attribute.KeyFields.Length; index++) {
        Pair<string, Direction> result = ParseFieldName(attribute.KeyFields[index]);
        KeyField field = new KeyField(result.First, ks.Fields[index]);

        if (!Validator.IsNameValid(result.First, ValidationRule.Field))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexFieldXIsIncorrect, attribute.KeyFields[index]));

        if (hierarchy.KeyFields.ContainsKey(field))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexAlreadyContainsField, attribute.KeyFields[index]));

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
      if (attribute.length==null)
        return;

      if (attribute.Length <= 0)
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidLengthAttributeOnXField, field.Name));

      field.Length = attribute.Length;
    }

    private static void ProcessOnDelete(FieldDef field, FieldAttribute attribute)
    {
      if (attribute.referentialAction==null)
        return;

      if (!field.IsEntity)
        throw new DomainBuilderException(
          string.Format(Strings.InvalidOnDeleteAttributeUsageOnFieldXFieldIsNotEntityReference, field.Name));

      field.OnDelete = attribute.OnDelete;
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
            Log.Warning(Strings.ExplicitIsNullableAttributeIsRedundant);
          else
            throw new DomainBuilderException(
              string.Format(
                Strings.ExFieldXHasYTypeButIsMarkedAsNotNullable,
                field.Name, field.UnderlyingProperty.PropertyType.Name));
        else
          field.IsNullable = attribute.IsNullable;
    }

    private static void ProcessLazyLoad(FieldDef field, FieldAttribute attribute)
    {
      field.LazyLoad = attribute.LazyLoad;
      if (!field.IsPrimitive && field.LazyLoad) {
        Log.Warning(Strings.ExplicitLazyLoadAttributeOnFieldXIsRedundant, field.Name);
      }
    }

    private static void ProcessMappingName(MappingNode node, MappingAttribute attribute, ValidationRule rule)
    {
      if (attribute.MappingName.IsNullOrEmpty())
        return;

      if (!Validator.IsNameValid(attribute.MappingName, rule))
        throw new DomainBuilderException(
          string.Format(Strings.InvalidMappingNameX, attribute.MappingName));

      if (comparer.Compare(node.MappingName, attribute.MappingName)==0)
        Log.Warning(
          Strings.ExplicitMappingNameSettingIsRedundantTheSameNameXWillBeGeneratedAutomatically, node.MappingName);
      else
        node.MappingName = attribute.MappingName;
    }

    private static void ProcessKeyFields(string[] source, IDictionary<string, Direction> target)
    {
      if (source==null || source.Length==0)
        throw new DomainBuilderException(
          string.Format(Strings.ExIndexMustContainAtLeastOneField));

      for (int index = 0; index < source.Length; index++) {
        Pair<string, Direction> result = ParseFieldName(source[index]);

        if (!Validator.IsNameValid(result.First, ValidationRule.Column))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexFieldXIsIncorrect, source[index]));

        if (target.ContainsKey(result.First))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexAlreadyContainsField, source[index]));

        target.Add(result.First, result.Second);
      }
    }

    private static void ProcessIncludedFields(string[] source, IList<string> target)
    {
      if (source==null || source.Length==0)
        return;

      for (int index = 0; index < source.Length; index++) {
        string fieldName = source[index];

        if (!Validator.IsNameValid(fieldName, ValidationRule.Column))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexFieldXIsIncorrect, source[index]));

        if (target.Contains(fieldName))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexAlreadyContainsField, source[index]));

        target.Add(fieldName);
      }
    }

    private static void ProcessFillFactor(IndexDef index, IndexAttribute attribute)
    {
      if (!attribute.fillFactor.HasValue)
        return;
      
      index.FillFactor = attribute.FillFactor;
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

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AttributeProcessor
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static void Process(TypeDef type, MappingAttribute attribute)
    {
      ProcessMappingName(type, attribute, ValidationRule.Type);
    }

    public static void Process(FieldDef field, MappingAttribute attribute)
    {
      ProcessMappingName(field, attribute, ValidationRule.Field);
    }

    public static void Process(TypeDef type, MaterializedViewAttribute attribute)
    {
      type.Attributes |= TypeAttributes.Materialized;
    }

    public static void Process(TypeDef type, SystemTypeAttribute attribute)
    {
        type.Attributes |= TypeAttributes.System;
        BuildingContext.Current.SystemTypeIds[type.UnderlyingType] = attribute.TypeId;
    }

    public static void Process(HierarchyDef hierarchyDef, HierarchyRootAttribute attribute)
    {
      hierarchyDef.Schema = attribute.InheritanceSchema;
      hierarchyDef.IncludeTypeId = attribute.IncludeTypeId;
    }

    public static void Process(HierarchyDef hierarchyDef, FieldDef fieldDef, KeyAttribute attribute)
    {
      ArgumentValidator.EnsureArgumentIsInRange(attribute.Position, 0, MagicNumberProvider.MaxKeyFieldCount-1, "attribute.Position");

      var keyField = new KeyField(fieldDef.Name, attribute.Direction);

      if (hierarchyDef.KeyFields.Count > attribute.Position) {
        var current = hierarchyDef.KeyFields[attribute.Position];
        if (current != null)
          throw new DomainBuilderException(string.Format("Key fields '{0}' & '{1}' have the same position: '{2}'.", current.Name, fieldDef.Name, attribute.Position));
        hierarchyDef.KeyFields[attribute.Position] = keyField;
      }
      else {
        // Adding null stubs for not yet processed key fields
        while (hierarchyDef.KeyFields.Count < attribute.Position)
          hierarchyDef.KeyFields.Add(null);

        // Finally adding target key field at the specified position
        hierarchyDef.KeyFields.Add(keyField);
      }
    }

    public static void Process(HierarchyDef hierarchy, KeyGeneratorAttribute attribute)
    {
      hierarchy.KeyGenerator = attribute.Type;
      if (attribute.cacheSize.HasValue)
        hierarchy.KeyGeneratorCacheSize = attribute.CacheSize;
    }

    public static void Process(FieldDef fieldDef, FieldAttribute attribute)
    {
      ProcessLength(fieldDef, attribute);
      ProcessScale(fieldDef, attribute);
      ProcessPrecision(fieldDef, attribute);
      ProcessIsLazyLoad(fieldDef, attribute);
    }

    public static void Process(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (fieldDef.IsPrimitive || fieldDef.IsStructure) {
        if (!attribute.PairTo.IsNullOrEmpty())
          throw new DomainBuilderException(
            string.Format(Strings.ExAssociationAttributeCanNotBeAppliedToXField, fieldDef.Name));
      }
      ProcessPairTo(fieldDef, attribute);
      ProcessOnOwnerRemove(fieldDef, attribute);
      ProcessOnTargetRemove(fieldDef, attribute);
    }

    public static void Process(IndexDef indexDef, IndexAttribute attribute)
    {
      ProcessMappingName(indexDef, attribute.Name, ValidationRule.Index);
      ProcessKeyFields(attribute.KeyFields, indexDef.KeyFields);
      ProcessIncludedFields(attribute.IncludedFields, indexDef.IncludedFields);
      ProcessFillFactor(indexDef, attribute);
      ProcessIsUnique(indexDef, attribute);
    }

    public static void ProcessPairTo(FieldDef fieldDef, AssociationAttribute attribute)
    {
      fieldDef.PairTo = attribute.PairTo;
    }

    public static void ProcessOnOwnerRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onOwnerRemove==null)
        return;
      fieldDef.OnOwnerRemove = attribute.OnOwnerRemove;
    }

    public static void ProcessOnTargetRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onTargetRemove==null)
        return;
      fieldDef.OnTargetRemove = attribute.OnTargetRemove;
    }

    private static void ProcessIsUnique(IndexDef indexDef, IndexAttribute attribute)
    {
      if (attribute.isUnique.HasValue)
        indexDef.IsUnique = attribute.IsUnique;
    }

    private static void ProcessLength(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.length==null)
        return;

      if (attribute.Length <= 0)
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidLengthAttributeOnXField, fieldDef.Name));

      fieldDef.Length = attribute.Length;
    }

    private static void ProcessScale(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.scale==null)
        return;

      if (attribute.Scale <= 0)
        throw new DomainBuilderException(
          string.Format(Strings.InvalidScaleAttributeOnFieldX, fieldDef.Name));

      fieldDef.Scale = attribute.Scale;
    }

    private static void ProcessPrecision(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.precision==null)
        return;

      if (attribute.Precision <= 0)
        throw new DomainBuilderException(
          string.Format(Strings.InvalidPrecisionAttributeOnFieldX, fieldDef.Name));

      fieldDef.Precision = attribute.Precision;
    }

    private static void ProcessIsLazyLoad(FieldDef fieldDef, FieldAttribute attribute)
    {
      fieldDef.IsLazyLoad = attribute.LazyLoad;
      if (!fieldDef.IsPrimitive && fieldDef.IsLazyLoad) {
        Log.Warning(Strings.ExplicitLazyLoadAttributeOnFieldXIsRedundant, fieldDef.Name);
      }
    }

    private static void ProcessMappingName(MappingNode node, MappingAttribute attribute, ValidationRule rule)
    {
      ProcessMappingName(node, attribute.Name, rule);
    }

    private static void ProcessMappingName(MappingNode node, string mappingName, ValidationRule rule)
    {
      if (mappingName.IsNullOrEmpty())
        return;

      mappingName = BuildingContext.Current.NameBuilder.NamingConvention.Apply(mappingName);

      Validator.EnsureNameIsValid(mappingName, rule);

      if (Comparer.Compare(node.MappingName, mappingName)==0)
        Log.Warning(
          Strings.ExplicitMappingNameSettingIsRedundantTheSameNameXWillBeGeneratedAutomatically, node.MappingName);
      else
        node.MappingName = mappingName;
    }

    private static void ProcessKeyFields(string[] source, IDictionary<string, Direction> target)
    {
      if (source==null || source.Length==0)
        throw new DomainBuilderException(
          string.Format(Strings.ExIndexMustContainAtLeastOneField));

      for (int index = 0; index < source.Length; index++) {
        Pair<string, Direction> result = ParseFieldName(source[index]);

        Validator.EnsureNameIsValid(result.First, ValidationRule.Column);

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

        Validator.EnsureNameIsValid(fieldName, ValidationRule.Column);

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

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Upgrade;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AttributeProcessor
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static void Process(TypeDef type, TableMappingAttribute attribute)
    {
      ProcessMappingName(type, attribute.Name, ValidationRule.Type);
    }

    public static void Process(FieldDef field, FieldMappingAttribute attribute)
    {
      ProcessMappingName(field, attribute.Name, ValidationRule.Field);
    }

    public static void Process(TypeDef type, MaterializedViewAttribute attribute)
    {
      type.Attributes |= TypeAttributes.Materialized;
    }

    public static void Process(TypeDef type, SystemTypeAttribute attribute)
    {
        type.Attributes |= TypeAttributes.System;
        BuildingContext.Demand().SystemTypeIds[type.UnderlyingType] = attribute.TypeId;
    }

    public static void Process(HierarchyDef hierarchyDef, HierarchyRootAttribute attribute)
    {
      hierarchyDef.Schema = attribute.InheritanceSchema;
      hierarchyDef.IncludeTypeId = attribute.IncludeTypeId;
    }

    public static void Process(HierarchyDef hierarchyDef, FieldDef fieldDef, KeyAttribute attribute)
    {
      ArgumentValidator.EnsureArgumentIsInRange(attribute.Position, 0, WellKnown.MaxKeyFieldNumber-1, "attribute.Position");

      var keyField = new KeyField(fieldDef.Name, attribute.Direction);

      if (hierarchyDef.KeyFields.Count > attribute.Position) {
        var current = hierarchyDef.KeyFields[attribute.Position];
        if (current != null)
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldsXAndXHaveTheSamePositionX, current.Name, fieldDef.Name, attribute.Position));
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
      hierarchy.KeyGeneratorType = attribute.Type;
      if (!attribute.Name.IsNullOrEmpty())
        hierarchy.KeyGeneratorName = attribute.Name;
    }

    public static void Process(FieldDef fieldDef, FieldAttribute attribute)
    {
      ProcessDefault(fieldDef, attribute);
      ProcessNullable(fieldDef, attribute);
      ProcessLength(fieldDef, attribute);
      ProcessScale(fieldDef, attribute);
      ProcessPrecision(fieldDef, attribute);
      ProcessLazyLoad(fieldDef, attribute);
      ProcessIndexed(fieldDef, attribute);
    }

    public static void Process(FieldDef fieldDef, AssociationAttribute[] attributes)
    {
      var attribute = attributes[0];
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

    public static void Process(TypeDef typeDef, TypeDiscriminatorValueAttribute attribute)
    {
      typeDef.IsDefaultTypeInHierarchy = attribute.Default;
      if (!attribute.Default && attribute.Value == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeDiscriminatorValueIsRequiredUnlessXIsMarkedAsDefaultTypeInHierarchy, typeDef.Name));
      typeDef.TypeDiscriminatorValue = attribute.Value;
    }

    public static void Process(FieldDef fieldDef, VersionAttribute attribute)
    {
      FieldAttributes value;
      switch(attribute.Mode) {
        case VersionMode.Manual:
          value = FieldAttributes.ManualVersion;
          break;
        case VersionMode.Skip:
          value = FieldAttributes.SkipVersion;
          break;
        case VersionMode.Auto:
          value = FieldAttributes.AutoVersion;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      fieldDef.Attributes |= value;
    }

    public static void Process(FieldDef fieldDef, TypeDiscriminatorAttribute attribute)
    {
      if (!typeof(Entity).IsAssignableFrom(fieldDef.UnderlyingProperty.DeclaringType))
        throw new DomainBuilderException(
          string.Format(Strings.ExXFieldIsNotDeclaredInEntityDescendantSoCannotBeUsedAsTypeDiscriminator, fieldDef.Name));
      fieldDef.IsTypeDiscriminator = true;
    }

    private static void ProcessPairTo(FieldDef fieldDef, AssociationAttribute attribute)
    {
      fieldDef.PairTo = attribute.PairTo;
    }

    private static void ProcessOnOwnerRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onOwnerRemove==null)
        return;
      fieldDef.OnOwnerRemove = attribute.OnOwnerRemove;
    }

    private static void ProcessOnTargetRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onTargetRemove==null)
        return;
      fieldDef.OnTargetRemove = attribute.OnTargetRemove;
    }

    private static void ProcessIsUnique(IndexDef indexDef, IndexAttribute attribute)
    {
      if (attribute.unique.HasValue)
        indexDef.IsUnique = attribute.Unique;
    }

    private static void ProcessDefault(FieldDef fieldDef, FieldAttribute attribute)
    {
      object defaultValue = attribute.DefaultValue;
      if (defaultValue != null) {
        if (fieldDef.ValueType.IsAssignableFrom(defaultValue.GetType()))
          fieldDef.DefaultValue = defaultValue;
        else {
          var valueType = fieldDef.ValueType.StripNullable();
          var parseException =
            new DomainBuilderException(string.Format("Unable to parse default value {0} for field {1}", defaultValue,
                                                     fieldDef.Name));
          if (valueType == typeof (Guid)) {
            Guid guid;
            try {
              guid = Guid.Parse((string) defaultValue);
            }
            catch (FormatException) {
              throw parseException;
            }
            fieldDef.DefaultValue = guid;
          }
          else if (valueType == typeof (TimeSpan)) {
            TimeSpan timespan;
            if (defaultValue is string && !TimeSpan.TryParse((string) defaultValue, out timespan))
              throw parseException;
            else {
              long ticks = (long) Convert.ChangeType(defaultValue, typeof (long));
              timespan = TimeSpan.FromTicks(ticks);
            }
            fieldDef.DefaultValue = timespan;
          }
          else
            fieldDef.DefaultValue = Convert.ChangeType(defaultValue, valueType);
        }
      }
    }

    private static void ProcessNullable(FieldDef fieldDef, FieldAttribute attribute)
    {
      bool canUseNullableFlag = !fieldDef.ValueType.IsValueType && !fieldDef.IsStructure;
      if (attribute.nullable.HasValue) {
        if (canUseNullableFlag)
          fieldDef.IsNullable = attribute.nullable.Value;
        else if (attribute.nullable.Value!=(fieldDef.ValueType.IsNullable()))
          throw new DomainBuilderException(
            string.Format(Strings.ExNullableAndNullableOnUpgradeCannotBeUsedWithXField, fieldDef.Name));
      }

      // NullableOnUpgrade support
      if (!attribute.NullableOnUpgrade)
        return;
      if (!canUseNullableFlag)
        throw new DomainBuilderException(
          string.Format(Strings.ExNullableAndNullableOnUpgradeCannotBeUsedWithXField, fieldDef.Name));
      if (fieldDef.IsNullable)
        return;
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext==null)
        return;
      if (upgradeContext.Stage!=UpgradeStage.Upgrading)
        return;
      if (canUseNullableFlag)
        fieldDef.IsNullable = true;
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

      if (attribute.Scale < 0)
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidScaleAttributeOnFieldX, fieldDef.Name));

      fieldDef.Scale = attribute.Scale;
    }

    private static void ProcessPrecision(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.precision==null)
        return;

      if (attribute.Precision <= 0)
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidPrecisionAttributeOnFieldX, fieldDef.Name));

      fieldDef.Precision = attribute.Precision;
    }

    private static void ProcessLazyLoad(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (!fieldDef.IsPrimitive && attribute.LazyLoad)
        Log.Warning(
          Strings.LogExplicitLazyLoadAttributeOnFieldXIsRedundant, fieldDef.Name);
      else 
        fieldDef.IsLazyLoad = attribute.LazyLoad;
    }

    private static void ProcessIndexed(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (!attribute.Indexed)
        return;
      if (fieldDef.IsEntitySet)
        throw new InvalidOperationException(string.Format("Unable to apply index to EntitySet field {0}.", fieldDef.Name));
      if (fieldDef.IsStructure)
        throw new InvalidOperationException(string.Format("Unable to apply index to Structure field {0}.", fieldDef.Name));

      if (!fieldDef.IsPrimitive)
        Log.Warning("Specifying index on field {0} is redundant.", fieldDef.Name);
      else
        fieldDef.IsIndexed = attribute.Indexed;
    }

    private static void ProcessMappingName(MappingNode node, string mappingName, ValidationRule rule)
    {
      if (mappingName.IsNullOrEmpty())
        return;

      mappingName = BuildingContext.Demand().NameBuilder.ApplyNamingRules(mappingName);

      Validator.ValidateName(mappingName, rule);

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

        Validator.ValidateName(result.First, ValidationRule.Column);

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

        Validator.ValidateName(fieldName, ValidationRule.Column);

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

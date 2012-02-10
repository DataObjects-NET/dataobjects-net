// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;
using TypeAttributes = Xtensive.Orm.Model.TypeAttributes;

namespace Xtensive.Orm.Building.Builders
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
      hierarchyDef.IsClustered = attribute.Clustered;
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

    public static void Process(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (fieldDef.IsPrimitive || fieldDef.IsStructure) {
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
      ProcessFilter(indexDef, attribute);
      ProcessClustered(indexDef, attribute);
    }

    private static void ProcessClustered(IndexDef indexDef, IndexAttribute attribute)
    {
      if (!attribute.Clustered)
        return;
      if (indexDef.Type.UnderlyingType.IsInterface)
        throw new DomainBuilderException(string.Format(Strings.ExClusteredIndexCanNotBeDeclaredInInterfaceX, indexDef.Type.UnderlyingType));
      indexDef.IsClustered = true;
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
      var reflectedType = fieldDef.UnderlyingProperty.ReflectedType;

      // Skip type discriminator declarations for interfaces
      // Those has no effect on interface itself,
      // but allow implementors to inherit discriminator declaration.
      if (reflectedType.IsInterface)
        return;

      if (!typeof(Entity).IsAssignableFrom(reflectedType))
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

    private static void ProcessFilter(IndexDef indexDef, IndexAttribute attribute)
    {
      if (string.IsNullOrEmpty(attribute.Filter))
        return;
      var declaringType = indexDef.Type.UnderlyingType;
      var filterType = attribute.FilterType ?? declaringType;
      indexDef.FilterExpression = GetExpressionFromProvider(filterType, attribute.Filter, declaringType, typeof (bool));
      if (indexDef.MappingName == null) {
        var nameBuilder = BuildingContext.Demand().NameBuilder;
        var name = nameBuilder.BuildPartialIndexName(indexDef, filterType, attribute.Filter);
        ProcessMappingName(indexDef, name, ValidationRule.Index);
      }
    }

    private static void ProcessDefault(FieldDef fieldDef, FieldAttribute attribute)
    {
      object defaultValue = attribute.DefaultValue;
      if (defaultValue!=null)
        fieldDef.DefaultValue = ValueTypeBuilder.AdjustValue(fieldDef, defaultValue);
    }

    private static void ProcessNullable(FieldDef fieldDef, FieldAttribute attribute)
    {
      bool canUseNullableFlag = !fieldDef.ValueType.IsValueType && !fieldDef.IsStructure;

      if (attribute.nullable!=null) {
        if (canUseNullableFlag)
          fieldDef.IsNullable = attribute.nullable.Value;
        else if (attribute.nullable.Value!=(fieldDef.ValueType.IsNullable()))
          throw new DomainBuilderException(
            string.Format(Strings.ExNullableAndNullableOnUpgradeCannotBeUsedWithXField, fieldDef.Name));
      }

      if (attribute.nullableOnUpgrade != null) {
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
        fieldDef.IsNullable = true;
      }
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
      if (attribute.lazyLoad == null)
        return;

      if (!fieldDef.IsPrimitive && attribute.LazyLoad)
        Log.Warning(
          Strings.LogExplicitLazyLoadAttributeOnFieldXIsRedundant, fieldDef.Name);
      else 
        fieldDef.IsLazyLoad = attribute.LazyLoad;
    }

    private static void ProcessIndexed(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.indexed==null)
        return;

      if (fieldDef.IsEntitySet)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToSetIndexedFlagOnEntitySetFieldX, fieldDef.Name));
      if (fieldDef.IsStructure)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToSetIndexedFlagOnStructureFieldX, fieldDef.Name));

      if (attribute.indexed==true)
        fieldDef.IsIndexed = true;
      else
        fieldDef.IsNotIndexed = true;
    }

    private static void ProcessMappingName(MappedNode node, string mappingName, ValidationRule rule)
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

    private static LambdaExpression GetExpressionFromProvider(Type providerType, string providerMember, Type parameterType, Type returnType)
    {
      const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
      const string memberNameFormat = "{0}.{1}";

      MethodInfo method = null;
      var memberName = string.Format(memberNameFormat, providerType.FullName, providerMember);

      // Check for property
      var property = providerType.GetProperty(providerMember, bindingFlags);
      if (property!=null)
        method = property.GetGetMethod() ?? property.GetGetMethod(true);

      // Check for method
      if (method==null)
        method = providerType.GetMethod(providerMember, bindingFlags, null, Type.EmptyTypes, null);

      if (method==null)
        throw new DomainBuilderException(string.Format(Strings.ExMemberXIsNotFoundCheckThatSuchMemberExists, memberName));
      if (!typeof (LambdaExpression).IsAssignableFrom(method.ReturnType))
        throw new DomainBuilderException(string.Format(Strings.ExMemberXShouldReturnValueThatIsAssignableToLambdaExpression, memberName));
      var expression = (LambdaExpression) method.Invoke(null, new object[0]);
      if (expression.Parameters.Count!=1 || !expression.Parameters[0].Type.IsAssignableFrom(parameterType))
        throw new DomainBuilderException(string.Format(Strings.ExLambdaExpressionReturnedByXShouldTakeOneParameterOfTypeYOrAnyBaseTypeOfIt, memberName, parameterType.FullName));
      if (!returnType.IsAssignableFrom(expression.GetReturnType()))
        throw new DomainBuilderException(string.Format(Strings.ExLambdaExpressionReturnedByXShouldReturnValueThatIsAssignableToY, memberName, returnType.FullName));

      return expression;
    }
  }
}

// Copyright (C) 2007-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;
using TypeAttributes = Xtensive.Orm.Model.TypeAttributes;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;


namespace Xtensive.Orm.Building.Builders
{
  internal sealed class AttributeProcessor
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly BuildingContext context;

    public void Process(TypeDef type, TableMappingAttribute attribute) =>
      ProcessMappingName(type, attribute.Name, ValidationRule.Type);

    public void Process(FieldDef field, FieldMappingAttribute attribute) =>
      ProcessMappingName(field, attribute.Name, ValidationRule.Field);

    public void Process(TypeDef type, MaterializedViewAttribute attribute) =>
      type.Attributes |= TypeAttributes.Materialized;

    public void Process(TypeDef type, SystemTypeAttribute attribute)
    {
      type.Attributes |= TypeAttributes.System;
      if (attribute.TypeId != TypeInfo.NoTypeId) {
        type.StaticTypeId = attribute.TypeId;
      }
    }

    public void Process(HierarchyDef hierarchyDef, HierarchyRootAttribute attribute)
    {
      hierarchyDef.Schema = attribute.InheritanceSchema;
      hierarchyDef.IncludeTypeId = attribute.IncludeTypeId;
      hierarchyDef.IsClustered = attribute.Clustered;
    }

    public void Process(HierarchyDef hierarchyDef, FieldDef fieldDef, KeyAttribute attribute)
    {
      ArgumentValidator.EnsureArgumentIsInRange(attribute.Position, 0, WellKnown.MaxKeyFieldNumber - 1,
        "attribute.Position");

      var keyField = new KeyField(fieldDef.Name, attribute.Direction);

      if (hierarchyDef.KeyFields.Count > attribute.Position) {
        var current = hierarchyDef.KeyFields[attribute.Position];
        if (current != null) {
          throw new DomainBuilderException(string.Format(Strings.ExKeyFieldsXAndXHaveTheSamePositionX, current.Name,
            fieldDef.Name, attribute.Position));
        }

        hierarchyDef.KeyFields[attribute.Position] = keyField;
      }
      else {
        // Adding null stubs for not yet processed key fields
        while (hierarchyDef.KeyFields.Count < attribute.Position) {
          hierarchyDef.KeyFields.Add(null);
        }

        // Finally adding target key field at the specified position
        hierarchyDef.KeyFields.Add(keyField);
      }
    }

    public void Process(HierarchyDef hierarchy, KeyGeneratorAttribute attribute)
    {
      hierarchy.KeyGeneratorKind = attribute.Kind;

      if (!string.IsNullOrEmpty(attribute.Name)) {
        hierarchy.KeyGeneratorName = attribute.Name;
      }
    }

    public void Process(FieldDef fieldDef, FieldAttribute attribute)
    {
      ProcessDefault(fieldDef, attribute);
      ProcessSqlDefault(fieldDef, attribute);
      ProcessNullable(fieldDef, attribute);
      ProcessLength(fieldDef, attribute);
      ProcessScale(fieldDef, attribute);
      ProcessPrecision(fieldDef, attribute);
      ProcessLazyLoad(fieldDef, attribute);
      ProcessIndexed(fieldDef, attribute);
    }

    public void Process(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (fieldDef.IsPrimitive || fieldDef.IsStructure) {
        throw new DomainBuilderException(
          string.Format(Strings.ExAssociationAttributeCanNotBeAppliedToXField, fieldDef.Name));
      }

      ProcessPairTo(fieldDef, attribute);
      ProcessOnOwnerRemove(fieldDef, attribute);
      ProcessOnTargetRemove(fieldDef, attribute);
    }

    public void Process(IndexDef indexDef, IndexAttribute attribute)
    {
      ProcessMappingName(indexDef, attribute.Name, ValidationRule.Index);
      ProcessKeyFields(attribute.KeyFields, indexDef.KeyFields);
      ProcessIncludedFields(attribute.IncludedFields, indexDef.IncludedFields);
      ProcessFillFactor(indexDef, attribute);
      ProcessIsUnique(indexDef, attribute);
      ProcessFilter(indexDef, attribute);
      ProcessClustered(indexDef, attribute);
    }

    private void ProcessClustered(IndexDef indexDef, IndexAttribute attribute)
    {
      if (!attribute.Clustered) {
        return;
      }

      if (indexDef.Type.UnderlyingType.IsInterface) {
        throw new DomainBuilderException(string.Format(Strings.ExClusteredIndexCanNotBeDeclaredInInterfaceX, indexDef.Type.UnderlyingType));
      }

      indexDef.IsClustered = true;
    }

    public void Process(TypeDef typeDef, TypeDiscriminatorValueAttribute attribute)
    {
      typeDef.IsDefaultTypeInHierarchy = attribute.Default;
      if (!attribute.Default && attribute.Value == null) {
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeDiscriminatorValueIsRequiredUnlessXIsMarkedAsDefaultTypeInHierarchy, typeDef.Name));
      }

      typeDef.TypeDiscriminatorValue = attribute.Value;
    }

    public void Process(FieldDef fieldDef, VersionAttribute attribute)
    {
      FieldAttributes value;
      switch (attribute.Mode) {
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

    public void Process(FieldDef fieldDef, TypeDiscriminatorAttribute attribute)
    {
      var reflectedType = fieldDef.UnderlyingProperty.ReflectedType;

      // Skip type discriminator declarations for interfaces
      // Those has no effect on interface itself,
      // but allow implementors to inherit discriminator declaration.
      if (reflectedType.IsInterface) {
        return;
      }

      if (!WellKnownOrmTypes.Entity.IsAssignableFrom(reflectedType)) {
        throw new DomainBuilderException(
          string.Format(Strings.ExXFieldIsNotDeclaredInEntityDescendantSoCannotBeUsedAsTypeDiscriminator, fieldDef.Name));
      }

      fieldDef.IsTypeDiscriminator = true;
    }

    private void ProcessPairTo(FieldDef fieldDef, AssociationAttribute attribute) =>
      fieldDef.PairTo = attribute.PairTo;

    private void ProcessOnOwnerRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onOwnerRemove == null) {
        return;
      }

      fieldDef.OnOwnerRemove = attribute.OnOwnerRemove;
    }

    private void ProcessOnTargetRemove(FieldDef fieldDef, AssociationAttribute attribute)
    {
      if (attribute.onTargetRemove == null) {
        return;
      }

      fieldDef.OnTargetRemove = attribute.OnTargetRemove;
    }

    private void ProcessIsUnique(IndexDef indexDef, IndexAttribute attribute)
    {
      if (attribute.unique.HasValue) {
        indexDef.IsUnique = attribute.Unique;
      }
    }

    private void ProcessFilter(IndexDef indexDef, IndexAttribute attribute)
    {
      if (string.IsNullOrEmpty(attribute.Filter)) {
        return;
      }

      var declaringType = indexDef.Type.UnderlyingType;
      var filterType = attribute.FilterType ?? declaringType;
      indexDef.FilterExpression =
        GetExpressionFromProvider(filterType, attribute.Filter, declaringType, WellKnownTypes.Bool);
      if (indexDef.MappingName == null) {
        var nameBuilder = context.NameBuilder;
        var name = nameBuilder.BuildPartialIndexName(indexDef, filterType, attribute.Filter);
        ProcessMappingName(indexDef, name, ValidationRule.Index);
      }
    }

    private void ProcessDefault(FieldDef fieldDef, FieldAttribute attribute)
    {
      var defaultValue = attribute.DefaultValue;
      if (defaultValue != null) {
        fieldDef.DefaultValue = ValueTypeBuilder.AdjustValue(fieldDef, defaultValue);
      }
    }

    private void ProcessNullable(FieldDef fieldDef, FieldAttribute attribute)
    {
      var canUseNullableFlag = !fieldDef.ValueType.IsValueType && !fieldDef.IsStructure;

      if (attribute.nullable != null) {
        if (canUseNullableFlag) {
          fieldDef.IsNullable = attribute.nullable.Value;
          fieldDef.IsDeclaredAsNullable = attribute.nullable.Value;
        }
        else if (attribute.nullable.Value != fieldDef.ValueType.IsNullable()) {
          throw new DomainBuilderException(
            string.Format(Strings.ExNullableAndNullableOnUpgradeCannotBeUsedWithXField, fieldDef.Name));
        }
      }

      if (attribute.nullableOnUpgrade != null) {
        if (!canUseNullableFlag) {
          throw new DomainBuilderException(
            string.Format(Strings.ExNullableAndNullableOnUpgradeCannotBeUsedWithXField, fieldDef.Name));
        }

        if (fieldDef.IsNullable) {
          return;
        }

        if (context.BuilderConfiguration.Stage == UpgradeStage.Upgrading) {
          fieldDef.IsNullable = attribute.nullableOnUpgrade == true;
        }
      }
    }

    private void ProcessLength(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.length == null) {
        return;
      }

      if (attribute.Length <= 0) {
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidLengthAttributeOnXField, fieldDef.Name));
      }

      fieldDef.Length = attribute.Length;
    }

    private void ProcessScale(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.scale == null) {
        return;
      }

      if (attribute.Scale < 0) {
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidScaleAttributeOnFieldX, fieldDef.Name));
      }

      fieldDef.Scale = attribute.Scale;
    }

    private void ProcessPrecision(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.precision == null) {
        return;
      }

      if (attribute.Precision <= 0) {
        throw new DomainBuilderException(
          string.Format(Strings.ExInvalidPrecisionAttributeOnFieldX, fieldDef.Name));
      }

      fieldDef.Precision = attribute.Precision;
    }

    private void ProcessLazyLoad(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.lazyLoad == null) {
        return;
      }

      if (!fieldDef.IsPrimitive && attribute.LazyLoad) {
        BuildLog.Warning(
          Strings.LogExplicitLazyLoadAttributeOnFieldXIsRedundant, fieldDef.Name);
      }
      else {
        fieldDef.IsLazyLoad = attribute.LazyLoad;
      }
    }

    private void ProcessIndexed(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (attribute.indexed == null) {
        return;
      }

      if (fieldDef.IsEntitySet) {
        throw new InvalidOperationException(string.Format(Strings.ExUnableToSetIndexedFlagOnEntitySetFieldX,
          fieldDef.Name));
      }

      if (fieldDef.IsStructure) {
        throw new InvalidOperationException(string.Format(Strings.ExUnableToSetIndexedFlagOnStructureFieldX,
          fieldDef.Name));
      }

      if (attribute.indexed == true) {
        fieldDef.IsIndexed = true;
      }
      else {
        fieldDef.IsNotIndexed = true;
      }
    }

    private void ProcessSqlDefault(FieldDef fieldDef, FieldAttribute attribute)
    {
      if (!string.IsNullOrEmpty(attribute.DefaultSqlExpression)) {
        fieldDef.DefaultSqlExpression = attribute.DefaultSqlExpression;
      }
    }

    private void ProcessMappingName(MappedNode node, string mappingName, ValidationRule rule)
    {
      if (mappingName.IsNullOrEmpty()) {
        return;
      }

      mappingName = context.NameBuilder.ApplyNamingRules(mappingName);

      context.Validator.ValidateName(mappingName, rule);

      if (Comparer.Equals(node.MappingName, mappingName)) {
        BuildLog.Warning(nameof(Strings.ExplicitMappingNameSettingIsRedundantTheSameNameXWillBeGeneratedAutomatically),
          node.MappingName);
      }
      else {
        node.MappingName = mappingName;
      }
    }

    private void ProcessKeyFields(string[] source, IDictionary<string, Direction> target)
    {
      if (source == null || source.Length == 0) {
        throw new DomainBuilderException(
          string.Format(Strings.ExIndexMustContainAtLeastOneField));
      }

      foreach (var fieldName in source) {
        var result = ParseFieldName(fieldName);

        context.Validator.ValidateName(result.First, ValidationRule.Column);

        if (!target.TryAdd(result.First, result.Second)) {
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexAlreadyContainsField, fieldName));
        }
      }
    }

    private void ProcessIncludedFields(string[] source, IList<string> target)
    {
      if (source == null || source.Length == 0) {
        return;
      }

      foreach (var fieldName in source) {

        context.Validator.ValidateName(fieldName, ValidationRule.Column);

        if (target.Contains(fieldName)) {
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexAlreadyContainsField, fieldName));
        }

        target.Add(fieldName);
      }
    }

    private void ProcessFillFactor(IndexDef index, IndexAttribute attribute)
    {
      if (!attribute.fillFactor.HasValue) {
        return;
      }

      index.FillFactor = attribute.FillFactor;
    }

    private Pair<string, Direction> ParseFieldName(string fieldName)
    {
      if (fieldName.EndsWith(":DESC", StringComparison.InvariantCultureIgnoreCase)) {
        return new Pair<string, Direction>(fieldName.Substring(0, fieldName.Length - 5), Direction.Negative);
      }

      if (fieldName.EndsWith(":ASC", StringComparison.InvariantCultureIgnoreCase)) {
        fieldName = fieldName.Substring(0, fieldName.Length - 4);
      }

      return new Pair<string, Direction>(fieldName, Direction.Positive);
    }

    private LambdaExpression GetExpressionFromProvider(Type providerType, string providerMember, Type parameterType,
      Type returnType)
    {
      const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
      const string memberNameFormat = "{0}.{1}";

      MethodInfo method = null;
      var memberName = string.Format(memberNameFormat, providerType.FullName, providerMember);

      // Check for property
      var property = providerType.GetProperty(providerMember, bindingFlags);
      if (property != null) {
        method = property.GetGetMethod() ?? property.GetGetMethod(true);
      }

      // Check for method
      if (method == null) {
        method = providerType.GetMethod(providerMember, bindingFlags, null, Type.EmptyTypes, null);
      }

      // Check for method in the BaseType
      while (method == null && providerType.BaseType != null) {
        providerType = providerType.BaseType;
        method = providerType.GetMethod(providerMember, bindingFlags, null, Type.EmptyTypes, null);
      }

      if (method == null) {
        throw new DomainBuilderException(
          string.Format(Strings.ExMemberXIsNotFoundCheckThatSuchMemberExists, memberName));
      }

      if (!typeof(LambdaExpression).IsAssignableFrom(method.ReturnType)) {
        throw new DomainBuilderException(
          string.Format(Strings.ExMemberXShouldReturnValueThatIsAssignableToLambdaExpression, memberName));
      }

      var expression = (LambdaExpression) method.Invoke(null, Array.Empty<object>());
      if (expression.Parameters.Count != 1 || !expression.Parameters[0].Type.IsAssignableFrom(parameterType)) {
        throw new DomainBuilderException(string.Format(
          Strings.ExLambdaExpressionReturnedByXShouldTakeOneParameterOfTypeYOrAnyBaseTypeOfIt, memberName,
          parameterType.FullName));
      }

      if (!returnType.IsAssignableFrom(expression.GetReturnType())) {
        throw new DomainBuilderException(string.Format(
          Strings.ExLambdaExpressionReturnedByXShouldReturnValueThatIsAssignableToY, memberName, returnType.FullName));
      }

      return expression;
    }

    // Constructors

    public AttributeProcessor(BuildingContext context)
    {
      this.context = context;
    }
  }
}

// Copyright (C) 2015-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.01.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Upgrade.Internals.Extensions;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class UpgradeHintsProcessor : IUpgradeHintsProcessor
  {
    private readonly NameBuilder nameBuilder;
    private readonly MappingResolver resolver;

    private readonly bool autoDetectTypesMovements;
    private readonly DomainModel domainModel;
    private readonly StoredDomainModel currentModel;
    private readonly StoredDomainModel extractedModel;
    private readonly StorageModel extractedStorageModel;
    private readonly NativeTypeClassifier<UpgradeHint> hints;

    private readonly Dictionary<string, StoredTypeInfo> currentTypes;
    private readonly Dictionary<string, StoredTypeInfo> extractedTypes;

    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> reverseFieldMapping;

    private HashSet<StoredTypeInfo> suspiciousTypes;
    private IReadOnlyList<StoredTypeInfo> currentNonConnectorTypes;
    private IReadOnlyList<StoredTypeInfo> extractedNonConnectorTypes;

    public UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints)
    {
      ArgumentValidator.EnsureArgumentNotNull(inputHints, "inputHints");

      ProcessGenericTypeHints(inputHints);
      ProcessTypeChanges();
      ProcessFieldChanges();
      ProcessConnectorTypes();
      ProcessFieldMovements();

      return new UpgradeHintsProcessingResult(
        hints, typeMapping, reverseTypeMapping, fieldMapping, reverseFieldMapping, currentTypes,
        suspiciousTypes, currentNonConnectorTypes, extractedNonConnectorTypes);
    }

    #region General steps

    private void ProcessGenericTypeHints(IEnumerable<UpgradeHint> inputHints)
    {
      hints.AddRange(RewriteGenericTypeHints(inputHints));
    }

    private void ProcessTypeChanges()
    {
      var renameTypeHints = hints.GetItems<RenameTypeHint>()
        .ToDictionary(h => h.OldType, StringComparer.Ordinal, hints.GetItemCount<RenameTypeHint>());
      var removeTypeHints = hints.GetItems<RemoveTypeHint>()
        .ToDictionary(h => h.Type, StringComparer.Ordinal, hints.GetItemCount<RenameTypeHint>());

      BuildTypeMapping(renameTypeHints, removeTypeHints);
    }

    private void ProcessFieldChanges()
    {
      var renameFieldHints = hints.GetItems<RenameFieldHint>().ToChainedBuffer();
      var changeFieldTypeHints = hints.GetItems<ChangeFieldTypeHint>().ToChainedBuffer();
      BuildFieldMapping(renameFieldHints, changeFieldTypeHints);
    }

    private void ProcessConnectorTypes()
    {
      BuildConnectorTypeMapping();
    }

    private void ProcessFieldMovements()
    {
      var moveFieldHints = hints.GetItems<MoveFieldHint>().ToChainedBuffer();
      hints.AddRange(RewriteMoveFieldHints(moveFieldHints));
      hints.AddRange(GenerateTypeIdFieldRemoveHintsForConcreteTable());
    }

    #endregion

    #region Process generic type hints stage
    /// <summary>
    /// Rewtires hints for generic types.
    /// </summary>
    private IEnumerable<UpgradeHint> RewriteGenericTypeHints(IEnumerable<UpgradeHint> hints)
    {
      // Prepare data
      GetGenericRenameHints(hints, out var renamedTypesLookup, out var renameGenericTypeHints, out var renameFieldHints);
      var rewrittenHints = new ChainedBuffer<UpgradeHint>();

      var genericTypeMapping = BuildGenericTypeMapping(renamedTypesLookup);
      BuildRenameHintsForGenericTypes(genericTypeMapping, rewrittenHints);
      BuildRenameFieldHintsForGenericTypes(genericTypeMapping, renameFieldHints, rewrittenHints);

      return hints
        .Except(renameGenericTypeHints.Cast<UpgradeHint>())
        .Except(renameFieldHints.Cast<UpgradeHint>())
        .Concat(rewrittenHints);
    }

    /// <summary>
    /// Builds generic types mapping.
    /// </summary>
    private List<(string, Type, List<Pair<string, Type>>)> BuildGenericTypeMapping(Dictionary<string, RenameTypeHint> renamedTypesLookup)
    {
      var oldGenericTypes = GetGenericTypes(extractedModel);
      var newGenericTypes = GetGenericTypes(domainModel);

      var genericTypeMapping = new List<(string, Type, List<Pair<string, Type>>)>();
      var newTypesLookup = newGenericTypes.GetClasses().ToDictionary(t => t.GetFullName());
      foreach (var oldGenericDefName in oldGenericTypes.GetClasses()) {
        var newGenericDefType = GetNewType(oldGenericDefName, newTypesLookup, renamedTypesLookup);
        if (newGenericDefType == null) {
          continue;
        }

        foreach (var pair in oldGenericTypes.GetItems(oldGenericDefName)) {
          var genericArgumentsMapping = new List<Pair<string, Type>>();
          foreach (var oldGenericArgumentType in pair.Second) {
            var newGenericArgumentType = GetNewType(oldGenericArgumentType, newTypesLookup, renamedTypesLookup);
            if (newGenericArgumentType == null) {
              break;
            }

            genericArgumentsMapping.Add(new Pair<string, Type>(oldGenericArgumentType, newGenericArgumentType));
          }
          if (genericArgumentsMapping.Count == pair.Second.Length) {
            genericTypeMapping.Add((oldGenericDefName, newGenericDefType, genericArgumentsMapping));
          }
        }
      }
      return genericTypeMapping;
    }

    /// <summary>
    /// Builds <see cref="RenameTypeHint"/> for generic types.
    /// </summary>
    private void BuildRenameHintsForGenericTypes(IList<(string, Type, List<Pair<string, Type>>)> genericTypeMapping, ICollection<UpgradeHint> rewrittenHints)
    {
      foreach (var triplet in genericTypeMapping) {
        var arrays = triplet.Item3.SelectToArrays(pair => pair.First, pair => pair.Second);
        var oldGenericArguments = arrays.First;
        var newGenericArguments = arrays.Second;

        var oldTypeFullName = GetGenericTypeFullName(triplet.Item1, oldGenericArguments);
        var newType = triplet.Item2.MakeGenericType(newGenericArguments);
        if (!oldTypeFullName.Equals(newType.GetFullName(), StringComparison.Ordinal)) {
          rewrittenHints.Add(new RenameTypeHint(oldTypeFullName, newType));
        }
      }
    }

    /// <summary>
    /// Builds <see cref="RenameFieldHint"/> for each of renamed field
    /// of generic type.
    /// </summary>
    private void BuildRenameFieldHintsForGenericTypes(
      IEnumerable<(string, Type, List<Pair<string, Type>>)> genericTypeMapping,
      IEnumerable<RenameFieldHint> renameFieldHints,
      ICollection<UpgradeHint> rewrittenHints)
    {
      var genericTypeDefLookup = (
        from triplet in genericTypeMapping
        group triplet by triplet.Item2.GetGenericTypeDefinition()
          into g
        select (Definition: g.Key, Instances: g.ToArray())
        ).ToDictionary(g => g.Definition);

      // Build rename generic type field hints
      foreach (var hint in renameFieldHints) {
        var newGenericDefType = hint.TargetType;
        if (genericTypeDefLookup.TryGetValue(newGenericDefType, out var instanceGroup)) {
          foreach (var triplet in instanceGroup.Instances) {
            var newGenericArguments = triplet.Item3.SelectToArray(pair => pair.Second);
            rewrittenHints.Add(new RenameFieldHint(newGenericDefType.MakeGenericType(newGenericArguments),
              hint.OldFieldName, hint.NewFieldName));
          }
        }
      }
    }
    #endregion

    #region Process type changes stage
    // Should be diffenent depending of autoDetectTypeMovements
    private void BuildTypeMapping(IReadOnlyDictionary<string, RenameTypeHint> renameLookup, IReadOnlyDictionary<string, RemoveTypeHint> removeLookup)
    {
      // Excluding EntitySetItem<TL,TR> descendants.
      // They're not interesting at all for us, since
      // these types aren't ever referenced.
      extractedNonConnectorTypes = GetNonConnectorTypes(extractedModel);
      currentNonConnectorTypes = GetNonConnectorTypes(currentModel);

      var newModelTypes = currentNonConnectorTypes.ToDictionary(type => type.UnderlyingType, currentModel.Types.Length);
      //var renameLookup = renameTypeHints.ToDictionary(hint => hint.OldType, renameTypeHints.Count);
      //var removeLookup = removeTypeHints.ToDictionary(hint => hint.Type, removeTypeHints.Count);

      // Types that are neither mapped nor removed.
      var suspects = new HashSet<StoredTypeInfo>();

      // Mapping types
      foreach (var oldType in extractedNonConnectorTypes) {
        var removeTypeHint = removeLookup.GetValueOrDefault(oldType.UnderlyingType);
        if (removeTypeHint != null) {
          continue;
        }
        var renameTypeHint = renameLookup.GetValueOrDefault(oldType.UnderlyingType);
        var newTypeName = renameTypeHint != null
          ? renameTypeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        var newType = newModelTypes.GetValueOrDefault(newTypeName);
        if (newType != null) {
          MapType(oldType, newType);
        }
        else {
          _ = suspects.Add(oldType);
        }
      }

      if (suspects.Count == 0 || !autoDetectTypesMovements) {
        suspiciousTypes = suspects;
        return;
      }

      // Now we'll lookup by using DO type name instead of CLR type name
      // By default DO type name is a CLR type name without namespace
      // however this could be adjusted by domain configuration.
      // If CLR name is changed but DO name is preserved we should
      // automatically process this type without extra hints.

      newModelTypes = newModelTypes.Values.ToDictionary(t => t.Name, StringComparer.Ordinal, newModelTypes.Values.Count);

      foreach (var oldType in suspects) {
        var newType = newModelTypes.GetValueOrDefault(oldType.Name);
        if (newType != null && !reverseTypeMapping.ContainsKey(newType)) {
          MapType(oldType, newType);
        }
        else {
          _ = suspiciousTypes.Add(oldType);
        }
      }
    }

    #endregion

    #region Process field changes stage

    private void BuildFieldMapping(ICollection<RenameFieldHint> renameFieldHints, ICollection<ChangeFieldTypeHint> changeFieldTypeHints)
    {
      foreach (var pair in typeMapping) {
        BuildFieldMapping(renameFieldHints, changeFieldTypeHints, pair.Key, pair.Value);
      }

      // Will be modified, so we need to copy sequence into independant collection
      foreach (var pair in fieldMapping.ToChainedBuffer()) {
        MapNestedFields(pair.Key, pair.Value);
      }
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, IEnumerable<ChangeFieldTypeHint> typeChanges,
      StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      var newFields = newType.Fields.ToDictionary(field => field.Name, StringComparer.Ordinal, newType.Fields.Length);
      foreach (var oldField in oldType.Fields) {
        var renameHint = renames.FirstOrDefault(hint => hint.OldFieldName.Equals(oldField.Name, StringComparison.Ordinal)
          && hint.TargetType.GetFullName().Equals(newType.UnderlyingType, StringComparison.Ordinal));
        var newFieldName = renameHint != null
          ? renameHint.NewFieldName
          : oldField.Name;

        //finding new field
        var newField = (renameHint != null)
          ? newFields.GetValueOrDefault(renameHint.NewFieldName)
          : CheckPropertyNameWasOverriden(oldField)
            ? newFields.GetValueOrDefault(oldField.Name) ?? newFields.GetValueOrDefault(oldField.PropertyName)
            : newFields.GetValueOrDefault(oldField.Name);

        if (newField == null) {
          continue;
        }

        if (oldField.IsStructure) {
          // If it is structure, we map it immediately
          MapField(oldField, newField);
          continue;
        }

        var typeChangeHint = typeChanges
          .FirstOrDefault(hint => hint.Type.GetFullName().Equals(newType.UnderlyingType, StringComparison.Ordinal)
            && hint.FieldName.Equals(newField.Name, StringComparison.Ordinal));
        if (typeChangeHint == null) {
          // Check & skip field if type is changed
          var newValueTypeName = newField.IsEntitySet
            ? newField.ItemType
            : newField.ValueType;
          var oldValueTypeName = oldField.IsEntitySet
            ? oldField.ItemType
            : oldField.ValueType;
          var newValueType = currentTypes.GetValueOrDefault(newValueTypeName);
          var oldValueType = extractedTypes.GetValueOrDefault(oldValueTypeName);
          if (newValueType != null && oldValueType != null) {
            // We deal with reference field
            var mappedOldValueType = typeMapping.GetValueOrDefault(oldValueType);
            if (mappedOldValueType == null) {
              // Mapped to nothing = removed
              continue;
            }
            if (mappedOldValueType != newValueType && !newValueType.AllDescendants.Contains(mappedOldValueType)) {
              // This isn't a Dog -> Animal type mapping
              continue;
            }
          }
          else {
            // We deal with regular field
            if (!oldValueTypeName.Equals(newValueTypeName, StringComparison.Ordinal)) {
              continue;
            }
          }
        }
        MapField(oldField, newField);
      }
    }

    #endregion

    #region Process connector types stage

    private void BuildConnectorTypeMapping()
    {
      var oldAssociations = extractedModel.Associations
        .Where(association => association.ConnectorType != null);

      foreach (var oldAssociation in oldAssociations) {
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType)) {
          continue;
        }

        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;

        var newReferencingType = typeMapping.GetValueOrDefault(oldReferencingType);
        if (newReferencingType == null) {
          continue;
        }

        var newReferencingField = fieldMapping.GetValueOrDefault(oldReferencingField);
        if (newReferencingField == null) {
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name.Equals(oldReferencingField.Name, StringComparison.Ordinal));
        }
        if (newReferencingField == null) {
          continue;
        }
        var newAssociation = currentModel.Associations
          .SingleOrDefault(association => association.ReferencingField == newReferencingField);
        if (newAssociation == null || newAssociation.ConnectorType == null) {
          continue;
        }

        MapType(oldAssociation.ConnectorType, newAssociation.ConnectorType);

        var oldMaster = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name.Equals(WellKnown.MasterFieldName, StringComparison.Ordinal));
        var newMaster = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name.Equals(WellKnown.MasterFieldName, StringComparison.Ordinal));
        var oldSlave = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name.Equals(WellKnown.SlaveFieldName, StringComparison.Ordinal));
        var newSlave = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name.Equals(WellKnown.SlaveFieldName, StringComparison.Ordinal));

        MapFieldRecursively(oldMaster, newMaster);
        MapFieldRecursively(oldSlave, newSlave);
      }
    }

    #endregion

    #region Process field movements stage

    private IEnumerable<UpgradeHint> RewriteMoveFieldHints(IEnumerable<MoveFieldHint> moveFieldHints)
    {
      foreach (var hint in moveFieldHints) {
        yield return new CopyFieldHint(hint.SourceType, hint.SourceField, hint.TargetType, hint.TargetField);
        yield return new RemoveFieldHint(hint.SourceType, hint.SourceField);
      }
    }

    private IEnumerable<UpgradeHint> GenerateTypeIdFieldRemoveHintsForConcreteTable()
    {
      // Removes TypeId field ( = column) from hierarchies with ConcreteTable inheritance mapping
      var result = new List<UpgradeHint>();
      var relevantTypes =
        from pair in typeMapping
        let sourceHierarchy = pair.Key.Hierarchy
        let targetHierarchy = pair.Value.Hierarchy
        where
          targetHierarchy != null && sourceHierarchy != null
          && targetHierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable
        select pair.Key;

      foreach (var type in relevantTypes) {
        var typeIdField = type.AllFields.SingleOrDefault(f => f.IsTypeId);
        if (typeIdField == null) // Table of old type may not contain TypeId
          continue;
        var targetType = typeMapping[type];
        var targetTypeIdField = targetType.AllFields.SingleOrDefault(f => f.IsTypeId);
        if (targetTypeIdField == null || targetTypeIdField.IsPrimaryKey) {
          continue;
        }
        if (!extractedStorageModel.Tables.TryGetValue(GetTableName(type), out var oldTable)
          || !((TableInfo)oldTable).Columns.Contains(typeIdField.MappingName)) {
          continue;
        }
        var hint = new RemoveFieldHint(targetType.UnderlyingType, targetTypeIdField.Name);

        // Generating affected columns list explicitly for a situation when "type" is renamed to "targetType"
        if (type != targetType) {
          hint.IsExplicit = true;
          hint.AffectedColumns = Array.AsReadOnly(new string[] {
            GetColumnPath(targetType, targetTypeIdField.MappingName)
          });
        }
        result.Add(hint);
      }
      return result;
    }

    #endregion

    private void GetGenericRenameHints(
      IEnumerable<UpgradeHint> hints,
      out Dictionary<string, RenameTypeHint> renameTypeHints,
      out ChainedBuffer<RenameTypeHint> renameGenericTypeHints,
      out ChainedBuffer<RenameFieldHint> renameFieldHints)
    {
      renameTypeHints = new Dictionary<string, RenameTypeHint>(StringComparer.Ordinal);
      renameGenericTypeHints = new ChainedBuffer<RenameTypeHint>();
      renameFieldHints = new ChainedBuffer<RenameFieldHint>();

      foreach (var upgradeHint in hints) {
        if (upgradeHint is RenameTypeHint renameTypeHint) {
          renameTypeHints.Add(renameTypeHint.OldType, renameTypeHint);
          if (renameTypeHint.NewType.IsGenericTypeDefinition) {
            renameGenericTypeHints.Add(renameTypeHint);
          }
        }
        if (upgradeHint is RenameFieldHint renameFieldHint && renameFieldHint.TargetType.IsGenericTypeDefinition) {
          renameFieldHints.Add(renameFieldHint);
        }
      }
    }

    #region Map methods

    private void MapType(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      if (typeMapping.TryGetValue(oldType, out var existingNewType)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateTypeXWithTypeYTypeXIsAlreadyMappedToTypeZ,
          oldType, newType, existingNewType));
      }
      typeMapping[oldType] = newType;
      reverseTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      if (fieldMapping.TryGetValue(oldField, out var existingNewField)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateFieldXWithFieldYFieldXIsAlreadyMappedToFieldZ,
          oldField, newField, existingNewField));
      }
      fieldMapping[oldField] = newField;
      reverseFieldMapping[newField] = oldField;
    }

    private void MapNestedFields(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      var oldNestedFields = oldField.Fields;
      if (oldNestedFields.Length == 0) {
        return;
      }

      var oldValueType = extractedModel.Types
        .Single(type => type.UnderlyingType.Equals(oldField.ValueType, StringComparison.Ordinal));
      foreach (var oldNestedField in oldNestedFields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = oldValueType.AllFields
          .Single(field => field.Name.Equals(oldNestedField.OriginalName, StringComparison.Ordinal));

        if (fieldMapping.TryGetValue(oldNestedFieldOrigin, out var newNestedFieldOrigin)) {
          var newNestedField = newField.Fields
            .Single(field => field.OriginalName.Equals(newNestedFieldOrigin.Name, StringComparison.Ordinal));
          MapFieldRecursively(oldNestedField, newNestedField);
        }
      }
    }

    private void MapFieldRecursively(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      MapField(oldField, newField);
      MapNestedFields(oldField, newField);
    }

    #endregion

    #region Helpers

    private static IReadOnlyList<StoredTypeInfo> GetNonConnectorTypes(StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type != null
        select type
        ).ToHashSet();
      return model.Types
        .Where(type => !connectorTypes.Contains(type))
        .ToArray(model.Types.Length - connectorTypes.Count);
    }

    public static ClassifiedCollection<string, Pair<string, string[]>> GetGenericTypes(StoredDomainModel model)
    {
      var genericTypes = new ClassifiedCollection<string, Pair<string, string[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        var typeDefinitionName = typeInfo.GenericTypeDefinition;
        genericTypes.Add(new Pair<string, string[]>(typeDefinitionName, typeInfo.GenericArguments));
      }
      return genericTypes;
    }

    public static ClassifiedCollection<Type, Pair<Type, Type[]>> GetGenericTypes(DomainModel model)
    {
      var genericTypes = new ClassifiedCollection<Type, Pair<Type, Type[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.UnderlyingType.IsGenericType)) {
        var typeDefinition = typeInfo.UnderlyingType.GetGenericTypeDefinition();
        genericTypes.Add(new Pair<Type, Type[]>(typeDefinition, typeInfo.UnderlyingType.GetGenericArguments()));
      }
      return genericTypes;
    }

    private string GetTableName(StoredTypeInfo type)
    {
      return resolver.GetNodeName(
        type.MappingDatabase, type.MappingSchema, type.MappingName);
    }

    private string GetColumnPath(StoredTypeInfo type, string columnName)
    {
      var nodeName = GetTableName(type);
      // Due to current implementation of domain model FieldInfo.MappingName is not correct,
      // it has naming rules unapplied, corresponding ColumnInfo.Name however is correct.
      // StoredFieldInfo.MappingName is taken directly from FieldInfo.MappingName and thus is incorrect too.
      // We need to apply naming rules here to make it work.
      var actualColumnName = nameBuilder.ApplyNamingRules(columnName);
      return $"Tables/{nodeName}/Columns/{actualColumnName}";
    }

    private static Type GetNewType(string oldTypeName, Dictionary<string, Type> newTypes, Dictionary<string, RenameTypeHint> hints)
    {
      return hints.TryGetValue(oldTypeName, out var hint)
        ? hint.NewType
        : (newTypes.TryGetValue(oldTypeName, out var newType) ? newType : null);
    }

    private static string GetGenericTypeFullName(string genericDefinitionTypeName, string[] genericArgumentNames)
    {
      return $"{genericDefinitionTypeName.Replace("<>", string.Empty)}<{genericArgumentNames.ToCommaDelimitedString()}>";
    }

    private static bool CheckPropertyNameWasOverriden(StoredFieldInfo fieldInfo)
    {
      // if there is no real property then there is nothing to put OverrideFieldNameAttribute on
      if (string.IsNullOrEmpty(fieldInfo.PropertyName)) {
        return false;
      }
      //seems to be it was OverrideFieldNameAttribute been applied;
      return StringComparer.Ordinal.Compare(fieldInfo.PropertyName, fieldInfo.OriginalName) != 0;
    }
    #endregion

    // Constructors

    public UpgradeHintsProcessor(
      HandlerAccessor handlers,
      MappingResolver resolver,
      StoredDomainModel currentDomainModel,
      StoredDomainModel extractedDomainModel,
      StorageModel extractedStorageModel,
      bool autoDetectTypesMovements)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, nameof(handlers));
      ArgumentValidator.EnsureArgumentNotNull(resolver, nameof(resolver));
      ArgumentValidator.EnsureArgumentNotNull(currentDomainModel, nameof(currentDomainModel));
      ArgumentValidator.EnsureArgumentNotNull(extractedDomainModel, nameof(extractedDomainModel));
      ArgumentValidator.EnsureArgumentNotNull(extractedStorageModel, nameof(extractedStorageModel));

      // since type mapping is intersection of current types and extracted types
      // it will be equal or less than min size of these two sets
      var typesCapacity = currentDomainModel.Types.Length > extractedDomainModel.Types.Length
        ? extractedDomainModel.Types.Length
        : currentDomainModel.Types.Length;

      // By setting capacity we eliminate resize work and memory fluctuations.
      // In the worst case, when almost all types don't intersect, we will have some waste of memory
      // but in real life this is very rare case.
      typeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>(typesCapacity);
      reverseTypeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>(typesCapacity);
      fieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      reverseFieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();

      this.resolver = resolver;
      nameBuilder = handlers.NameBuilder;
      domainModel = handlers.Domain.Model;

      this.extractedStorageModel = extractedStorageModel;

      currentModel = currentDomainModel;
      currentTypes = currentModel.Types.ToDictionary(t => t.UnderlyingType, StringComparer.Ordinal, currentModel.Types.Length);

      extractedModel = extractedDomainModel;
      extractedTypes = extractedModel.Types.ToDictionary(t => t.UnderlyingType, StringComparer.Ordinal, extractedModel.Types.Length);

      this.autoDetectTypesMovements = autoDetectTypesMovements;
      hints = new NativeTypeClassifier<UpgradeHint>(true);
      suspiciousTypes = new HashSet<StoredTypeInfo>();
    }
  }
}

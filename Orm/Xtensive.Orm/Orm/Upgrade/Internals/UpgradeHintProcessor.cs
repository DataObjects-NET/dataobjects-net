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

    public UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints)
    {
      ArgumentValidator.EnsureArgumentNotNull(inputHints, "inputHints");
      ProcessGenericTypeHints(inputHints);
      ProcessTypeChanges();
      ProcessFieldChanges();
      ProcessConnectorTypes();
      ProcessFieldMovements();

      return new UpgradeHintsProcessingResult(hints, typeMapping, reverseTypeMapping, fieldMapping, reverseFieldMapping, currentTypes);
    }

    #region General steps

    private void ProcessGenericTypeHints(IEnumerable<UpgradeHint> inputHints)
    {
      hints.AddRange(RewriteGenericTypeHints(inputHints));
    }

    private void ProcessTypeChanges()
    {
      var renameTypeHints = hints.GetItems<RenameTypeHint>().ToChainedBuffer();
      var removeTypeHints = hints.GetItems<RemoveTypeHint>().ToChainedBuffer();
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
      Dictionary<string, RenameTypeHint> renamedTypesLookup;
      ChainedBuffer<RenameTypeHint> renameGenericTypeHints;
      ChainedBuffer<RenameFieldHint> renameFieldHints;
      GetGenericRenameHints(hints, out renamedTypesLookup, out renameGenericTypeHints, out renameFieldHints);
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
    private List<Triplet<string, Type, List<Pair<string, Type>>>> BuildGenericTypeMapping(Dictionary<string, RenameTypeHint> renamedTypesLookup)
    {
      var oldGenericTypes = extractedModel.GetGenericTypes();
      var newGenericTypes = domainModel.GetGenericTypes();

      var genericTypeMapping = new List<Triplet<string, Type, List<Pair<string, Type>>>>();
      var newTypesLookup = newGenericTypes.GetClasses().ToDictionary(t => t.GetFullName());
      foreach (var oldGenericDefName in oldGenericTypes.GetClasses()) {
        var newGenericDefType = GetNewType(oldGenericDefName, newTypesLookup, renamedTypesLookup);
        if (newGenericDefType==null)
          continue;
        foreach (var pair in oldGenericTypes.GetItems(oldGenericDefName)) {
          var genericArgumentsMapping = new List<Pair<string, Type>>();
          foreach (var oldGenericArgumentType in pair.Second) {
            var newGenericArgumentType = GetNewType(oldGenericArgumentType, newTypesLookup, renamedTypesLookup);
            if (newGenericArgumentType == null)
              break;
            genericArgumentsMapping.Add(new Pair<string, Type>(oldGenericArgumentType, newGenericArgumentType));
          }
          if (genericArgumentsMapping.Count==pair.Second.Length)
            genericTypeMapping.Add(new Triplet<string, Type, List<Pair<string, Type>>>(
              oldGenericDefName, newGenericDefType, genericArgumentsMapping));
        }
      }
      return genericTypeMapping;
    }

    /// <summary>
    /// Builds <see cref="RenameTypeHint"/> for generic types.
    /// </summary>
    private void BuildRenameHintsForGenericTypes(IList<Triplet<string, Type, List<Pair<string, Type>>>> genericTypeMapping, ChainedBuffer<UpgradeHint> rewrittenHints)
    {
      foreach (var triplet in genericTypeMapping) {
        var oldGenericArguments = triplet.Third.Select(pair => pair.First).ToArray();
        var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
        var oldTypeFullName = GetGenericTypeFullName(triplet.First, oldGenericArguments);
        var newType = triplet.Second.MakeGenericType(newGenericArguments);
        if (oldTypeFullName!=newType.GetFullName())
          rewrittenHints.Add(new RenameTypeHint(oldTypeFullName, newType));
      }
    }

    /// <summary>
    /// Builds <see cref="RenameFieldHint"/> for each of renamed field
    /// of generic type.
    /// </summary>
    private void BuildRenameFieldHintsForGenericTypes(IEnumerable<Triplet<string, Type, List<Pair<string, Type>>>> genericTypeMapping, IEnumerable<RenameFieldHint> renameFieldHints, ChainedBuffer<UpgradeHint> rewrittenHints)
    {
      var genericTypeDefLookup = (
        from triplet in genericTypeMapping
        group triplet by triplet.Second.GetGenericTypeDefinition()
          into g
          select new { Definition = g.Key, Instances = g.ToArray() }
        ).ToDictionary(g => g.Definition);

      // Build rename generic type field hints
      foreach (var hint in renameFieldHints) {
        var newGenericDefType = hint.TargetType;
        var instanceGroup = genericTypeDefLookup.GetValueOrDefault(newGenericDefType);
        if (instanceGroup==null)
          continue;
        foreach (var triplet in instanceGroup.Instances) {
          var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
          rewrittenHints.Add(new RenameFieldHint(newGenericDefType.MakeGenericType(newGenericArguments),
            hint.OldFieldName, hint.NewFieldName));
        }
      }
    }
    #endregion

    #region Process type changes stage
    // Should be diffenent depending of autoDetectTypeMovements
    private void BuildTypeMapping(ICollection<RenameTypeHint> renameTypeHints, ICollection<RemoveTypeHint> removeTypeHints)
    {
      // Excluding EntitySetItem<TL,TR> descendants.
      // They're not interesting at all for us, since
      // these types aren't ever referenced.
      var oldModelTypes = extractedModel.GetNonConnectorTypes();

      var newConnectorTypes = currentModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();

      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType);

      var renameLookup = renameTypeHints.ToDictionary(hint => hint.OldType);
      var removeLookup = removeTypeHints.ToDictionary(hint => hint.Type);

      // Types that are neither mapped nor removed.
      var suspiciousTypes = new List<StoredTypeInfo>();

      // Mapping types
      foreach (var oldType in oldModelTypes) {
        var removeTypeHint = removeLookup.GetValueOrDefault(oldType.UnderlyingType);
        if (removeTypeHint!=null)
          continue;
        var renameTypeHint = renameLookup.GetValueOrDefault(oldType.UnderlyingType);
        var newTypeName = renameTypeHint!=null
          ? renameTypeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        var newType = newModelTypes.GetValueOrDefault(newTypeName);
        if (newType!=null)
          MapType(oldType, newType);
        else
          //todo:// movements detection should be implemented by other processor
          if (autoDetectTypesMovements)
            suspiciousTypes.Add(oldType);
      }

      if (suspiciousTypes.Count == 0)
        return;

      // Now we'll lookup by using DO type name instead of CLR type name
      // By default DO type name is a CLR type name without namespace
      // however this could be adjusted by domain configuration.
      // If CLR name is changed but DO name is preserved we should
      // automatically process this type without extra hints.

      newModelTypes = newModelTypes.Values.ToDictionary(t => t.Name);

      foreach (var oldType in suspiciousTypes) {
        var newType = newModelTypes.GetValueOrDefault(oldType.Name);
        if (newType != null && !reverseTypeMapping.ContainsKey(newType))
          MapType(oldType, newType);
      }
    }

    #endregion

    #region Process field changes stage

    private void BuildFieldMapping(ICollection<RenameFieldHint> renameFieldHints, ICollection<ChangeFieldTypeHint> changeFieldTypeHints)
    {
      foreach (var pair in typeMapping)
        BuildFieldMapping(renameFieldHints, changeFieldTypeHints, pair.Key, pair.Value);

      // Will be modified, so we need to copy sequence into independant collection
      foreach (var pair in fieldMapping.ToChainedBuffer())
        MapNestedFields(pair.Key, pair.Value);
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, IEnumerable<ChangeFieldTypeHint> typeChanges,
      StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      var newFields = newType.Fields.ToDictionary(field => field.Name);
      foreach (var oldField in oldType.Fields) {
        var renameHint = renames.FirstOrDefault(hint => hint.OldFieldName==oldField.Name && hint.TargetType.GetFullName()==newType.UnderlyingType);
        var newFieldName = renameHint!=null
          ? renameHint.NewFieldName
          : oldField.Name;

        var newField = newFields.GetValueOrDefault(newFieldName);
        if (newField==null)
          continue;

        if (oldField.IsStructure) {
          // If it is structure, we map it immediately
          MapField(oldField, newField);
          continue;
        }

        var typeChangeHint = typeChanges
          .FirstOrDefault(hint => hint.Type.GetFullName()==newType.UnderlyingType && hint.FieldName==newField.Name);
        if (typeChangeHint==null) {
          // Check & skip field if type is changed
          var newValueTypeName = newField.IsEntitySet
            ? newField.ItemType
            : newField.ValueType;
          var oldValueTypeName = oldField.IsEntitySet
            ? oldField.ItemType
            : oldField.ValueType;
          var newValueType = currentTypes.GetValueOrDefault(newValueTypeName);
          var oldValueType = extractedTypes.GetValueOrDefault(oldValueTypeName);
          if (newValueType!=null &&oldValueType!=null) {
            // We deal with reference field
            var mappedOldValueType = typeMapping.GetValueOrDefault(oldValueType);
            if (mappedOldValueType==null)
              // Mapped to nothing = removed
              continue;
            if (mappedOldValueType!=newValueType && !newValueType.AllDescendants.Contains(mappedOldValueType))
              // This isn't a Dog -> Animal type mapping
              continue;
          }
          else
            // We deal with regular field
            if (oldValueTypeName!=newValueTypeName)
              continue;
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
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType))
          continue;

        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;

        var newReferencingType = typeMapping.GetValueOrDefault(oldReferencingType);
        if (newReferencingType==null)
          continue;

        var newReferencingField = fieldMapping.GetValueOrDefault(oldReferencingField);
        if (newReferencingField==null)
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name==oldReferencingField.Name);
        if (newReferencingField==null)
          continue;

        var newAssociation = currentModel.Associations
          .SingleOrDefault(association => association.ReferencingField == newReferencingField);
        if (newAssociation==null || newAssociation.ConnectorType==null)
          continue;

        MapType(oldAssociation.ConnectorType, newAssociation.ConnectorType);

        var oldMaster = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.MasterFieldName);
        var newMaster = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.MasterFieldName);
        var oldSlave = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.SlaveFieldName);
        var newSlave = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.SlaveFieldName);

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
          targetHierarchy!=null && sourceHierarchy!=null
          && targetHierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable
        select pair.Key;

      foreach (var type in relevantTypes) {
        var typeIdField = type.AllFields.SingleOrDefault(f => f.IsTypeId);
        if (typeIdField==null) // Table of old type may not contain TypeId
          continue;
        var targetType = typeMapping[type];
        var targetTypeIdField = targetType.AllFields.SingleOrDefault(f => f.IsTypeId);
        if (targetTypeIdField==null)
          continue;
        if (targetTypeIdField.IsPrimaryKey)
          continue;
        if (!extractedStorageModel.Tables.Contains(GetTableName(type)))
          continue;
        if (!extractedStorageModel.Tables[GetTableName(type)].Columns.Contains(typeIdField.MappingName))
          continue;
        var hint = new RemoveFieldHint(targetType.UnderlyingType, targetTypeIdField.Name);

        // Generating affected columns list explicitly for a situation when "type" is renamed to "targetType"
        if (type != targetType)
        {
          hint.IsExplicit = true;
          hint.AffectedColumns = new ReadOnlyList<string>(new List<string> {
            GetColumnPath(targetType, targetTypeIdField.MappingName)
          });
        }
        result.Add(hint);
      }
      return result;
    }

    #endregion

    private void GetGenericRenameHints(IEnumerable<UpgradeHint> hints, out Dictionary<string, RenameTypeHint> renameTypeHints, out ChainedBuffer<RenameTypeHint> renameGenericTypeHints, out ChainedBuffer<RenameFieldHint> renameFieldHints)
    {
      renameTypeHints = new Dictionary<string, RenameTypeHint>();
      renameGenericTypeHints = new ChainedBuffer<RenameTypeHint>();
      renameFieldHints = new ChainedBuffer<RenameFieldHint>();

      foreach (var upgradeHint in hints) {
        var renameTypeHint = upgradeHint as RenameTypeHint;
        if (renameTypeHint!=null) {
          renameTypeHints.Add(renameTypeHint.OldType, renameTypeHint);
          if (renameTypeHint.NewType.IsGenericTypeDefinition)
            renameGenericTypeHints.Add(renameTypeHint);
        }
        var renameFieldHint = upgradeHint as RenameFieldHint;
        if (renameFieldHint!=null && renameFieldHint.TargetType.IsGenericTypeDefinition)
          renameFieldHints.Add(renameFieldHint);
      }
    }

    #region Map methods
    private void MapType(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      StoredTypeInfo existingNewType;
      if (typeMapping.TryGetValue(oldType, out existingNewType)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateTypeXWithTypeYTypeXIsAlreadyMappedToTypeZ,
          oldType, newType, existingNewType));
      }
      typeMapping[oldType] = newType;
      reverseTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      StoredFieldInfo existingNewField;
      if (fieldMapping.TryGetValue(oldField, out existingNewField)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateFieldXWithFieldYFieldXIsAlreadyMappedToFieldZ,
          oldField, newField, existingNewField));
      }
      fieldMapping[oldField] = newField;
      reverseFieldMapping[newField] = oldField;
    }

    private void MapNestedFields(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      var oldNestedFields = ((IEnumerable<StoredFieldInfo>)oldField.Fields).ToArray();
      if (oldNestedFields.Length==0)
        return;
      var oldValueType = extractedModel.Types
        .Single(type => type.UnderlyingType==oldField.ValueType);
      foreach (var oldNestedField in oldNestedFields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = oldValueType.AllFields
          .Single(field => field.Name==oldNestedFieldOriginalName);
        if (!fieldMapping.ContainsKey(oldNestedFieldOrigin))
          continue;
        var newNestedFieldOrigin = fieldMapping[oldNestedFieldOrigin];
        var newNestedField = newField.Fields
          .Single(field => field.OriginalName==newNestedFieldOrigin.Name);
        MapFieldRecursively(oldNestedField, newNestedField);
      }
    }

    private void MapFieldRecursively(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      MapField(oldField, newField);
      MapNestedFields(oldField, newField);
    }

    #endregion

    #region Helpers

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
      return string.Format("Tables/{0}/Columns/{1}", nodeName, actualColumnName);
    }

    private static Type GetNewType(string oldTypeName, Dictionary<string, Type> newTypes, Dictionary<string, RenameTypeHint> hints)
    {
      RenameTypeHint hint;
      Type newType;
      return hints.TryGetValue(oldTypeName, out hint)
        ? hint.NewType
        : (newTypes.TryGetValue(oldTypeName, out newType) ? newType : null);
    }

    private static string GetGenericTypeFullName(string genericDefinitionTypeName, string[] genericArgumentNames)
    {
      return string.Format("{0}<{1}>", genericDefinitionTypeName.Replace("<>", string.Empty),
        genericArgumentNames.ToCommaDelimitedString());
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
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(resolver, "resolver");
      ArgumentValidator.EnsureArgumentNotNull(currentDomainModel, "currentDomainModel");
      ArgumentValidator.EnsureArgumentNotNull(extractedDomainModel, "extractedDomainModel");
      ArgumentValidator.EnsureArgumentNotNull(extractedStorageModel, "extractedStorageModel");

      typeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      reverseTypeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      fieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      reverseFieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();

      this.resolver = resolver;
      nameBuilder = handlers.NameBuilder;
      domainModel = handlers.Domain.Model;

      this.extractedStorageModel = extractedStorageModel;

      currentModel = currentDomainModel;
      currentTypes = currentModel.Types.ToDictionary(t => t.UnderlyingType);

      extractedModel = extractedDomainModel;
      extractedTypes = extractedModel.Types.ToDictionary(t => t.UnderlyingType);

      this.autoDetectTypesMovements = autoDetectTypesMovements;
      hints = new NativeTypeClassifier<UpgradeHint>(true);
    }
  }
}

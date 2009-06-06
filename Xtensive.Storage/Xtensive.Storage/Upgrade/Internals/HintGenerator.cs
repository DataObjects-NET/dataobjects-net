// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Upgrade
{
  internal sealed class HintGenerator
  {
    private readonly StoredDomainModel storedModel;
    private readonly DomainModel currentModel;
 
    private readonly Dictionary<StoredTypeInfo, TypeInfo> typeMapping
      = new Dictionary<StoredTypeInfo, TypeInfo>();
    private readonly Dictionary<TypeInfo, StoredTypeInfo> backwardTypeMapping
      = new Dictionary<TypeInfo, StoredTypeInfo>();
    private readonly Dictionary<StoredFieldInfo, FieldInfo> fieldMapping
      = new Dictionary<StoredFieldInfo, FieldInfo>();
    private readonly List<Hint> resultHints = new List<Hint>();

    public IEnumerable<Hint> GenerateHints(IEnumerable<UpgradeHint> upgradeHints)
    {
      var typeRenames = upgradeHints.OfType<RenameTypeHint>().ToArray();
      var fieldRenames = upgradeHints.OfType<RenameFieldHint>().ToArray();
      var fieldCopyHints = upgradeHints.OfType<CopyFieldHint>().ToArray();

      ValidateRenameTypeHints(typeRenames);
      BuildTypeMapping(typeRenames);

      ValidateRenameFieldHints(fieldRenames);
      BuildFieldMapping(fieldRenames);
      BuildAssociationMapping();

      ValidateCopyFieldHints(fieldCopyHints);
      GenerateRenameTableHints();
      GenerateRenameColumnHints();
      GenerateCopyColumnHints(fieldCopyHints);
      return resultHints;
    }

    #region Validation

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        // checking that types exists in models
        if (!currentModel.Types.Contains(hint.NewType))
          throw TypeIsNotFound(hint.NewType.GetFullName());
        if (!storedModel.Types.Any(type => type.UnderlyingType==hint.OldType))
          throw TypeIsNotFound(hint.OldType);
        // each original type should be used only once
        // each result type should be used only once
        RenameTypeHint evilHint;
        if (sourceTypes.TryGetValue(hint.OldType, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetTypes.TryGetValue(hint.NewType, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceTypes.Add(hint.OldType, hint);
        targetTypes.Add(hint.NewType, hint);
      }
    }
    
    private void ValidateRenameFieldHints(IEnumerable<RenameFieldHint> hints)
    {
      var sourceFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      var targetFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      foreach (var hint in hints) {
        // checking that both target and source fields exists in models
        if (!currentModel.Types.Contains(hint.TargetType))
          throw TypeIsNotFound(hint.TargetType.GetFullName());
        var targetType = currentModel.Types[hint.TargetType];
        var sourceType = backwardTypeMapping[targetType];
        if (!GetDeclaredFields(sourceType).Any(field => field.Name==hint.OldFieldName))
          throw FieldIsNotFound(sourceType.UnderlyingType, hint.OldFieldName);
        if (!GetDeclaredFields(targetType).Any(field => field.Name==hint.NewFieldName))
          throw FieldIsNotFound(hint.TargetType.GetFullName(), hint.NewFieldName);
        // each source field should be used only once
        // each destination field should be used only once
        RenameFieldHint evilHint;
        var sourceField = new Pair<Type, string>(hint.TargetType, hint.OldFieldName);
        var targetField = new Pair<Type, string>(hint.TargetType, hint.NewFieldName);
        if (sourceFields.TryGetValue(sourceField, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetFields.TryGetValue(targetField, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceFields.Add(sourceField, hint);
        targetFields.Add(targetField, hint);
      }
    }

    private void ValidateCopyFieldHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints) {
        // checking source type/field
        var sourceTypeName = hint.SourceType;
        if (!storedModel.Types.Any(type => type.UnderlyingType==hint.SourceType))
          throw TypeIsNotFound(sourceTypeName);
        var sourceType = storedModel.Types.Single(type => type.UnderlyingType==hint.SourceType);
        if (!GetAllFields(sourceType).Any(field => field.Name==hint.SourceField))
          throw FieldIsNotFound(sourceTypeName, hint.SourceField);
        // checking destination type/field
        var targetTypeName = hint.TargetType.GetFullName();
        if (!currentModel.Types.Any(type => type.UnderlyingType==hint.TargetType))
          throw TypeIsNotFound(targetTypeName);
        var targetType = currentModel.Types[hint.TargetType];
        if (!GetAllFields(targetType).Any(field => field.Name==hint.TargetField))
          throw FieldIsNotFound(targetTypeName, hint.TargetField);
      }
    }

    #endregion

    #region Mapping

    private void MapType(StoredTypeInfo oldType, TypeInfo newType)
    {
      if (typeMapping.ContainsKey(oldType))
        throw new InvalidOperationException();
      typeMapping[oldType] = newType;
      backwardTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, FieldInfo newField)
    {
      if (fieldMapping.ContainsKey(oldField))
        throw new InvalidOperationException();
      fieldMapping[oldField] = newField;
    }

    private void MapNestedFields(StoredFieldInfo oldField, FieldInfo newField)
    {
      var oldNestedFields = GetNestedFields(oldField).ToArray();
      if (oldNestedFields.Length==0)
        return;
      var oldValueType = storedModel.Types
        .Single(type => type.UnderlyingType==oldField.ValueType);
      foreach (var oldNestedField in oldNestedFields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = GetAllFields(oldValueType)
          .Single(field => field.Name==oldNestedFieldOriginalName);
        var newNestedFieldOrigin = fieldMapping[oldNestedFieldOrigin];
        var newNestedField = GetNestedFields(newField)
          .Single(field => field.OriginalName==newNestedFieldOrigin.Name);
        MapFieldRecursively(oldNestedField, newNestedField);
      }
    }

    private void MapFieldRecursively(StoredFieldInfo oldField, FieldInfo newField)
    {
      MapField(oldField, newField);
      MapNestedFields(oldField, newField);
    }
    
    private void BuildTypeMapping(IEnumerable<RenameTypeHint> renames)
    {
      var oldConnectorTypes = storedModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();
      var newConnectorTypes = currentModel.Associations
        .Select(association => association.UnderlyingType)
        .Where(type => type!=null)
        .ToHashSet();

      var oldModelTypes = storedModel.Types
        .Where(type => !oldConnectorTypes.Contains(type));
      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType.GetFullName());

      foreach (var oldType in oldModelTypes) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldType==oldType.UnderlyingType);
        var newTypeName = maybeHint!=null
          ? maybeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        TypeInfo newType;
        if (newModelTypes.TryGetValue(newTypeName, out newType))
          MapType(oldType, newType);
      }
    }
    
    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames)
    {
      foreach (var pair in typeMapping)
        BuildFieldMapping(renames, pair.Key, pair.Value);

      foreach (var pair in fieldMapping.ToList())
        MapNestedFields(pair.Key, pair.Value);
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, StoredTypeInfo oldType, TypeInfo newType)
    {
      var newFields = GetDeclaredFields(newType).ToDictionary(field => field.Name);
      foreach (var oldField in GetDeclaredFields(oldType)) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldFieldName==oldField.Name && hint.TargetType==newType.UnderlyingType);
        var newFieldName = maybeHint!=null
          ? maybeHint.NewFieldName
          : oldField.Name;
        FieldInfo newField;
        if (newFields.TryGetValue(newFieldName, out newField))
          MapField(oldField, newField);
      }
    }

    private void BuildAssociationMapping()
    {
      var oldAssociations = storedModel.Associations
        .Where(association => association.ConnectorType!=null);
      foreach (var oldAssociation in oldAssociations) {
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType))
          continue;
        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;
        TypeInfo newReferencingType;
        if (!typeMapping.TryGetValue(oldReferencingType, out newReferencingType))
          continue;
        FieldInfo newReferencingField;
        if (!fieldMapping.TryGetValue(oldReferencingField, out newReferencingField))
          newReferencingField = GetDeclaredFields(newReferencingType)
            .SingleOrDefault(field => field.Name==oldReferencingField.Name);
        if (newReferencingField==null)
          continue;
        var newAssociation = newReferencingField.Association;
        MapType(oldAssociation.ConnectorType, newAssociation.UnderlyingType);

        var oldMaster = GetAllFields(oldAssociation.ConnectorType)
          .Single(field => field.Name==WellKnown.MasterField);
        var newMaster = GetAllFields(newAssociation.UnderlyingType)
          .Single(field => field.Name==WellKnown.MasterField);
        var oldSlave = GetAllFields(oldAssociation.ConnectorType)
          .Single(field => field.Name==WellKnown.SlaveField);
        var newSlave = GetAllFields(newAssociation.UnderlyingType)
          .Single(field => field.Name==WellKnown.SlaveField);

        MapFieldRecursively(oldMaster, newMaster);
        MapFieldRecursively(oldSlave, newSlave);
      }
    }

    #endregion

    #region Hint generation

    private void GenerateRenameTableHints()
    {
      var mappingsToProcess = typeMapping
        .Where(type => type.Key.IsEntity)
        .Where(type => type.Key.Hierarchy.Schema!=InheritanceSchema.SingleTable || type.Key.Hierarchy.Root==type.Key);
      foreach (var mapping in mappingsToProcess) {
        var oldTable = GetMappingName(mapping.Key);
        var newTable = GetMappingName(mapping.Value);
        if (oldTable != newTable)
          resultHints.Add(new RenameHint(GetTablePath(oldTable), GetTablePath(newTable)));
      }
    }

    private void GenerateRenameColumnHints()
    {
      var fieldsToProcess = fieldMapping
        .Where(m => m.Key.IsPrimitive && m.Key.DeclaringType.IsEntity);
      foreach (var mapping in fieldsToProcess) {
        var oldField = mapping.Key;
        var newField = mapping.Value;
        var newType = newField.DeclaringType;
        switch (newType.Hierarchy.Schema) {
        case InheritanceSchema.ClassTable:
          // rename column in inheritors only when field is a key
          GenerateRenameFieldHint(oldField, newField, newType, newField.IsPrimaryKey);
          break;
        case InheritanceSchema.SingleTable:
          // always rename only one column
          GenerateRenameFieldHint(oldField, newField, newType, false);
          break;
        case InheritanceSchema.ConcreteTable:
          // rename column in all inheritors and type itself
          GenerateRenameFieldHint(oldField, newField, newType, true);
          break;
        default:
          throw new ArgumentOutOfRangeException();
        }
      }
    }

    private void GenerateRenameFieldHint(StoredFieldInfo oldField, FieldInfo newField,
      TypeInfo newType, bool includeInheritors)
    {
      if (oldField.MappingName==newField.MappingName)
        return;
      foreach (var newTargetType in GenerateTargetTypes(newType, includeInheritors)) {
        StoredTypeInfo oldTargetType;
        if (!backwardTypeMapping.TryGetValue(newTargetType, out oldTargetType))
          continue;
        resultHints.Add(new RenameHint(
          GetColumnPath(GetMappingName(oldTargetType), oldField.MappingName),
          GetColumnPath(GetMappingName(newTargetType), newField.MappingName)));
      }
    }
    
    private void GenerateCopyColumnHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints)
        TranslateCopyColumnHint(hint);
    }

    private void TranslateCopyColumnHint(CopyFieldHint hint)
    {
      // searching for all required objects
      var targetTypeName = hint.TargetType.GetFullName();
      var targetType = currentModel.Types[hint.TargetType];
      var targetField = GetAllFields(targetType).Single(field => field.Name==hint.TargetField);
      var targetHierarchy = targetType.Hierarchy;

      var sourceTypeName = hint.SourceType;
      var sourceType = storedModel.Types.Single(type => type.UnderlyingType==sourceTypeName);
      var sourceField = GetAllFields(sourceType).Single(field => field.Name==hint.SourceField);
      var sourceHierarchy = sourceType.Hierarchy;
      
      // checking that types have hierarchies
      if (sourceHierarchy==null)
        throw TypeIsNotInHierarchy(hint.SourceType);
      if (targetHierarchy == null)
        throw TypeIsNotInHierarchy(targetTypeName);
      
      // building set of key columns
      var pairedKeyColumns = AssociateMappedKeyFields(sourceHierarchy, targetHierarchy);
      if (pairedKeyColumns==null)
        throw KeysDoNotMatch(sourceTypeName, targetTypeName);

      // building set of copied columns
      var pairedColumns = AssociateMappedFields(new Pair<StoredFieldInfo, FieldInfo>(sourceField, targetField));
      if (pairedColumns==null)
        throw FieldsDoNotMatch(sourceField, targetField);

      // building source/destination table/column names
      var sourceTable = GetMappingName(sourceType);
      var targetTables = GenerateTargetTypes(targetType, targetHierarchy.Schema==InheritanceSchema.ConcreteTable)
        .Select(type => GetMappingName(type));

      // generating result hints
      foreach (var targetTable in targetTables) {
        var resultHint = new CopyDataHint {SourceTablePath = GetTablePath(sourceTable)};
        foreach (var keyColumnPair in pairedKeyColumns)
          resultHint.Identities.Add(new IdentityPair(
            GetColumnPath(sourceTable, keyColumnPair.First),
            GetColumnPath(targetTable, keyColumnPair.Second),
            false));
        foreach (var columnPair in pairedColumns)
          resultHint.CopiedColumns.Add(new Pair<string>(
            GetColumnPath(sourceTable, columnPair.First),
            GetColumnPath(targetTable, columnPair.Second)));
        resultHints.Add(resultHint);
      }
    }

    private IEnumerable<TypeInfo> GenerateTargetTypes(TypeInfo type, bool includeInheritors)
    {
      var result = EnumerableUtils.One(type);
      if (includeInheritors)
        result = result.Concat(currentModel.Types.FindDescendants(type, true));
      return result;
    }

    #endregion


    // Constructors

    public HintGenerator(StoredDomainModel storedModel, DomainModel currentModel)
    {
      this.storedModel = storedModel;
      this.currentModel = currentModel;
    }
    
    #region Static helpers

    private static string GetTablePath(string name)
    {
      return string.Format("Tables/{0}", name);
    }

    private static string GetColumnPath(string tableName, string columnName)
    {
      return string.Format("Tables/{0}/Columns/{1}", tableName, columnName);
    }
    
    private static string GetMappingName(StoredTypeInfo type)
    {
      // for StoredTypeInfo MappingName is always correct
      return type.MappingName;
    }

    private static string GetMappingName(TypeInfo type)
    {
      // for TypeInfo MappingName is not correct for types beyond hierarchy root
      // when inheritance schema is SingleTable
      if (type.Hierarchy!=null && type.Hierarchy.Schema==InheritanceSchema.SingleTable)
        type = type.Hierarchy.Root;
      return type.MappingName;
    }

    private static IEnumerable<StoredFieldInfo> GetDeclaredFields(StoredTypeInfo type)
    {
      return type.Fields;
    }

    private static IEnumerable<FieldInfo> GetDeclaredFields(TypeInfo type)
    {
      return type.Fields.Where(field => field.IsDeclared && !field.IsNested);
    }

    private static IEnumerable<StoredFieldInfo> GetAllFields(StoredTypeInfo type)
    {
      return type.AllFields;
    }

    private static IEnumerable<FieldInfo> GetAllFields(TypeInfo type)
    {
      return type.Fields.Where(field => !field.IsNested);
    }

    private static IEnumerable<StoredFieldInfo> GetNestedFields(StoredFieldInfo parentField)
    {
      return parentField.Fields;
    }

    private static IEnumerable<FieldInfo> GetNestedFields(FieldInfo parentField)
    {
      return parentField.Fields.Where(field => field.Parent==parentField);
    }


    private static Pair<StoredFieldInfo, FieldInfo>[] JoinFieldsByName(
      IEnumerable<StoredFieldInfo> sources, IEnumerable<FieldInfo> targets)
    {
      var result =
        from source in sources
        join target in targets
          on source.Name equals target.Name
        select new Pair<StoredFieldInfo, FieldInfo>(source, target);
      return result.ToArray();
    }

    private static Pair<string>[] AssociateMappedKeyFields(StoredHierarchyInfo sourceHierarchy, HierarchyInfo targetHierarchy)
    {
      var sourceKeyFields = GetDeclaredFields(sourceHierarchy.Root)
        .Where(field => field.IsPrimaryKey)
        .ToArray();
      var targetKeyFields = GetDeclaredFields(targetHierarchy.Root)
        .Where(field => field.IsPrimaryKey)
        .ToArray();
      if (sourceKeyFields.Length != targetKeyFields.Length)
        return null;
      var pairedKeyFields = JoinFieldsByName(sourceKeyFields, targetKeyFields);
      if (pairedKeyFields.Length != sourceKeyFields.Length)
        return null;
      return AssociateMappedFields(pairedKeyFields);
    }

    private static Pair<string>[] AssociateMappedFields(params Pair<StoredFieldInfo, FieldInfo>[] fieldsToProcess)
    {
      var result = new List<Pair<StoredFieldInfo, FieldInfo>>();
      var tasks = new Queue<Pair<StoredFieldInfo, FieldInfo>>();
      foreach (var task in fieldsToProcess)
        tasks.Enqueue(task);
      while (tasks.Count > 0) {
        var task = tasks.Dequeue();
        var source = task.First;
        var target = task.Second;
        // both fields are primitive -> put to result is types match
        if (source.IsPrimitive && target.IsPrimitive) {
          if (source.ValueType!=target.ValueType.GetFullName())
            return null;
          result.Add(task);
          continue;
        }
        // exactly one of the fields is primitive -> failure
        if (source.IsPrimitive || target.IsPrimitive)
          return null;
        // both fields are not primitive -> recursively process nested fields
        var sourceNestedFields = GetNestedFields(source).ToArray();
        var targetNestedFields = GetNestedFields(target).ToArray();
        if (sourceNestedFields.Length != targetNestedFields.Length)
          return null;
        var pairedNestedFields = JoinFieldsByName(sourceNestedFields, targetNestedFields);
        if (pairedNestedFields.Length != sourceNestedFields.Length)
          return null;
        foreach (var newTask in pairedNestedFields)
          tasks.Enqueue(newTask);
      }
      return result
        .Select(mapping => new Pair<string>(mapping.First.MappingName, mapping.Second.MappingName))
        .ToArray();
    }

    private static InvalidOperationException TypeIsNotFound(string name)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXIsNotFound, name));
    }

    private static InvalidOperationException FieldIsNotFound(string typeName, string fieldName)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExFieldXYIsNotFound, typeName, fieldName));
    }

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));
    }

    private static InvalidOperationException KeysDoNotMatch(string typeOne, string typeTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExKeyOfXDoesNotMatchKeyOfY, typeOne, typeTwo));
    }

    private static InvalidOperationException FieldsDoNotMatch(StoredFieldInfo fieldOne, FieldInfo fieldTwo)
    {
      var nameOne = fieldOne.DeclaringType.UnderlyingType + "." + fieldOne.Name;
      var nameTwo = fieldTwo.DeclaringType.UnderlyingType.GetFullName() + "." + fieldTwo.Name;
      return new InvalidOperationException(string.Format(
        Strings.ExStructureOfFieldXDoesNotMatchStructureOfFieldY, nameOne, nameTwo));
    }

    private static InvalidOperationException TypeIsNotInHierarchy(string type)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXMustBelongToHierarchy, type));
    }

    #endregion
  }
}
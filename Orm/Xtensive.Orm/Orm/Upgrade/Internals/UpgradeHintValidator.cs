// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class UpgradeHintValidator : IUpgradeHintValidator
  {
    private readonly StoredDomainModel currentModel;
    private readonly StoredDomainModel extractedModel;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping;

    public void Validate(NativeTypeClassifier<UpgradeHint> hintsToValidate)
    {
      ValidateRenameTypeHints(hintsToValidate.GetItems<RenameTypeHint>());
      ValidateRemoveTypeHints(hintsToValidate.GetItems<RemoveTypeHint>());
      ValidateRenameFieldHints(hintsToValidate.GetItems<RenameFieldHint>());
      ValidateRemoveFieldHints(hintsToValidate.GetItems<RemoveFieldHint>().Where(h => !h.IsExplicit));
      ValidateCopyFieldHints(hintsToValidate.GetItems<CopyFieldHint>());
    }

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        var newTypeName = hint.NewType.GetFullName();
        var oldTypeName = hint.OldType;
        // Checking that types exists in models
        if (!currentModel.Types.Any(type => type.UnderlyingType==newTypeName))
          throw TypeNotFound(hint.NewType.GetFullName());
        if (!extractedModel.Types.Any(type => type.UnderlyingType==oldTypeName))
          throw TypeNotFound(hint.OldType);
        // Each original type should be used only once
        // Each result type should be used only once
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
        // Both target and source fields should exist
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType == targetTypeName);
        if (targetType==null)
          throw TypeNotFound(targetTypeName);
        if (!reverseTypeMapping.TryGetValue(targetType, out var sourceType))
          throw TypeNotFoundInStorageModel(targetTypeName, hint.OldFieldName);
        var sourceTypeName = sourceType.UnderlyingType;
        if (sourceType.GetField(hint.OldFieldName)==null)
          throw FieldNotFound(sourceTypeName, hint.OldFieldName);
        if (targetType.GetField(hint.NewFieldName)==null)
          throw FieldNotFound(targetTypeName, hint.NewFieldName);
        // Each source field should be used only once
        // Each destination field should be used only once
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
        // Checking source type and field
        var sourceTypeName = hint.SourceType;
        var sourceType = extractedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);
        if (!sourceType.AllFields.Any(field => field.Name==hint.SourceField))
          throw FieldNotFound(sourceTypeName, hint.SourceField);
        // Checking destination type and field
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType == targetTypeName);
        if (targetType==null)
          throw TypeNotFound(targetTypeName);
        if (!targetType.AllFields.Any(field => field.Name==hint.TargetField))
          throw FieldNotFound(targetTypeName, hint.TargetField);
      }
    }

    private void ValidateRemoveFieldHints(IEnumerable<RemoveFieldHint> hints)
    {
      foreach (var hint in hints) {
        // Checking source type and field
        var sourceTypeName = hint.Type;
        var sourceType = extractedModel.Types.SingleOrDefault(type => type.UnderlyingType == sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);

        // Handling structure field
        if (hint.Field.Contains(".", StringComparison.Ordinal)) {
          StoredFieldInfo storedField;
          string[] path = hint.Field.Split('.');
          var fields = sourceType.AllFields;
          string fieldName = string.Empty;
          for (int i = 0; i < path.Length; i++) {
            fieldName += string.IsNullOrEmpty(fieldName) ? path[i] : "." + path[i];
            string parameter = fieldName;
            storedField = fields.SingleOrDefault(field => field.Name==parameter);
            if (storedField==null)
              throw FieldNotFound(sourceTypeName, hint.Field);
            fields = storedField.Fields;
          }
        }
        else if (!sourceType.AllFields.Any(field => field.Name==hint.Field))
          throw FieldNotFound(sourceTypeName, hint.Field);
      }
    }

    private void ValidateRemoveTypeHints(IEnumerable<RemoveTypeHint> hints)
    {
      foreach (var hint in hints) {
        // Checking source type
        var sourceTypeName = hint.Type;
        var sourceType = extractedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);
      }
    }

    private static InvalidOperationException TypeNotFound(string name) =>
      new InvalidOperationException(string.Format(Strings.ExTypeXIsNotFound, name));

    private static InvalidOperationException TypeNotFoundInStorageModel(string typeName, string oldFieldName) =>
      new InvalidOperationException(string.Format(Strings.ExTypeXWhichContainsRenamingFieldYDoesntExistInStorageModel, typeName, oldFieldName));

    private static InvalidOperationException FieldNotFound(string typeName, string fieldName) =>
      new InvalidOperationException(string.Format(Strings.ExFieldXYIsNotFound, typeName, fieldName));

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo) =>
      new InvalidOperationException(string.Format(Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));

    public UpgradeHintValidator(StoredDomainModel currentModel, StoredDomainModel extractedModel, Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping, Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping)
    {
      this.currentModel = currentModel;
      this.extractedModel = extractedModel;
      this.typeMapping = typeMapping;
      this.reverseTypeMapping = reverseTypeMapping;
    }
  }
}
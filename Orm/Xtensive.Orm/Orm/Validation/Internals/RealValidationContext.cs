// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  internal sealed class RealValidationContext : ValidationContext
  {
    private Dictionary<Entity, Pair<IEnumerable<Pair<bool, FieldInfo>>, EntityErrorInfo>> entitiesToValidate;

    public override void Reset()
    {
      entitiesToValidate = null;
    }

    public override void Validate(Entity target)
    {
      var result = ValidateAndGetErrors(target);

      if (result.Count > 0)
        throw new ValidationFailedException(GetErrorMessage(ValidationReason.UserRequest)) {
          ValidationErrors = new List<EntityErrorInfo> {new EntityErrorInfo(target, result)}
        };
    }

    public override void ValidateSetAttempt(Entity target, FieldInfo field, object value)
    {
      if (!field.HasImmediateValidators)
        return;

      var isChanged = target.GetFieldAccessor(field).AreSameValues(target.GetFieldValue(field), value);

      foreach (var validator in field.Validators.Where(v => v.IsImmediate && ((!isChanged && v.ValidateOnlyIfModified) || (!v.ValidateOnlyIfModified)))) {
        var result = validator.Validate(target, value);
        if (result.IsError)
          throw new ArgumentException(result.ErrorMessage, "value");
      }
    }

    public override void RegisterForValidation(Entity target)
    {
      RegisterForValidation(target, null);
    }

    public override void Validate(ValidationReason reason)
    {
      var errors = ValidateAndGetErrors(reason);
      if (errors.Count > 0)
        throw new ValidationFailedException(GetErrorMessage(reason)) {ValidationErrors = errors};
    }

    public override IList<ValidationResult> ValidateAndGetErrors(Entity target)
    {
      var result = new List<ValidationResult>();
      GetValidationErrors(target, entitiesToValidate[target], result);
      if (result.Count==0)
        return EmptyValidationResultCollection;
      var lockedResult = result.AsReadOnly();
      var errorInfo = new EntityErrorInfo(target, lockedResult);
      RegisterForValidation(target, errorInfo);
      return lockedResult;
    }

    public override IList<ValidationResult> ValidateOnceAndGetErrors(Entity target)
    {
      Pair<IEnumerable<Pair<bool, FieldInfo>>, EntityErrorInfo> fieldValidationData;
      if (entitiesToValidate != null && entitiesToValidate.TryGetValue(target, out fieldValidationData))
        return fieldValidationData.Second.Errors;

      return ValidateAndGetErrors(target);
    }

    public override IList<EntityErrorInfo> ValidateAndGetErrors()
    {
      return ValidateAndGetErrors(null);
    }

    private IList<EntityErrorInfo> ValidateAndGetErrors(ValidationReason? reason)
    {
      var errors = new List<EntityErrorInfo>();
      GetValidationErrors(errors, reason);
      return errors.Count==0 ? EmptyEntityErrorCollection : errors.AsReadOnly();
    }

    private void RegisterForValidation(Entity target, EntityErrorInfo previousStatus)
    {
      if (!target.TypeInfo.HasValidators)
        return;

      if (entitiesToValidate==null)
        entitiesToValidate = new Dictionary<Entity, Pair<IEnumerable<Pair<bool, FieldInfo>>, EntityErrorInfo>>();

      var fieldPermanenceMap = target.TypeInfo.Fields.Select(field => new Pair<bool, FieldInfo>(IsFieldChanged(target, field), field)).ToList();

      entitiesToValidate[target] = new Pair<IEnumerable<Pair<bool, FieldInfo>>, EntityErrorInfo>(fieldPermanenceMap, previousStatus);
    }

    private void GetValidationErrors(List<EntityErrorInfo> output, ValidationReason? validationReason = null)
    {
      if (entitiesToValidate==null)
        return;

      var currentEntityErrors = new List<ValidationResult>();
      var entitiesToProcess = entitiesToValidate;
      entitiesToValidate = null;

      foreach (var entity in entitiesToProcess.Keys)
        if (entity.CanBeValidated) {
          GetValidationErrors(entity, entitiesToProcess[entity], currentEntityErrors, validationReason);
          if (currentEntityErrors.Count > 0) {
            var errorInfo = new EntityErrorInfo(entity, currentEntityErrors.AsReadOnly());
            RegisterForValidation(entity, errorInfo);
            output.Add(errorInfo);
            currentEntityErrors = new List<ValidationResult>();
          }
        }
    }

    private void GetValidationErrors(Entity target, Pair<IEnumerable<Pair<bool, FieldInfo>>, EntityErrorInfo> fieldValidationData, List<ValidationResult> output, ValidationReason? validationReason = null)
    {
      foreach (var fieldPermanencePair in fieldValidationData.First) {
        if (!fieldPermanencePair.Second.HasValidators)
          continue;

        var value = target.GetFieldValue(fieldPermanencePair.Second);
        foreach (var validator in fieldPermanencePair.Second.Validators) {
          if (validator.ValidateOnlyIfModified) {
            if (validator.SkipOnTransactionCommit) {
              if (!fieldPermanencePair.First)
                continue;
            }
            else {
              if (validationReason==null || validationReason==ValidationReason.UserRequest)
                continue;
              if (validationReason==ValidationReason.Commit)
                if (!fieldPermanencePair.First)
                  continue;
            }
          }

          if (validationReason.HasValue && validationReason.Value==ValidationReason.Commit && validator.SkipOnTransactionCommit)
            continue;
          var result = validator.Validate(target, value);
          if (result.IsError)
            output.Add(result);
        }
      }

      foreach (var validator in target.TypeInfo.Validators) {
        var result = validator.Validate(target);
        if (result.IsError)
          output.Add(result);
      }
    }

    private bool IsFieldChanged(Entity target, FieldInfo field)
    {
      var difTuple = target.State.DifferentialTuple;
      if (difTuple==null)
        return false;
      return difTuple.IsChanged(field.MappingInfo.Offset);
    }

    private string GetErrorMessage(ValidationReason reason)
    {
      switch (reason) {
      case ValidationReason.UserRequest:
        return Strings.ExValidationFailed;
      case ValidationReason.Commit:
        return Strings.ExCanNotCommitATransactionEntitiesValidationFailed;
      default:
        throw new ArgumentOutOfRangeException("reason");
      }
    }
  }
}
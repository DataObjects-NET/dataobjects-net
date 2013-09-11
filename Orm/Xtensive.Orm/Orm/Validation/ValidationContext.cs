// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  internal sealed class ValidationContext
  {
    private HashSet<Entity> entitiesToValidate;

    public void Reset()
    {
      entitiesToValidate = null;
    }

    public void Validate(Entity target)
    {
      var result = ValidateEntity(target);
      if (result!=null)
        throw new ValidationFailedException(GetErrorMessage(ValidationReason.UserRequest)) {
          ValidationErrors = new List<EntityErrorInfo> {result}
        };
    }

    public EntityErrorInfo ValidateAndGetErrors(Entity target)
    {
      return ValidateEntity(target);
    }

    public void ValidateAll(ValidationReason reason)
    {
      var errors = GetValidationErrors();
      if (errors.Count > 0)
        throw new ValidationFailedException(GetErrorMessage(reason)) {
          ValidationErrors = errors
        };
    }

    public IList<EntityErrorInfo> ValidateAllAndGetErrors()
    {
      return GetValidationErrors();
    }

    public void EnqueueValidation(Entity target)
    {
      if (!target.TypeInfo.HasValidators)
        return;
      if (entitiesToValidate==null)
        entitiesToValidate = new HashSet<Entity>();
      entitiesToValidate.Add(target);
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

    private List<EntityErrorInfo> GetValidationErrors()
    {
      var errors = new List<EntityErrorInfo>();

      if (entitiesToValidate==null)
        return errors;

      var entitiesToProcess = entitiesToValidate;
      entitiesToValidate = null;

      foreach (var entity in entitiesToProcess)
        if (entity.CanBeValidated) {
          var result = ValidateEntity(entity);
          if (result!=null)
            errors.Add(result);
        }

      return errors;
    }

    private EntityErrorInfo ValidateEntity(Entity target)
    {
      EntityErrorInfo errorInfo = null;

      foreach (var field in target.TypeInfo.Fields) {
        if (!field.HasValidators)
          continue;
        var value = target.GetFieldValue(field);
        foreach (var validator in field.Validators) {
          var result = validator.Validate(target, value);
          ProcessValidationResult(target, result, ref errorInfo);
        }
      }

      if (errorInfo==null) {
        foreach (var validator in target.TypeInfo.Validators) {
          var result = validator.Validate(target);
          ProcessValidationResult(target, result, ref errorInfo);
        }
      }

      if (errorInfo!=null)
        EnqueueValidation(target);

      return errorInfo;
    }

    private void ProcessValidationResult(Entity target, ValidationResult result, ref EntityErrorInfo errorInfo)
    {
      if (result.IsError) {
        if (errorInfo==null)
          errorInfo = new EntityErrorInfo(target);
        errorInfo.Errors.Add(result);
      }
    }
  }
}

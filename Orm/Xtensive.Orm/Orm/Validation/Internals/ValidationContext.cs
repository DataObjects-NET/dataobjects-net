// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  internal abstract class ValidationContext
  {
    protected static IList<EntityErrorInfo> EmptyEntityErrorCollection = new List<EntityErrorInfo>().AsReadOnly();

    protected static IList<ValidationResult> EmptyValidationResultCollection = new List<ValidationResult>().AsReadOnly();

    public abstract void Reset();

    public abstract void Validate(ValidationReason reason);

    public abstract void Validate(Entity target);

    public abstract IList<EntityErrorInfo> ValidateAndGetErrors();

    public abstract IList<ValidationResult> ValidateAndGetErrors(Entity target);

    public abstract IList<ValidationResult> ValidateOnceAndGetErrors(Entity target);

    public abstract void ValidateSetAttempt(Entity target, FieldInfo field, object value);

    public abstract void RegisterForValidation(Entity target);
  }
}

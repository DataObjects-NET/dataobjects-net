// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.11

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validator that invokes <see cref="EntitySetBase.OnValidate"/>.
  /// </summary>
  public sealed class EntitySetFieldValidator : PropertyValidator
  {
    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      try {
        var entitySet = (EntitySetBase) fieldValue;
        EntitySetBase.ExecuteOnValidate(entitySet);
      }
      catch(Exception exception) {
        return Error(exception, fieldValue);
      }
      return Success();
    }

    public override IPropertyValidator CreateNew()
    {
      return new EntitySetFieldValidator();
    }
  }
}
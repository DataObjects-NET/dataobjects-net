// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.11

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validator that invokes <see cref="Persistent.OnValidate"/>
  /// for <see cref="Entity"/> derived types.
  /// </summary>
  public sealed class EntityValidator : ObjectValidator
  {
    public override ValidationResult Validate(Entity target)
    {
      try {
        Persistent.ExecuteOnValidate(target);
      }
      catch(Exception exception) {
        return Error(exception);
      }
      return Success();
    }

    public override IObjectValidator CreateNew()
    {
      return new EntityValidator();
    }
  }
}
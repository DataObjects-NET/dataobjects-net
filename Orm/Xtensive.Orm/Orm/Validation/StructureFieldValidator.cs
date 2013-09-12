// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.11

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validator and invokes <see cref="Persistent.OnValidate"/>
  /// for <see cref="Structure"/> derived types.
  /// </summary>
  public sealed class StructureFieldValidator : PropertyValidator
  {
    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      try {
        var structure = (Structure) fieldValue;
        Persistent.ExecuteOnValidate(structure);
      }
      catch(Exception exception) {
        return Error(exception, fieldValue);
      }
      return Success();
    }

    public override IPropertyValidator CreateNew()
    {
      return new StructureFieldValidator();
    }
  }
}
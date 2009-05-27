// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures property value is not null.
  /// </summary>
  [Serializable]
  public class NotNullConstraint : PropertyConstraintAspect
  {
    public override bool IsSupported(Type valueType)
    {
      return true;
    }

    public override bool IsValid(object value)
    {
      return value != null;
    }

    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueCanNotBeNull;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using PostSharp.Aspects.Dependencies;
using Xtensive.Aspects;
using Xtensive.Storage.Validation.Resources;

namespace Xtensive.Storage.Validation
{
  /// <summary>
  /// Ensures that date value is in the past.
  /// </summary>
  [Serializable]
  [ProvideAspectRole(StandardRoles.Validation)]
  [AspectRoleDependency(AspectDependencyAction.Commute, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(InconsistentRegionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public sealed class PastConstraint : PropertyConstraintAspect
  {
    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      if (value==null)
        return true;
      return (DateTime) value <= DateTime.Now;
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType == typeof (DateTime)
        || valueType == typeof (DateTime?);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueMustBeInThePast;
    }
  }
}
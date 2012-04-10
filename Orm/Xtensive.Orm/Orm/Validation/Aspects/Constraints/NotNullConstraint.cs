// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using PostSharp.Aspects.Dependencies;
using Xtensive.Aspects;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures property value is not <see langword="null" />.
  /// </summary>
  [Serializable]
  [ProvideAspectRole(StandardRoles.Validation)]
  [AspectRoleDependency(AspectDependencyAction.Commute, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(InconsistentRegionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public sealed class NotNullConstraint : PropertyConstraintAspect
  {
    
    public override bool CheckValue(object value)
    {
      return value != null;
    }

    
    public override bool IsSupported(Type valueType)
    {
      return true;
    }

    
    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueCanNotBeNull;
    }
  }
}
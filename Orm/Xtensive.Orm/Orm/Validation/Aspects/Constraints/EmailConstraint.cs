// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using System.Text.RegularExpressions;
using PostSharp.Aspects.Dependencies;
using Xtensive.Aspects;
using Xtensive.Orm.Validation.Resources;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that email address is in correct format.
  /// </summary>
  [Serializable]
  [ProvideAspectRole(StandardRoles.Validation)]
  [AspectRoleDependency(AspectDependencyAction.Commute, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(InconsistentRegionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public sealed class EmailConstraint : PropertyConstraintAspect
  {
    private const string EmailPattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    private Regex emailRegex;

    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      string stringValue = (string) value;
      return
        string.IsNullOrEmpty(stringValue) ||
          emailRegex.IsMatch(stringValue);
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof(string);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueFormatIsIncorrect;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      emailRegex = new Regex(EmailPattern);
    }
  }
}
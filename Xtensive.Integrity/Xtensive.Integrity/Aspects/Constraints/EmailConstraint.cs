// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures that email address is in correct format.
  /// </summary>
  [Serializable]
  public class EmailConstraint : PropertyConstraintAspect
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
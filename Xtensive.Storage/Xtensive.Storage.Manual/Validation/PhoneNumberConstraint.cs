// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.28

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using PostSharp.Aspects.Dependencies;
using Xtensive.Aspects;
using Xtensive.Storage.Validation;

namespace Xtensive.Storage.Manual.Validation
{
  [Serializable]
  [ProvideAspectRole(StandardRoles.Validation)]
  [AspectRoleDependency(AspectDependencyAction.Commute, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(InconsistentRegionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public class PhoneNumberConstraint : PropertyConstraintAspect
  {
    private const string PhoneNumberPattern = "^[2-9]\\d{2}-\\d{3}-\\d{4}$";

    [NonSerialized]
    private Regex phoneNumberRegex;

    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof(string);
    }

    public override bool CheckValue(object value)
    {
      string phoneNumber = (string) value;
      return
        string.IsNullOrEmpty(phoneNumber) ||
        phoneNumberRegex.IsMatch(phoneNumber);
    }

    protected override string GetDefaultMessage()
    {
      return "Phone number is incorrect";
    }

    protected override void Initialize()
    {
      base.Initialize();
      phoneNumberRegex = new Regex(PhoneNumberPattern, RegexOptions.Compiled);
    }
  }
}
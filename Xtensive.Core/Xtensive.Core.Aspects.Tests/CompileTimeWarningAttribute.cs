// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.31

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Aspects.Tests
{
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Property | MulticastTargets.Method | MulticastTargets.Field)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class CompileTimeWarningAttribute : CompoundAspect
  {
    public override bool CompileTimeValidate(object element)
    {
      var member = element as MemberInfo;
      var method = element as MethodInfo;
      var ctor = element as ConstructorInfo;
      var property = element as PropertyInfo;
      var field = element as FieldInfo;
      if (member!=null)
        AspectHelper.ValidateMemberType(this, SeverityType.Warning,
          member, true, MemberTypes.Event);
      return false;
    }

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
    }


    // Constructors

    public CompileTimeWarningAttribute()
    {
    }
  }
}
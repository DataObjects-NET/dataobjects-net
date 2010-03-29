// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.31

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Property | MulticastTargets.Method | MulticastTargets.Field)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class CompileTimeWarningAttribute : Aspect
  {
    public override bool CompileTimeValidate(object element)
    {
      var method = element as MethodInfo;
      var ctor = element as ConstructorInfo;
      var property = element as PropertyInfo;
      var field = element as FieldInfo;
      if (property!=null) {
        AspectHelper.ValidateMemberType(this, SeverityType.Warning,
          property, true, MemberTypes.Field);
        AspectHelper.ValidatePropertyAccessor(this, SeverityType.Warning,
          property, true, null, true);
        AspectHelper.ValidatePropertyAccessor(this, SeverityType.Warning,
          property, true, true, false);
        AspectHelper.ValidateBaseType(this, SeverityType.Warning,
          property.DeclaringType, true, typeof(ILockable));

        ConstructorInfo constructor;
        AspectHelper.ValidateConstructor(this, SeverityType.Warning,
          property.DeclaringType, true, BindingFlags.Public, new Type[] {}, out constructor);
      }
      if (field!=null) {
        AspectHelper.ValidateFieldAttributes(this, SeverityType.Warning,
          field, true, FieldAttributes.Static);
      }
      if (method!=null) {
        MethodInfo methodInfo;
        AspectHelper.ValidateMethod(this, SeverityType.Warning,
          method.DeclaringType, true, BindingFlags.Public, typeof(void), "NotExistingMethod", new Type[] {}, out methodInfo);
        AspectHelper.ValidateMethodAttributes(this, SeverityType.Warning,
          method, true, MethodAttributes.Abstract);
        AspectHelper.ValidateMemberAttribute<CompilerGeneratedAttribute>(this, SeverityType.Warning,
          method, true, AttributeSearchOptions.Default);
      }
      return false;
    }


    // Constructors

    public CompileTimeWarningAttribute()
    {
    }
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.27

using System;
using System.Reflection;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Helpers
{
  public static class ContextBoundAspectValidator<TContext>
    where TContext : class, IContext
  {
    public static bool CompileTimeValidate(Attribute originalAspect, MethodBase method)
    {
      foreach (var attribute in method.GetAttributes<SuppressActivationAttribute>(false))
        if (attribute.ContextType == typeof(TContext))
          return false;

      if (!AspectHelper.ValidateMemberType(originalAspect, SeverityType.Error,
        method, false, MemberTypes.Constructor))
        return false;
      if (!AspectHelper.ValidateMethodAttributes(originalAspect, SeverityType.Error,
        method, false, MethodAttributes.Static))
        return false;
      if (!AspectHelper.ValidateBaseType(originalAspect, SeverityType.Error,
        method.DeclaringType, true, typeof(IContextBound<TContext>)))
        return false;

      return true;
    }
  }
}

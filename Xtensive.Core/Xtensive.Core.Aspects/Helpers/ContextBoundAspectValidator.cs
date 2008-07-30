// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.27

using System;
using System.Reflection;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Helpers
{
  public static class ContextBoundAspectValidator<TContext>
    where TContext : class, IContext
  {
    public static bool CompileTimeValidate(Attribute aspect, MethodBase method)
    {
      foreach (var attribute in method.GetAttributes<SuppressActivationAttribute>(false))
        if (attribute.ContextType == typeof(TContext))
          return false;

      var methodInfo = method as MethodInfo;
      if (methodInfo == null) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExCannotBeAppliedToConstructor,
          aspect.GetType().GetShortName(), 
          method.DeclaringType.GetShortName());
        return false;
      }

      if (methodInfo.IsStatic) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExCannotBeAppliedToStaticMember,
          aspect.GetType().GetShortName(), 
          method.DeclaringType.GetShortName());
        return false;
      }

      Type type = methodInfo.DeclaringType;
      if (!typeof(IContextBound<TContext>).IsAssignableFrom(type)) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExNoBaseTypeOrInterface,
          aspect.GetType().GetShortName(), 
          method.DeclaringType.GetShortName(), 
          typeof(IContextBound<TContext>).GetShortName());
        return false;
      }

      return true;
    }
  }
}

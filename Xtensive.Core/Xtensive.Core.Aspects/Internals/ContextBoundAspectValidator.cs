// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.27

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects;
using Xtensive.Core.Resources;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Internals
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
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCannotBeAppliedToConstructor",
            new object[] { aspect.GetType().Name, method.DeclaringType.FullName });
        return false;
      }

      if (methodInfo.IsStatic) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCannotBeAppliedToStaticMember",
            new object[] { aspect.GetType().Name, method.DeclaringType.FullName });
        return false;
      }

      Type type = methodInfo.DeclaringType;
      if (!typeof(IContextBound<TContext>).IsAssignableFrom(type)) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExTypeShouldImplementXxx",
            new object[] { aspect.GetType().Name, method.DeclaringType.FullName, typeof(IContextBound<TContext>).FullName });
        return false;
      }

      return true;
    }
  }
}

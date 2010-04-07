// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.16

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Disposing;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Aspects
{
  /// <summary>
  /// Wraps a method of property body into so-called "inconsistent region"
  /// using <see cref="ValidationContextBase.OpenInconsistentRegion"/> method.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  [Serializable]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(PropertyConstraintAspect))]
  public sealed class InconsistentRegionAttribute : OnMethodBoundaryAspect
  {
    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error, method.DeclaringType, true, typeof(IValidationAware)))
        return false;

      if (!(method is ConstructorInfo)) {
        var methodInfo = method as MethodInfo;
        if (methodInfo.IsGetter()) {
          // Property getter is marked as [InconsistentRegion]
          ErrorLog.Write(SeverityType.Warning, AspectMessageType.AspectPossiblyMissapplied,
            AspectHelper.FormatType(GetType()),
            AspectHelper.FormatMember(methodInfo.DeclaringType, methodInfo));
        }
      }

      return true;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionArgs eventArgs)
    {
      var validatable = (IValidationAware)eventArgs.Instance;
      var context = validatable.Context;
      var region = context.OpenInconsistentRegion();
      eventArgs.MethodExecutionTag = region;
    }

    /// <inheritdoc/>
    public override void OnSuccess(MethodExecutionArgs eventArgs)
    {
      var region = (InconsistentRegion) eventArgs.MethodExecutionTag;
      region.Complete();
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionArgs eventArgs)
    {
      var region = (InconsistentRegion) eventArgs.MethodExecutionTag;
      region.DisposeSafely();
    }
  }
}
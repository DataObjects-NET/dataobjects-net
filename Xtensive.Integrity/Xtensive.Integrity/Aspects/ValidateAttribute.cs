// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.16

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Aspects
{
  /// <summary>
  /// Provides validation feature for methods it is applied on
  /// by <see cref="ValidationContextBase"/> activation.
  /// </summary>
  // [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ValidateAttribute : OnMethodBoundaryAspect, 
    ILaosWeavableAspect
  {
    public bool IsConsistent { get; set; }

    int ILaosWeavableAspect.AspectPriority { get { return (int)IntegrityAspectPriority.Validate; } }
    
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (!ContextBoundAspectValidator<ValidationContextBase>.CompileTimeValidate(this, method))
        return false;

      MethodInfo methodInfo = method as MethodInfo;      

      if (methodInfo.IsSpecialName && methodInfo.Name.StartsWith(WellKnown.GetterPrefix)) {
        string expectedPropertyName = methodInfo.Name.Remove(0, WellKnown.GetterPrefix.Length);

        // This is getter; let's check if it is explicitely marked as [Validate]
        PropertyInfo propertyInfo = methodInfo.DeclaringType.UnderlyingSystemType.GetProperty(expectedPropertyName, 
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo!=null && propertyInfo.GetAttribute<AtomicAttribute>(false)!=null)
          // Property itself is marked as [Validate]
          return false;
        
        // Property getter is marked as [Validate]
        ErrorLog.Write(SeverityType.Warning, AspectMessageType.AspectPossiblyMissapplied,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(methodInfo.DeclaringType, methodInfo));
      }

      return true;
    }

    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      var validationContextBound = (IContextBound<ValidationContextBase>)eventArgs.Instance;
      var validationScope        = (ValidationScope)validationContextBound.ActivateContext();
      eventArgs.MethodExecutionTag = validationScope;
      if (!IsConsistent)
        if (validationScope==null)
          // Scope is already active
          eventArgs.MethodExecutionTag = ValidationScope.CurrentContext.InconsistentRegion();
        else
          // Active scope is just created
          eventArgs.MethodExecutionTag = new Disposable<IDisposable, IDisposable>(
            validationScope, 
            validationScope.Context.InconsistentRegion(), 
            (disposing, d1, d2) => {
              d2.DisposeSafely();
              d1.DisposeSafely();
            });
    }

    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionEventArgs eventArgs)
    {
      var d = (IDisposable)eventArgs.MethodExecutionTag;
      d.DisposeSafely();
    }


    // Constructors

    public ValidateAttribute()
      : this(true)
    {
    }

    public ValidateAttribute(bool isConsistent)
    {
      IsConsistent = isConsistent;
    }
  }
}
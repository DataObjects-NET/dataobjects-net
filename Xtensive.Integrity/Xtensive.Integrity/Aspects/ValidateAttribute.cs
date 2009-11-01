// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Aspects.Internals;
using Xtensive.Core.Helpers;
using Xtensive.Integrity.Aspects.Internals;
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
      Type type = methodInfo.DeclaringType;

      if (methodInfo.IsSpecialName && methodInfo.Name.StartsWith("get_")) {
        // This is getter; let's check if it is explicitely marked as [Validate]
        PropertyInfo propertyInfo = methodInfo.DeclaringType.UnderlyingSystemType.GetProperty(methodInfo.Name.Remove(0, 4), 
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo!=null && Attribute.GetCustomAttribute(propertyInfo, typeof(ValidateAttribute), false)!=null)
          // Property itself is marked as [Validate]
          return false;
        AspectsMessageSource.Instance.Write(SeverityType.Warning, "AspectExPossiblyMissapplied",
            new object[] { this.GetType().Name, method.DeclaringType.FullName, method.Name });
      }

      return true;
    }

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
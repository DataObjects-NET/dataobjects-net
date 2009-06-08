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
using Xtensive.Core.Notifications;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  /// <summary>
  /// Internally applied by <see cref="ChangerAttribute"/>.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class NotifyOnChangeAspect : OnMethodBoundaryAspect, ILaosWeavableAspect
  {
    private ChangerAttribute changerAttribute;

    int ILaosWeavableAspect.AspectPriority { get { return (int)CoreAspectPriority.Changer; } }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (!AspectHelper.ValidateMemberType(changerAttribute, SeverityType.Error,
        method, false, MemberTypes.Constructor))
        return false;
      if (!AspectHelper.ValidateMethodAttributes(changerAttribute, SeverityType.Error,
        method, false, MethodAttributes.Static))
        return false;

      var methodInfo = method as MethodInfo;
      if (methodInfo.IsGetter()) {
        // This is getter; let's check if it is explicitely marked as [Changer]
        var propertyInfo = methodInfo.GetProperty();
        if (propertyInfo!=null && propertyInfo.GetAttribute<ChangerAttribute>(
          AttributeSearchOptions.Default)!=null)
          // Property itself is marked as [Changer]
          return false;
        ErrorLog.Write(SeverityType.Warning, AspectMessageType.AspectPossiblyMissapplied,
          AspectHelper.FormatType(changerAttribute.GetType()), 
          AspectHelper.FormatMember(method.DeclaringType, method));
      }

      return true;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      var composed = eventArgs.Instance as IComposed<IChangeNotifier>;
      if (composed==null)
        // TODO: AY: Support custom IChangeNotifier implementations?
        return;
      var implementation = (ChangeNotifier)composed.GetImplementation(eventArgs.InstanceCredentials);
      if (!implementation.IsEnabled)
        return;

      var notifyEventArgs = new ChangeNotifierEventArgs(eventArgs);
      eventArgs.MethodExecutionTag = notifyEventArgs;
      implementation.OnChanging(notifyEventArgs);
      implementation.IsEnabled = false;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionEventArgs eventArgs)
    {
      var composed = eventArgs.Instance as IComposed<IChangeNotifier>;
      if (composed==null)
        return;
      var implementation = (ChangeNotifier)composed.GetImplementation(eventArgs.InstanceCredentials);
      var notifyEventArgs = (ChangeNotifierEventArgs)eventArgs.MethodExecutionTag;
      if (notifyEventArgs==null)
        return; // There was no notification
      implementation.IsEnabled = true;
      implementation.OnChanged(notifyEventArgs);
    }

    
    // Constructors

    internal NotifyOnChangeAspect(ChangerAttribute changerAttribute)
    {
      this.changerAttribute = changerAttribute;
    }
  }
}
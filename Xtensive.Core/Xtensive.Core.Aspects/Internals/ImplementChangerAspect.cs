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
using Xtensive.Core.Resources;

namespace Xtensive.Core.Aspects.Internals
{
  [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementChangerAspect : OnMethodBoundaryAspect, ILaosWeavableAspect
  {
    private ChangerAttribute changerAttribute;

    int ILaosWeavableAspect.AspectPriority { get { return (int)CoreAspectPriority.Changer; } }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(MethodBase method)
    {
      Type originalAspectType = typeof (ChangerAttribute);

      MethodInfo methodInfo = method as MethodInfo;
      if (methodInfo == null) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCannotBeAppliedToConstructor",
            new object[] { originalAspectType.Name, method.DeclaringType.FullName });
        return false;
      }

      if (methodInfo.IsStatic) {
        AspectsMessageSource.Instance.Write(SeverityType.Error, "AspectExCannotBeAppliedToStaticMember",
            new object[] { originalAspectType.Name, method.DeclaringType.FullName });
        return false;
      }

      if (methodInfo.IsSpecialName && methodInfo.Name.StartsWith("get_")) {
        // This is getter; let's check if it is explicitely marked as [Changer]
        PropertyInfo propertyInfo = methodInfo.DeclaringType.UnderlyingSystemType.GetProperty(methodInfo.Name.Remove(0, 4), 
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo!=null && Attribute.GetCustomAttribute(propertyInfo, originalAspectType, false)!=null)
          // Property itself is marked as [Changer]
          return false;
        AspectsMessageSource.Instance.Write(SeverityType.Warning, "AspectExPossiblyMissapplied",
            new object[] { originalAspectType.Name, method.DeclaringType.FullName, method.Name });
      }

      return true;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      IComposed<IChangeNotifier> composed = eventArgs.Instance as IComposed<IChangeNotifier>;
      if (composed==null)
        // TODO: AY: Support custom IChangeNotifier implementations?
        return;
      ChangeNotifierImplementation implementation = (ChangeNotifierImplementation)composed.GetImplementation(eventArgs.InstanceCredentials);
      if (!implementation.IsEnabled)
        return;

      ChangeNotifierEventArgs notifyEventArgs = new ChangeNotifierEventArgs(eventArgs);
      eventArgs.MethodExecutionTag = notifyEventArgs;
      implementation.OnChanging(notifyEventArgs);
      implementation.IsEnabled = false;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionEventArgs eventArgs)
    {
      IComposed<IChangeNotifier> composed = eventArgs.Instance as IComposed<IChangeNotifier>;
      if (composed==null)
        return;
      ChangeNotifierImplementation implementation = (ChangeNotifierImplementation)composed.GetImplementation(eventArgs.InstanceCredentials);
      ChangeNotifierEventArgs notifyEventArgs = (ChangeNotifierEventArgs)eventArgs.MethodExecutionTag;
      if (notifyEventArgs==null)
        return; // There was no notification
      implementation.IsEnabled = true;
      implementation.OnChanged(notifyEventArgs);
    }

    
    // Constructors

    internal ImplementChangerAspect(ChangerAttribute changerAttribute)
    {
      this.changerAttribute = changerAttribute;
    }
  }
}